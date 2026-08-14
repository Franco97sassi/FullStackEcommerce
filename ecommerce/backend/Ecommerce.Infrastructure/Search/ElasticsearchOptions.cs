namespace Ecommerce.Infrastructure.Search;

public sealed class ElasticsearchOptions
{
    public const string SectionName = "Elasticsearch";
    public bool Enabled { get; set; }
    public string Endpoint { get; set; } = "http://localhost:9200";
    public string ProductIndex { get; set; } = "ecommerce-products-v1";
    public string ConsumerGroup { get; set; } = "ecommerce-search-indexer-v1";
}
