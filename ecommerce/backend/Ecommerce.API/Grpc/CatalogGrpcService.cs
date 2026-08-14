using Ecommerce.Grpc.Contracts.Catalog.V1;
using Ecommerce.Infrastructure.Persistence;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Grpc;

public sealed class CatalogGrpcService(EcommerceDbContext db) : CatalogService.CatalogServiceBase
{
    public override async Task<ProductResponse> GetProduct(GetProductRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProductId, out var id))
            throw new GrpcContractException(StatusCode.InvalidArgument, "product_id no es válido.");

        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.IsActive, context.CancellationToken)
            ?? throw new GrpcContractException(StatusCode.NotFound, "Producto no encontrado.");
        return Map(product);
    }

    public override async Task<ProductsResponse> GetProducts(GetProductsRequest request, ServerCallContext context)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize == 0 ? 20 : request.PageSize, 1, 100);
        var ids = request.ProductIds.Select(value => Guid.TryParse(value, out var id) ? id : Guid.Empty).Where(x => x != Guid.Empty).ToArray();
        var query = db.Products.AsNoTracking().Where(x => x.IsActive);
        if (ids.Length > 0) query = query.Where(x => ids.Contains(x.Id));

        var response = new ProductsResponse { TotalCount = await query.CountAsync(context.CancellationToken) };
        response.Products.AddRange((await query.OrderBy(x => x.Name).Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(context.CancellationToken)).Select(Map));
        return response;
    }

    private static ProductResponse Map(Ecommerce.Domain.Entities.Product product) => new()
    {
        ProductId = product.Id.ToString(), Name = product.Name, Slug = product.Slug,
        Description = product.Description ?? string.Empty, Price = product.Price.ToString(System.Globalization.CultureInfo.InvariantCulture),
        AvailableQuantity = product.Stock, CategoryId = product.CategoryId.ToString()
    };
}
