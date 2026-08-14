using System.Net.Http.Json;
using System.Text.Json;
using Confluent.Kafka;
using Ecommerce.Application.Events;
using Ecommerce.Infrastructure.Messaging;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ecommerce.Infrastructure.Search;

public sealed class ProductSearchIndexer(IServiceScopeFactory scopeFactory, IHttpClientFactory clients,
    IOptions<KafkaOptions> kafka, IOptions<ElasticsearchOptions> elasticsearch,
    ILogger<ProductSearchIndexer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var es = elasticsearch.Value;
        if (!es.Enabled) return;
        try { await BootstrapIndexAsync(stoppingToken); }
        catch (HttpRequestException exception) { logger.LogWarning(exception, "Initial Elasticsearch bootstrap failed; Kafka changes will still be consumed."); }
        var consumerConfig = new ConsumerConfig { BootstrapServers = kafka.Value.BootstrapServers, GroupId = es.ConsumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest, EnableAutoCommit = false };
        using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        consumer.Subscribe(kafka.Value.ProductChangedTopic);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var record = consumer.Consume(stoppingToken);
                var change = JsonSerializer.Deserialize<ProductChangedEvent>(record.Message.Value, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
                await IndexAsync(change, stoppingToken);
                consumer.Commit(record);
            }
            catch (ConsumeException exception) { logger.LogWarning(exception, "Product indexer could not consume Kafka event."); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }

    private async Task BootstrapIndexAsync(CancellationToken token)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();
        var productIds = await db.Products.AsNoTracking().Select(product => product.Id).ToListAsync(token);
        foreach (var productId in productIds)
            await IndexAsync(new ProductChangedEvent(Guid.Empty, productId, "updated", DateTime.UtcNow), token);
        logger.LogInformation("Elasticsearch bootstrap indexed {ProductCount} products from PostgreSQL.", productIds.Count);
    }

    private async Task IndexAsync(ProductChangedEvent change, CancellationToken token)
    {
        var client = clients.CreateClient("elasticsearch");
        var path = $"{elasticsearch.Value.ProductIndex}/_doc/{change.ProductId}";
        if (change.Operation == "deleted") { using var deleted = await client.DeleteAsync(path, token); return; }
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();
        var document = await db.Products.AsNoTracking().Where(p => p.Id == change.ProductId).Select(p => new {
            p.Id, p.Name, p.Slug, p.Description, p.Price, p.Stock, p.IsActive,
            CategoryName = p.Category.Name, CategorySlug = p.Category.Slug, CategoryActive = p.Category.IsActive
        }).SingleOrDefaultAsync(token);
        if (document is null) return;
        using var response = await client.PutAsJsonAsync(path, document, token);
        response.EnsureSuccessStatusCode();
    }
}
