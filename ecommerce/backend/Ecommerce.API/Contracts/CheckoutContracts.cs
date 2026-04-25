using System.ComponentModel.DataAnnotations;
namespace Ecommerce.API.Contracts;

 
public sealed record CreateOrderItemRequest(
    [property: Required(ErrorMessage = "El ProductId es obligatorio.")]
    Guid ProductId,

    [property: Range(1, 1000, ErrorMessage = "La cantidad debe estar entre 1 y 1000.")]
    int Quantity); 
public sealed record ShippingAddressRequest(
    [property: Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [property: StringLength(120, MinimumLength = 3, ErrorMessage = "El nombre completo debe tener entre 3 y 120 caracteres.")]
    string FullName,

    [property: Required(ErrorMessage = "La dirección es obligatoria.")]
    [property: StringLength(180, MinimumLength = 5, ErrorMessage = "La dirección debe tener entre 5 y 180 caracteres.")]
    string AddressLine1,

    [property: Required(ErrorMessage = "La ciudad es obligatoria.")]
    [property: StringLength(100, MinimumLength = 2, ErrorMessage = "La ciudad debe tener entre 2 y 100 caracteres.")]
    string City,

    [property: Required(ErrorMessage = "La provincia/estado es obligatorio.")]
    [property: StringLength(100, MinimumLength = 2, ErrorMessage = "La provincia/estado debe tener entre 2 y 100 caracteres.")]
    string State,

    [property: Required(ErrorMessage = "El código postal es obligatorio.")]
    [property: StringLength(20, MinimumLength = 3, ErrorMessage = "El código postal debe tener entre 3 y 20 caracteres.")]
    string PostalCode,

    [property: Required(ErrorMessage = "El país es obligatorio.")]
    [property: StringLength(100, MinimumLength = 2, ErrorMessage = "El país debe tener entre 2 y 100 caracteres.")]
    string Country,

    [property: Phone(ErrorMessage = "El teléfono no tiene un formato válido.")]
    string? Phone);

public sealed record CreateOrderRequest(
    [property: Required(ErrorMessage = "Debes enviar al menos un item.")]
    [property: MinLength(1, ErrorMessage = "Debes enviar al menos un item.")]
    IReadOnlyList<CreateOrderItemRequest> Items,

    [property: Required(ErrorMessage = "La dirección de envío es obligatoria.")]
    ShippingAddressRequest ShippingAddress);

public sealed record ShippingAddressResponse(
    string FullName,
    string AddressLine1,
    string City,
    string State,
    string PostalCode,
    string Country,
    string? Phone);

public sealed record OrderItemResponse(
    Guid ProductId,
    string ProductName,
    string ProductSlug,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);

public sealed record OrderSummaryResponse(
    Guid OrderId,
    DateTime CreatedAtUtc,
    string Status,
    decimal Total,
    int ItemCount);

public sealed record CreateOrderResponse(
    Guid OrderId,
    DateTime CreatedAtUtc,
    string Status,
    decimal Total,
        ShippingAddressResponse ShippingAddress,
     IReadOnlyList<OrderItemResponse> Items);

public sealed record OrderDetailResponse(
    Guid OrderId,
    DateTime CreatedAtUtc,
    string Status,
    decimal Total,
        ShippingAddressResponse ShippingAddress,
     IReadOnlyList<OrderItemResponse> Items);