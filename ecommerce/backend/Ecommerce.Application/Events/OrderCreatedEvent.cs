namespace Ecommerce.Application.Events;

public sealed record OrderCreatedEvent(
    Guid EventId,
    Guid OrderId,
    Guid UserId,
    DateTime OccurredAtUtc,
    string Status,
    decimal TotalAmount,
    IReadOnlyList<OrderCreatedItem> Items);

public sealed record OrderCreatedItem(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);
