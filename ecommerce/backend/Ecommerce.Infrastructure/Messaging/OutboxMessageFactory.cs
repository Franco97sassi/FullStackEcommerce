using System.Text.Json;
using Ecommerce.Application.Events;

namespace Ecommerce.Infrastructure.Messaging;

public static class OutboxMessageFactory
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static OutboxMessage From(OrderCreatedEvent domainEvent, string topic)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);

        return new OutboxMessage
        {
            Id = domainEvent.EventId,
            Topic = topic,
            MessageKey = domainEvent.OrderId.ToString(),
            EventType = nameof(OrderCreatedEvent),
            Payload = JsonSerializer.Serialize(domainEvent, SerializerOptions),
            OccurredAtUtc = domainEvent.OccurredAtUtc
        };
    }

    public static OutboxMessage From(ProductChangedEvent domainEvent, string topic)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);

        return new OutboxMessage
        {
            Id = domainEvent.EventId,
            Topic = topic,
            MessageKey = domainEvent.ProductId.ToString(),
            EventType = nameof(ProductChangedEvent),
            Payload = JsonSerializer.Serialize(domainEvent, SerializerOptions),
            OccurredAtUtc = domainEvent.OccurredAtUtc
        };
    }
}
