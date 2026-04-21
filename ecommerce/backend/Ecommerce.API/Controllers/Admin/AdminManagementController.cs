using Ecommerce.API.Contracts;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminManagementController(EcommerceDbContext dbContext) : ControllerBase
{
    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyList<AdminCategoryResponse>>> GetCategories(CancellationToken cancellationToken = default)
    {
        var categories = await dbContext.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new AdminCategoryResponse(c.Id, c.Name, c.Slug, c.Description, c.IsActive, c.Products.Count))
            .ToListAsync(cancellationToken);

        return Ok(categories);
    }

    [HttpPost("categories")]
    public async Task<ActionResult<AdminCategoryResponse>> CreateCategory([FromBody] AdminCategoryRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Slug))
        {
            return BadRequest("Name y slug son obligatorios.");
        }

        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();
        var exists = await dbContext.Categories.AnyAsync(c => c.Slug == normalizedSlug, cancellationToken);
        if (exists)
        {
            return Conflict("Ya existe una categoría con ese slug.");
        }

        var category = new Category
        {
            Name = request.Name.Trim(),
            Slug = normalizedSlug,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsActive = request.IsActive
        };

        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, new AdminCategoryResponse(category.Id, category.Name, category.Slug, category.Description, category.IsActive, 0));
    }

    [HttpPut("categories/{id:guid}")]
    public async Task<ActionResult<AdminCategoryResponse>> UpdateCategory(Guid id, [FromBody] AdminCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await dbContext.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (category is null)
        {
            return NotFound();
        }

        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();
        var duplicate = await dbContext.Categories.AnyAsync(c => c.Id != id && c.Slug == normalizedSlug, cancellationToken);
        if (duplicate)
        {
            return Conflict("Ya existe una categoría con ese slug.");
        }

        category.Name = request.Name.Trim();
        category.Slug = normalizedSlug;
        category.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        category.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new AdminCategoryResponse(category.Id, category.Name, category.Slug, category.Description, category.IsActive, category.Products.Count));
    }

    [HttpDelete("categories/{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await dbContext.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (category is null)
        {
            return NotFound();
        }

        if (category.Products.Count > 0)
        {
            return BadRequest("No se puede eliminar una categoría con productos asociados.");
        }

        dbContext.Categories.Remove(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpGet("products")]
    public async Task<ActionResult<IReadOnlyList<AdminProductResponse>>> GetProducts(CancellationToken cancellationToken = default)
    {
        var products = await dbContext.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .Select(p => new AdminProductResponse(p.Id, p.Name, p.Slug, p.Description, p.Price, p.Stock, p.IsActive, p.CategoryId, p.Category.Name))
            .ToListAsync(cancellationToken);

        return Ok(products);
    }

    [HttpPost("products")]
    public async Task<ActionResult<AdminProductResponse>> CreateProduct([FromBody] AdminProductRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Price < 0 || request.Stock < 0)
        {
            return BadRequest("Price y stock deben ser mayores o iguales a cero.");
        }

        var categoryExists = await dbContext.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            return BadRequest("La categoría indicada no existe.");
        }

        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();
        var exists = await dbContext.Products.AnyAsync(p => p.Slug == normalizedSlug, cancellationToken);
        if (exists)
        {
            return Conflict("Ya existe un producto con ese slug.");
        }

        var product = new Product
        {
            Name = request.Name.Trim(),
            Slug = normalizedSlug,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Price = request.Price,
            Stock = request.Stock,
            IsActive = request.IsActive,
            CategoryId = request.CategoryId
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        var categoryName = await dbContext.Categories
            .Where(c => c.Id == request.CategoryId)
            .Select(c => c.Name)
            .FirstAsync(cancellationToken);

        return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, new AdminProductResponse(product.Id, product.Name, product.Slug, product.Description, product.Price, product.Stock, product.IsActive, product.CategoryId, categoryName));
    }

    [HttpPut("products/{id:guid}")]
    public async Task<ActionResult<AdminProductResponse>> UpdateProduct(Guid id, [FromBody] AdminProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        var category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (category is null)
        {
            return BadRequest("La categoría indicada no existe.");
        }

        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();
        var duplicate = await dbContext.Products.AnyAsync(p => p.Id != id && p.Slug == normalizedSlug, cancellationToken);
        if (duplicate)
        {
            return Conflict("Ya existe un producto con ese slug.");
        }

        product.Name = request.Name.Trim();
        product.Slug = normalizedSlug;
        product.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.IsActive = request.IsActive;
        product.CategoryId = request.CategoryId;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new AdminProductResponse(product.Id, product.Name, product.Slug, product.Description, product.Price, product.Stock, product.IsActive, product.CategoryId, category.Name));
    }

    [HttpPatch("products/{id:guid}/stock")]
    public async Task<ActionResult<AdminProductResponse>> UpdateProductStock(Guid id, [FromBody] UpdateProductStockRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Stock < 0)
        {
            return BadRequest("El stock no puede ser negativo.");
        }

        var product = await dbContext.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        product.Stock = request.Stock;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new AdminProductResponse(product.Id, product.Name, product.Slug, product.Description, product.Price, product.Stock, product.IsActive, product.CategoryId, product.Category.Name));
    }

    [HttpDelete("products/{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpGet("orders")]
    public async Task<ActionResult<IReadOnlyList<AdminOrderResponse>>> GetOrders([FromQuery] string? status, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalized = status.Trim();
            query = query.Where(o => o.Status == normalized);
        }

        var orders = await query
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => new AdminOrderResponse(o.Id, o.CreatedAtUtc, o.Status, o.TotalAmount, o.UserId, o.User.Email, o.Items.Sum(i => i.Quantity)))
            .ToListAsync(cancellationToken);

        return Ok(orders);
    }

    [HttpPatch("orders/{id:guid}/status")]
    public async Task<ActionResult<AdminOrderResponse>> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders.Include(o => o.User).FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return BadRequest("El status es obligatorio.");
        }

        order.Status = request.Status.Trim();
        await dbContext.SaveChangesAsync(cancellationToken);

        var itemCount = await dbContext.OrderItems
            .Where(i => i.OrderId == id)
            .SumAsync(i => i.Quantity, cancellationToken);

        return Ok(new AdminOrderResponse(order.Id, order.CreatedAtUtc, order.Status, order.TotalAmount, order.UserId, order.User.Email, itemCount));
    }

    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyList<AdminUserResponse>>> GetUsers(CancellationToken cancellationToken = default)
    {
        var users = await dbContext.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAtUtc)
            .Select(u => new AdminUserResponse(u.Id, u.FullName, u.Email, u.Role, u.IsActive, u.CreatedAtUtc, u.Orders.Count))
            .ToListAsync(cancellationToken);

        return Ok(users);
    }

    [HttpPatch("users/{id:guid}/role")]
    public async Task<ActionResult<AdminUserResponse>> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleRequest request, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.Include(u => u.Orders).FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Role))
        {
            return BadRequest("El rol es obligatorio.");
        }

        user.Role = request.Role.Trim();
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new AdminUserResponse(user.Id, user.FullName, user.Email, user.Role, user.IsActive, user.CreatedAtUtc, user.Orders.Count));
    }

    [HttpPatch("users/{id:guid}/active")]
    public async Task<ActionResult<AdminUserResponse>> UpdateUserActive(Guid id, [FromBody] UpdateUserActiveRequest request, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.Include(u => u.Orders).FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        user.IsActive = request.IsActive;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new AdminUserResponse(user.Id, user.FullName, user.Email, user.Role, user.IsActive, user.CreatedAtUtc, user.Orders.Count));
    }
}
