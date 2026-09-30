using Papirfly_ArticlesAPI.Models;

namespace Papirfly_ArticlesAPI.Repositories
{
    public interface IArticleRepository
    {
        Article Add(Article article);
        Article? Get(int id);
        IReadOnlyList<Article> Search(string? name, string? category);

        // Replaces the article with the same ArticleId and Version as the expected article with the replacement article.
        bool TryReplace(Article expected, Article replacement);
    }
}
