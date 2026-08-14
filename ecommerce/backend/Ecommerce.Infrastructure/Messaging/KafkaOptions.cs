namespace Ecommerce.Infrastructure.Messaging;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";

    public bool Enabled { get; set; }
    public string BootstrapServers { get; set; } = "localhost:29092";
    public string ClientId { get; set; } = "ecommerce-api";
    public string OrderCreatedTopic { get; set; } = "ecommerce.orders.created.v1";
    public string ProductChangedTopic { get; set; } = "ecommerce.products.changed.v1";
    public int OutboxBatchSize { get; set; } = 50;
    public int OutboxPollingIntervalSeconds { get; set; } = 5;
    public int OutboxMaxRetryDelaySeconds { get; set; } = 300;
}
