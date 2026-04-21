namespace Ecommerce.API.Contracts;

public sealed record AdminCategoryRequest(string Name, string Slug, string? Description, bool IsActive);
public sealed record AdminCategoryResponse(Guid Id, string Name, string Slug, string? Description, bool IsActive, int ProductCount);

public sealed record AdminProductRequest(
    string Name,
    string Slug,
    string? Description,
    decimal Price,
    int Stock,
    bool IsActive,
    Guid CategoryId);

public sealed record AdminProductResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    decimal Price,
    int Stock,
    bool IsActive,
    Guid CategoryId,
    string CategoryName);

public sealed record AdminOrderResponse(
    Guid Id,
    DateTime CreatedAtUtc,
    string Status,
    decimal TotalAmount,
    Guid UserId,
    string UserEmail,
    int ItemCount);

public sealed record UpdateOrderStatusRequest(string Status);

public sealed record AdminUserResponse(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAtUtc,
    int OrdersCount);

public sealed record UpdateUserRoleRequest(string Role);
public sealed record UpdateUserActiveRequest(bool IsActive);
public sealed record UpdateProductStockRequest(int Stock);
