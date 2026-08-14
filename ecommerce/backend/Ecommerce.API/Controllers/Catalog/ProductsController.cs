using Ecommerce.API.Contracts;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Infrastructure.Search;

namespace Ecommerce.API.Controllers.Catalog;

[ApiController]
[Route("api/catalog/products")]
public class ProductsController(EcommerceDbContext dbContext, IProductSearch productSearch) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedProductsResponse>> Get(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 6,
        CancellationToken cancellationToken = default)
    {
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 50);
        var normalizedSearch = search?.Trim().ToLowerInvariant();

        var elasticsearchResult = await productSearch.SearchAsync(search, category, minPrice, maxPrice,
            safePage, safePageSize, cancellationToken);
        if (elasticsearchResult is not null)
        {
            var searchTotalPages = Math.Max(1, (int)Math.Ceiling(elasticsearchResult.Total / (double)safePageSize));
            return Ok(new PagedProductsResponse(elasticsearchResult.Items.Select(p => new ProductResponse(
                p.Id, p.Name, p.Slug, p.Description, p.Price, p.Stock, p.CategoryName, p.CategorySlug)).ToList(),
                safePage, safePageSize, elasticsearchResult.Total, searchTotalPages));
        }

        var query = dbContext.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.Category.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            query = query.Where(p =>
                p.Name.ToLower().Contains(normalizedSearch) ||
                (p.Description != null && p.Description.ToLower().Contains(normalizedSearch)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var normalizedCategory = category.Trim().ToLowerInvariant();
            query = query.Where(p => p.Category.Slug.ToLower() == normalizedCategory);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)safePageSize));
        safePage = Math.Min(safePage, totalPages);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(p => new ProductResponse(
                p.Id,
                p.Name,
                p.Slug,
                p.Description,
                p.Price,
                p.Stock,
                p.Category.Name,
                p.Category.Slug))
            .ToListAsync(cancellationToken);

        return Ok(new PagedProductsResponse(items, safePage, safePageSize, total, totalPages));
    }

    [HttpGet("suggest")]
    public async Task<ActionResult<IReadOnlyList<string>>> Suggest([FromQuery] string search,
        [FromQuery] int size = 8, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(search)) return Ok(Array.Empty<string>());
        var suggestions = await productSearch.SuggestAsync(search.Trim(), Math.Clamp(size, 1, 20), cancellationToken);
        if (suggestions is not null) return Ok(suggestions);
        return Ok(await dbContext.Products.AsNoTracking().Where(p => p.IsActive && p.Name.ToLower().Contains(search.Trim().ToLower()))
            .OrderBy(p => p.Name).Select(p => p.Name).Take(Math.Clamp(size, 1, 20)).ToListAsync(cancellationToken));
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<ProductDetailResponse>> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();

        var product = await dbContext.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.Category.IsActive)
            .Where(p => p.Slug.ToLower() == normalizedSlug)
            .Select(p => new ProductDetailResponse(
                p.Id,
                p.Name,
                p.Slug,
                p.Description,
                p.Price,
                p.Stock,
                p.Category.Name,
                p.Category.Slug))
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }
}
