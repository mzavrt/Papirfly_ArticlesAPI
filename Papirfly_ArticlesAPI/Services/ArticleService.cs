using Papirfly_ArticlesAPI.Models;
using Papirfly_ArticlesAPI.Repositories;

namespace Papirfly_ArticlesAPI.Services
{
    public enum UpdateStatus { Updated, NotFound, Conflict }

    public record UpdateResult(UpdateStatus Status, Article? Article);

    public class ArticleService(IArticleRepository repository)
    {
        public Article Create(ArticleRequest request) => repository.Add(request.ToArticle());

        public Article? Get(int id) => repository.Get(id);

        public IReadOnlyList<Article> Search(string? name, string? category) => repository.Search(name, category);


        public IReadOnlyList<Article> CreateMany(IReadOnlyList<ArticleRequest> requests)
        {
            var articles = requests
                .Select(r => r.ToArticle())
                .ToList();

            return repository.AddMany(articles);
        }

        public UpdateResult Update(int id, ArticleRequest request, int? expectedVersion)
        {
            var current = repository.Get(id);
            if (current is null) return new(UpdateStatus.NotFound, null);

            // Klient poslal verzi, kterou viděl. Pokud už není aktuální, někdo jiný mezitím článek změnil.
            if (expectedVersion is not null && expectedVersion != current.Version)
                return new(UpdateStatus.Conflict, null);

            var updated = request.ToArticle() with { ArticleId = id, Version = current.Version + 1 };

            // Poslední pojistka proti souběhu: pokud se článek změnil až teď, TryReplace selže.
            return repository.TryReplace(current, updated)
                ? new(UpdateStatus.Updated, updated)
                : new(UpdateStatus.Conflict, null);
        }
    }
}
