using System.ComponentModel.DataAnnotations;

namespace Ecommerce.API.Contracts;

public sealed record AdminCategoryRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Slug { get; init; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; init; }

    public bool IsActive { get; init; } = true;
}
 public sealed record AdminCategoryResponse(Guid Id, string Name, string Slug, string? Description, bool IsActive, int ProductCount);

public sealed record AdminProductRequest
{
    [Required]
    [StringLength(160, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(180, MinimumLength = 2)]
    public string Slug { get; init; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; init; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal Price { get; init; }

    [Range(0, int.MaxValue)]
    public int Stock { get; init; }

    public bool IsActive { get; init; } = true;

    [Required]
    public Guid CategoryId { get; init; }
}
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

public sealed record UpdateOrderStatusRequest
{
    [Required]
    [StringLength(40, MinimumLength = 2)]
    public string Status { get; init; } = string.Empty;
}
public sealed record AdminUserResponse(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAtUtc,
    int OrdersCount);

public sealed record UpdateUserRoleRequest
{
    [Required]
    [StringLength(30, MinimumLength = 2)]
    public string Role { get; init; } = string.Empty;
}
public sealed record UpdateUserActiveRequest(bool IsActive);
public sealed record UpdateProductStockRequest
{
    [Range(0, int.MaxValue)]
    public int Stock { get; init; }
}