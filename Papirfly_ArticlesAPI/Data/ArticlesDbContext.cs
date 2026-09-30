using Microsoft.EntityFrameworkCore;
using Papirfly_ArticlesAPI.Models;

namespace Papirfly_ArticlesAPI.Data;

public class ArticlesDbContext(DbContextOptions<ArticlesDbContext> options) : DbContext(options)
{
    public DbSet<Article> Articles => Set<Article>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Article>()
            .HasKey(a => a.ArticleId)
            .IsClustered();
    }
}