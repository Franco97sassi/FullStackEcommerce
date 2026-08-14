using Confluent.Kafka;
using Ecommerce.Infrastructure.Messaging;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<EcommerceDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddOptions<KafkaOptions>()
            .Bind(configuration.GetSection(KafkaOptions.SectionName))
            .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.BootstrapServers),
                "Kafka:BootstrapServers is required when Kafka is enabled.")
            .Validate(options => options.OutboxBatchSize > 0, "Kafka:OutboxBatchSize must be greater than zero.")
            .Validate(options => options.OutboxPollingIntervalSeconds > 0,
                "Kafka:OutboxPollingIntervalSeconds must be greater than zero.")
            .ValidateOnStart();

        services.AddSingleton<IProducer<string, string>>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<KafkaOptions>>().Value;
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = options.BootstrapServers,
                ClientId = options.ClientId,
                Acks = Acks.All,
                EnableIdempotence = true,
                MessageSendMaxRetries = 5
            };

            return new ProducerBuilder<string, string>(producerConfig).Build();
        });
        services.AddHostedService<KafkaOutboxPublisher>();

        return services;
    }
}
