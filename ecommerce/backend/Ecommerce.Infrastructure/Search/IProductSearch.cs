namespace Ecommerce.Infrastructure.Search;

public sealed record ProductSearchHit(Guid Id, string Name, string Slug, string? Description,
    decimal Price, int Stock, string CategoryName, string CategorySlug);

public sealed record ProductSearchResult(IReadOnlyList<ProductSearchHit> Items, int Total);

public interface IProductSearch
{
    Task<ProductSearchResult?> SearchAsync(string? text, string? category, decimal? minPrice,
        decimal? maxPrice, int page, int pageSize, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>?> SuggestAsync(string text, int size, CancellationToken cancellationToken);
}
