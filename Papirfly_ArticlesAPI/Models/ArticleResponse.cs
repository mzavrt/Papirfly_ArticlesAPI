using System.Text.Json.Serialization;

namespace Papirfly_ArticlesAPI.Models
{
    public record ArticleResponse(
     [property: JsonPropertyName("article_id")] int ArticleId,
     [property: JsonPropertyName("name")] string Name,
     [property: JsonPropertyName("description")] string Description,
     [property: JsonPropertyName("category"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Category,
     [property: JsonPropertyName("price")] decimal Price,
     [property: JsonPropertyName("currency"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Currency)
    {
        public static ArticleResponse From(Article a)
            => new(a.ArticleId, a.Name, a.Description, a.Category, a.Price, a.Currency);
    }
}
