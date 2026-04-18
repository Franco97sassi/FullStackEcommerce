using Ecommerce.API.Contracts;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers.Checkout;

[ApiController]
[Route("api/checkout/orders")]
public class OrdersController(EcommerceDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreateOrderResponse>> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null || request.Items is null || request.Items.Count == 0)
        {
            return BadRequest("El carrito no puede estar vacío.");
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

        Console.WriteLine("=== CREATE ORDER DEBUG ===");
        Console.WriteLine($"Connection string: {dbContext.Database.GetConnectionString()}");

        Console.WriteLine("ProductIds recibidos:");
        foreach (var id in productIds)
        {
            Console.WriteLine(id);
        }

        var products = await dbContext.Products
            .Include(product => product.Category)
            .Where(product => product.IsActive)
            .Where(product => product.Category.IsActive)
            .Where(product => productIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, cancellationToken);

        Console.WriteLine("Products encontrados:");
        foreach (var product in products.Values)
        {
            Console.WriteLine($"Id: {product.Id} | Name: {product.Name} | Stock: {product.Stock}");
        }

        var missingIds = productIds
            .Except(products.Keys)
            .ToList();

        if (missingIds.Any())
        {
            Console.WriteLine("MissingIds:");
            foreach (var id in missingIds)
            {
                Console.WriteLine(id);
            }

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
            CreatedAtUtc = DateTime.UtcNow,
            Status = "Confirmed",
            TotalAmount = total,
            Items = orderItems
        };

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);

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

        var response = new CreateOrderResponse(
            order.Id,
            order.CreatedAtUtc,
            order.Status,
            order.TotalAmount,
            responseItems);

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, response);
    }








    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDetailResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await dbContext.Orders
            .AsNoTracking()
            .Include(item => item.Items)
            .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (order is null)
        {
            return NotFound();
        }

        var response = new OrderDetailResponse(
            order.Id,
            order.CreatedAtUtc,
            order.Status,
            order.TotalAmount,
            order.Items
                .Select(item => new OrderItemResponse(
                    item.ProductId,
                    item.Product.Name,
                    item.Product.Slug,
                    item.UnitPrice,
                    item.Quantity,
                    item.LineTotal))
                .ToList());

        return Ok(response);
    }
}
