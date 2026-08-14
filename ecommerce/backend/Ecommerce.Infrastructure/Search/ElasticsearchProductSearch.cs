using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ecommerce.Infrastructure.Search;

public sealed class ElasticsearchProductSearch(HttpClient client, IOptions<ElasticsearchOptions> options,
    ILogger<ElasticsearchProductSearch> logger) : IProductSearch
{
    private readonly ElasticsearchOptions settings = options.Value;

    public async Task<ProductSearchResult?> SearchAsync(string? text, string? category, decimal? minPrice,
        decimal? maxPrice, int page, int pageSize, CancellationToken cancellationToken)
    {
        if (!settings.Enabled) return null;
        var filters = new List<object>
        {
            new { term = new Dictionary<string, object> { ["isActive"] = true } },
            new { term = new Dictionary<string, object> { ["categoryActive"] = true } }
        };
        if (!string.IsNullOrWhiteSpace(category)) filters.Add(new { term = new Dictionary<string, object> { ["categorySlug"] = category.Trim().ToLowerInvariant() } });
        if (minPrice.HasValue || maxPrice.HasValue)
            filters.Add(new { range = new { price = new { gte = minPrice, lte = maxPrice } } });
        object query = string.IsNullOrWhiteSpace(text)
            ? new { @bool = new { filter = filters } }
            : new { @bool = new { must = new[] { new { multi_match = new { query = text, fields = new[] { "name^3", "description", "categoryName" }, fuzziness = "AUTO" } } }, filter = filters } };
        var body = new { from = (page - 1) * pageSize, size = pageSize, query };
        try
        {
            using var response = await client.PostAsJsonAsync($"{settings.ProductIndex}/_search", body, cancellationToken);
            response.EnsureSuccessStatusCode();
            using var json = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken));
            var hits = json.RootElement.GetProperty("hits");
            var total = hits.GetProperty("total").GetProperty("value").GetInt32();
            var items = hits.GetProperty("hits").EnumerateArray().Select(hit =>
                JsonSerializer.Deserialize<ProductSearchHit>(hit.GetProperty("_source"), new JsonSerializerOptions(JsonSerializerDefaults.Web))!).ToList();
            return new ProductSearchResult(items, total);
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException)
        {
            logger.LogWarning(exception, "Elasticsearch query failed; PostgreSQL remains the catalog fallback.");
            return null;
        }
    }

    public async Task<IReadOnlyList<string>?> SuggestAsync(string text, int size, CancellationToken cancellationToken)
    {
        var result = await SearchAsync(text, null, null, null, 1, size, cancellationToken);
        return result?.Items.Select(item => item.Name).Distinct().ToList();
    }
}
