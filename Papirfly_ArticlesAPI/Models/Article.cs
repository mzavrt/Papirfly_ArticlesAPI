using System.Text.Json.Serialization;

namespace Papirfly_ArticlesAPI.Models
{
    public record Article
    {
        public int ArticleId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? Category { get; init; }
        public decimal Price { get; init; }
        public string? Currency { get; init; }
        public int Version { get; init; }
    }
}
