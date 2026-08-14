namespace Ecommerce.Application.Events;

public sealed record ProductChangedEvent(
    Guid EventId,
    Guid ProductId,
    string Operation,
    DateTime OccurredAtUtc);
