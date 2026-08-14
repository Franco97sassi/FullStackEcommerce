using Confluent.Kafka;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ecommerce.Infrastructure.Messaging;

public sealed class KafkaOutboxPublisher(
    IServiceScopeFactory scopeFactory,
    IProducer<string, string> producer,
    IOptions<KafkaOptions> options,
    ILogger<KafkaOutboxPublisher> logger) : BackgroundService
{
    private readonly KafkaOptions kafkaOptions = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!kafkaOptions.Enabled)
        {
            logger.LogInformation("Kafka outbox publisher is disabled.");
            return;
        }

        logger.LogInformation(
            "Kafka outbox publisher started for bootstrap servers {BootstrapServers}.",
            kafkaOptions.BootstrapServers);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(kafkaOptions.OutboxPollingIntervalSeconds));

        do
        {
            await PublishPendingMessagesAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    internal async Task PublishPendingMessagesAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();
        var now = DateTime.UtcNow;

        var messages = await dbContext.OutboxMessages
            .Where(message => message.ProcessedAtUtc == null)
            .Where(message => message.NextAttemptAtUtc == null || message.NextAttemptAtUtc <= now)
            .OrderBy(message => message.OccurredAtUtc)
            .Take(kafkaOptions.OutboxBatchSize)
            .ToListAsync(cancellationToken);

        foreach (var outboxMessage in messages)
        {
            try
            {
                var headers = new Headers
                {
                    { "event-id", System.Text.Encoding.UTF8.GetBytes(outboxMessage.Id.ToString()) },
                    { "event-type", System.Text.Encoding.UTF8.GetBytes(outboxMessage.EventType) },
                    { "occurred-at-utc", System.Text.Encoding.UTF8.GetBytes(outboxMessage.OccurredAtUtc.ToString("O")) }
                };

                await producer.ProduceAsync(
                    outboxMessage.Topic,
                    new Message<string, string>
                    {
                        Key = outboxMessage.MessageKey,
                        Value = outboxMessage.Payload,
                        Headers = headers
                    },
                    cancellationToken);

                outboxMessage.ProcessedAtUtc = DateTime.UtcNow;
                outboxMessage.NextAttemptAtUtc = null;
                outboxMessage.LastError = null;
            }
            catch (ProduceException<string, string> exception)
            {
                RegisterFailure(outboxMessage, exception.Error.Reason);
                logger.LogWarning(
                    exception,
                    "Could not publish outbox message {MessageId} to topic {Topic}. Attempt {Attempt}.",
                    outboxMessage.Id,
                    outboxMessage.Topic,
                    outboxMessage.Attempts);
            }
            catch (KafkaException exception)
            {
                RegisterFailure(outboxMessage, exception.Error.Reason);
                logger.LogWarning(
                    exception,
                    "Kafka failed while publishing outbox message {MessageId}. Attempt {Attempt}.",
                    outboxMessage.Id,
                    outboxMessage.Attempts);
            }
        }

        if (messages.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private void RegisterFailure(OutboxMessage message, string error)
    {
        message.Attempts++;
        var retryDelaySeconds = Math.Min(
            Math.Pow(2, Math.Min(message.Attempts, 10)),
            kafkaOptions.OutboxMaxRetryDelaySeconds);
        message.NextAttemptAtUtc = DateTime.UtcNow.AddSeconds(retryDelaySeconds);
        message.LastError = error.Length <= 2000 ? error : error[..2000];
    }

}
