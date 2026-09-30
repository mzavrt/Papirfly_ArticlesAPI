using Microsoft.EntityFrameworkCore;
using Papirfly_ArticlesAPI.Data;
using Papirfly_ArticlesAPI.Models;

namespace Papirfly_ArticlesAPI.Repositories;

public class SqlArticleRepository(ArticlesDbContext context) : IArticleRepository
{
    /// <summary>
    /// Adds a new article to the database and returns it with the database-generated ID.
    /// </summary>
    public Article Add(Article article)
    {
        // Set initial version to 1 for new articles
        var newArticle = article with { Version = 1 };

        context.Articles.Add(newArticle);
        context.SaveChanges();

        // Reload to get the database-generated ArticleId
        context.Entry(newArticle).Reload();
        return newArticle;
    }

    public IReadOnlyList<Article> AddMany(IEnumerable<Article> articles)
    {
        var articleList = articles
       .Select(a => a with { Version = 1 })
       .ToList();

        context.Articles.AddRange(articleList);
        context.SaveChanges();

        return articleList.AsReadOnly();
    }

    /// <summary>
    /// Retrieves an article by its ID.
    /// </summary>
    public Article? Get(int id) =>
        context.Articles.FirstOrDefault(a => a.ArticleId == id);

    /// <summary>
    /// Searches for articles by name (partial match, case-insensitive) and/or category (exact match).
    /// </summary>
    public IReadOnlyList<Article> Search(string? name, string? category)
    {
        var query = context.Articles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var nameLower = name.ToLower();
            query = query.Where(a => a.Name.ToLower().Contains(nameLower));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(a => a.Category == category);
        }

        return query.ToList().AsReadOnly();
    }

    /// <summary>
    /// Replaces an article using optimistic concurrency control.
    /// Only replaces if the ArticleId and Version match the expected article.
    /// Increments the Version on successful replacement.
    /// </summary>
    public bool TryReplace(Article expected, Article replacement)
    {
        var existing = context.Articles.FirstOrDefault(a =>
            a.ArticleId == expected.ArticleId &&
            a.Version == expected.Version);

        if (existing is null)
        {
            return false; // Article not found or version mismatch (concurrency conflict)
        }

        // Increment version for optimistic concurrency control
        var updated = replacement with { Version = expected.Version + 1 };

        context.Entry(existing).CurrentValues.SetValues(updated);
        context.SaveChanges();

        return true;
    }
}