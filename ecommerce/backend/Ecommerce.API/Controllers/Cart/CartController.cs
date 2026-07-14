using System.Security.Claims;
using Ecommerce.API.Contracts;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers.Cart;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController(EcommerceDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CartResponse>> Get(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var cart = await EnsureCartAsync(userId.Value, cancellationToken);
        return Ok(ToCartResponse(cart));
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartResponse>> AddItem([FromBody] AddCartItemRequest request, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var product = await dbContext.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive && p.Category.IsActive, cancellationToken);

        if (product is null)
        {
            return BadRequest("El producto no existe o no está disponible.");
        }

        var cart = await EnsureCartAsync(userId.Value, cancellationToken);
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);

        if (existingItem is null)
        {
            if (request.Quantity > product.Stock)
            {
                return BadRequest($"Stock insuficiente para '{product.Name}'.");
            }

            cart.Items.Add(new CartItem
            {
                ProductId = product.Id,
                Quantity = request.Quantity
            });
        }
        else
        {
            var nextQuantity = existingItem.Quantity + request.Quantity;
            if (nextQuantity > product.Stock)
            {
                return BadRequest($"Stock insuficiente para '{product.Name}'.");
            }

            existingItem.Quantity = nextQuantity;
        }

        cart.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        var updatedCart = await GetCartWithProductsAsync(cart.Id, cancellationToken);
        return Ok(ToCartResponse(updatedCart));
    }

    [HttpPatch("items/{productId:guid}")]
    public async Task<ActionResult<CartResponse>> UpdateItem(Guid productId, [FromBody] UpdateCartItemRequest request, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var cart = await EnsureCartAsync(userId.Value, cancellationToken);
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

        if (item is null)
        {
            return NotFound();
        }

        var product = await dbContext.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive && p.Category.IsActive, cancellationToken);

        if (product is null)
        {
            return BadRequest("El producto no existe o no está disponible.");
        }

        if (request.Quantity > product.Stock)
        {
            return BadRequest($"Stock insuficiente para '{product.Name}'.");
        }

        item.Quantity = request.Quantity;
        cart.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToCartResponse(cart));
    }

    [HttpDelete("items/{productId:guid}")]
    public async Task<ActionResult<CartResponse>> RemoveItem(Guid productId, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var cart = await EnsureCartAsync(userId.Value, cancellationToken);
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

        if (item is null)
        {
            return NotFound();
        }

        dbContext.CartItems.Remove(item);
        cart.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToCartResponse(cart));
    }

    [HttpDelete]
    public async Task<IActionResult> Clear(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var cart = await EnsureCartAsync(userId.Value, cancellationToken);
        if (cart.Items.Count == 0)
        {
            return NoContent();
        }

        dbContext.CartItems.RemoveRange(cart.Items);
        cart.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private Guid? GetUserId()
    {
        var rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(rawUserId, out var userId))
        {
            return userId;
        }

        return null;
    }
 
    private async Task<Ecommerce.Domain.Entities.Cart> GetCartWithProductsAsync(Guid cartId, CancellationToken cancellationToken)
    {
        return await dbContext.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstAsync(c => c.Id == cartId, cancellationToken);
    }

    private async Task<Ecommerce.Domain.Entities.Cart> EnsureCartAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cart = await dbContext.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart is not null)
        {
            return cart;
        }

        cart = new Ecommerce.Domain.Entities.Cart
        {
            UserId = userId,
            UpdatedAtUtc = DateTime.UtcNow
        };

        dbContext.Carts.Add(cart);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await dbContext.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstAsync(c => c.Id == cart.Id, cancellationToken);
    }

    private static CartResponse ToCartResponse(Ecommerce.Domain.Entities.Cart cart)
    {
        var items = cart.Items
            .OrderBy(i => i.Product.Name)
            .Select(i => new CartItemResponse(
                i.ProductId,
                i.Product.Slug,
                i.Product.Name,
                i.Product.Price,
                i.Quantity,
                i.Product.Stock,
                i.Product.Price * i.Quantity))
            .ToList();

        return new CartResponse(
            cart.Id,
            cart.UserId,
            cart.UpdatedAtUtc,
            items,
            items.Sum(i => i.Quantity),
            items.Sum(i => i.LineTotal));
    }
}
