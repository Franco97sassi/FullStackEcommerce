namespace Ecommerce.API.Contracts;

public sealed record CreateOrderItemRequest(Guid ProductId, int Quantity);

public sealed record CreateOrderRequest(IReadOnlyList<CreateOrderItemRequest> Items);

public sealed record OrderItemResponse(
    Guid ProductId,
    string ProductName,
    string ProductSlug,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);

public sealed record CreateOrderResponse(
    Guid OrderId,
    DateTime CreatedAtUtc,
    string Status,
    decimal Total,
    IReadOnlyList<OrderItemResponse> Items);

public sealed record OrderDetailResponse(
    Guid OrderId,
    DateTime CreatedAtUtc,
    string Status,
    decimal Total,
    IReadOnlyList<OrderItemResponse> Items);
