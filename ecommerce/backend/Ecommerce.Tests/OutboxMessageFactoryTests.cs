using System.Text.Json;
using Ecommerce.Application.Events;
using Ecommerce.Infrastructure.Messaging;

namespace Ecommerce.Tests;

public sealed class OutboxMessageFactoryTests
{
    [Fact]
    public void Order_created_event_is_serialized_with_stable_routing_metadata()
    {
        var eventId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var occurredAtUtc = DateTime.UtcNow;
        var domainEvent = new OrderCreatedEvent(
            eventId,
            orderId,
            Guid.NewGuid(),
            occurredAtUtc,
            "Confirmed",
            42.50m,
            [new OrderCreatedItem(Guid.NewGuid(), 2, 21.25m, 42.50m)]);

        var message = OutboxMessageFactory.From(domainEvent, "ecommerce.orders.created.v1");
        var payload = JsonSerializer.Deserialize<OrderCreatedEvent>(
            message.Payload,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Equal(eventId, message.Id);
        Assert.Equal(orderId.ToString(), message.MessageKey);
        Assert.Equal(nameof(OrderCreatedEvent), message.EventType);
        Assert.Equal("ecommerce.orders.created.v1", message.Topic);
        Assert.NotNull(payload);
        Assert.Equal(domainEvent.EventId, payload.EventId);
        Assert.Equal(domainEvent.OrderId, payload.OrderId);
        Assert.Equal(domainEvent.TotalAmount, payload.TotalAmount);
        Assert.Equal(domainEvent.Items.Single(), payload.Items.Single());
        Assert.Null(message.ProcessedAtUtc);
        Assert.Equal(0, message.Attempts);
    }
}
