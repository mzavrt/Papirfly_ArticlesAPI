
using System.Text.Json.Serialization;

namespace Papirfly_ArticlesAPI.Models
{
    // Disallow = if client sends unknown properties (article_id which is not allowed in the request), it's a 400 error.
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public class ArticleRequest
    {
        [JsonPropertyName("name")]
        public required string Name { get; init; }

        [JsonPropertyName("description")]
        public required string Description { get; init; }

        [JsonPropertyName("category")]
        public string? Category { get; init; }

        [JsonPropertyName("price")]
        public required decimal Price { get; init; }

        [JsonPropertyName("currency")]
        public string? Currency { get; init; }

        public Article ToArticle() => new Article
        {
            Name = Name,
            Description = Description,
            Category = Category,
            Price = Price,
            Currency = string.IsNullOrWhiteSpace(Currency) ? null : Currency,
            ArticleId = 0,     
            Version = 0         
        };
    }
}
