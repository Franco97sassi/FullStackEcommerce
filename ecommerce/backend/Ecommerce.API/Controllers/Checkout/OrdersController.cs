using System.Security.Claims;
using Ecommerce.Application.Events;
using Ecommerce.API.Contracts;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Messaging;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ecommerce.API.Controllers.Checkout;

[ApiController]
[Route("api/checkout/orders")]
[Authorize]
public class OrdersController(EcommerceDbContext dbContext, IOptions<KafkaOptions> kafkaOptions) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreateOrderResponse>> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        if (request is null || request.Items is null || request.Items.Count == 0)
        {
            return BadRequest("El carrito no puede estar vacío.");
        }

        if (!IsValidAddress(request.ShippingAddress))
        {
            return BadRequest("La dirección de envío es obligatoria.");
        }

        if (request.Items.Any(item => item.Quantity <= 0))
        {
            return BadRequest("Todas las cantidades deben ser mayores a 0.");
        }

        var groupedItems = request.Items
            .GroupBy(item => item.ProductId)
            .Select(group => new CreateOrderItemRequest(group.Key, group.Sum(i => i.Quantity)))
            .ToList();

        var productIds = groupedItems
            .Select(item => item.ProductId)
            .ToList();

        var products = await dbContext.Products
            .Include(product => product.Category)
            .Where(product => product.IsActive)
            .Where(product => product.Category.IsActive)
            .Where(product => productIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, cancellationToken);

        var missingIds = productIds
            .Except(products.Keys)
            .ToList();

        if (missingIds.Any())
        {
            return BadRequest(new
            {
                message = "Uno o más productos no existen o no están disponibles.",
                missingProductIds = missingIds
            });
        }

        foreach (var item in groupedItems)
        {
            var product = products[item.ProductId];

            if (product.Stock < item.Quantity)
            {
                return BadRequest(new
                {
                    message = $"Stock insuficiente para '{product.Name}'.",
                    productId = product.Id,
                    requestedQuantity = item.Quantity,
                    availableStock = product.Stock
                });
            }
        }

        var orderItems = groupedItems.Select(item =>
        {
            var product = products[item.ProductId];
            var lineTotal = product.Price * item.Quantity;

            product.Stock -= item.Quantity;

            return new OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                LineTotal = lineTotal
            };
        }).ToList();

        var total = orderItems.Sum(item => item.LineTotal);

        var order = new Order
        {
            UserId = userId.Value,
            CreatedAtUtc = DateTime.UtcNow,
            Status = "Confirmed",
            TotalAmount = total,
            ShippingFullName = request.ShippingAddress.FullName.Trim(),
            ShippingAddressLine1 = request.ShippingAddress.AddressLine1.Trim(),
            ShippingCity = request.ShippingAddress.City.Trim(),
            ShippingState = request.ShippingAddress.State.Trim(),
            ShippingPostalCode = request.ShippingAddress.PostalCode.Trim(),
            ShippingCountry = request.ShippingAddress.Country.Trim(),
            ShippingPhone = string.IsNullOrWhiteSpace(request.ShippingAddress.Phone)
                ? null
                : request.ShippingAddress.Phone.Trim(),
            Items = orderItems
        };

        dbContext.Orders.Add(order);
        var orderCreatedEvent = new OrderCreatedEvent(
            Guid.NewGuid(),
            order.Id,
            order.UserId,
            order.CreatedAtUtc,
            order.Status,
            order.TotalAmount,
            order.Items.Select(item => new OrderCreatedItem(
                item.ProductId,
                item.Quantity,
                item.UnitPrice,
                item.LineTotal)).ToList());
        dbContext.OutboxMessages.Add(OutboxMessageFactory.From(
            orderCreatedEvent,
            kafkaOptions.Value.OrderCreatedTopic));
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = ToCreateOrderResponse(order, products);

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, response);
    }

    [HttpGet("me")]
    public async Task<ActionResult<IReadOnlyList<OrderSummaryResponse>>> GetMine(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var orders = await dbContext.Orders
            .AsNoTracking()
            .Where(order => order.UserId == userId.Value)
            .OrderByDescending(order => order.CreatedAtUtc)
            .Select(order => new OrderSummaryResponse(
                order.Id,
                order.CreatedAtUtc,
                order.Status,
                order.TotalAmount,
                order.Items.Sum(item => item.Quantity)))
            .ToListAsync(cancellationToken);

        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDetailResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var order = await dbContext.Orders
            .AsNoTracking()
            .Include(item => item.Items)
            .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (order is null)
        {
            return NotFound();
        }

        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin && order.UserId != userId.Value)
        {
            return Forbid();
        }

        return Ok(ToOrderDetailResponse(order));
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

    private static bool IsValidAddress(ShippingAddressRequest address)
    {
        return
            !string.IsNullOrWhiteSpace(address.FullName) &&
            !string.IsNullOrWhiteSpace(address.AddressLine1) &&
            !string.IsNullOrWhiteSpace(address.City) &&
            !string.IsNullOrWhiteSpace(address.State) &&
            !string.IsNullOrWhiteSpace(address.PostalCode) &&
            !string.IsNullOrWhiteSpace(address.Country);
    }

    private static ShippingAddressResponse ToAddressResponse(Order order)
    {
        return new ShippingAddressResponse(
            order.ShippingFullName,
            order.ShippingAddressLine1,
            order.ShippingCity,
            order.ShippingState,
            order.ShippingPostalCode,
            order.ShippingCountry,
            order.ShippingPhone);
    }

    private static CreateOrderResponse ToCreateOrderResponse(Order order, IReadOnlyDictionary<Guid, Product> products)
    {
        var responseItems = order.Items
            .Select(item =>
            {
                var product = products[item.ProductId];
                return new OrderItemResponse(
                    item.ProductId,
                    product.Name,
                    product.Slug,
                    item.UnitPrice,
                    item.Quantity,
                    item.LineTotal);
            })
            .ToList();

        return new CreateOrderResponse(
            order.Id,
            order.CreatedAtUtc,
            order.Status,
            order.TotalAmount,
            ToAddressResponse(order),
            responseItems);
    }

    private static OrderDetailResponse ToOrderDetailResponse(Order order)
    {
        var items = order.Items
            .Select(item => new OrderItemResponse(
                item.ProductId,
                item.Product.Name,
                item.Product.Slug,
                item.UnitPrice,
                item.Quantity,
                item.LineTotal))
            .ToList();

        return new OrderDetailResponse(
            order.Id,
            order.CreatedAtUtc,
            order.Status,
            order.TotalAmount,
            ToAddressResponse(order),
            items);
    }
}
