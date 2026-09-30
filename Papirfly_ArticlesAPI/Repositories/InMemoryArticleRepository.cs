using System.Collections.Concurrent;
using Papirfly_ArticlesAPI.Models;

namespace Papirfly_ArticlesAPI.Repositories
{
    public class InMemoryArticleRepository : IArticleRepository
    {
        private readonly ConcurrentDictionary<int, Article> _items = new();

        private int _lastId;

        public Article Add(Article article)
        {
            // Interlocked.Increment increments the number atomically: two concurrent requests will never get the same ID.
            var id = Interlocked.Increment(ref _lastId);
            var stored = article with { ArticleId = id, Version = 1 };
            _items[id] = stored;
            return stored;
        }

        public Article? Get(int id) => _items.TryGetValue(id, out var article) ? article : null;

        public IReadOnlyList<Article> Search(string? name, string? category)
        {
            IEnumerable<Article> query = _items.Values;

            // name: partial match, case-insensitive.
            if (!string.IsNullOrEmpty(name))
                query = query.Where(a => a.Name.Contains(name, StringComparison.OrdinalIgnoreCase));

            // category: exact match required (and case-sensitive, as per README).
            if (!string.IsNullOrEmpty(category))
                query = query.Where(a => string.Equals(a.Category, category, StringComparison.Ordinal));

            return query.OrderBy(a => a.ArticleId).ToList();
        }

        // TryUpdate = "replace the value only if it is still what I expect". This happens atomically in a single step.
        public bool TryReplace(Article expected, Article replacement)
            => _items.TryUpdate(expected.ArticleId, replacement, expected);
    }
}