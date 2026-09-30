using Papirfly_ArticlesAPI.Models;
using Papirfly_ArticlesAPI.Repositories;
using Papirfly_ArticlesAPI.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Papirfly_ArticlesAPI.Tests
{
    public class ArticleServiceTests
    {
        private static ArticleRequest Make(string name, string? category = null)
            => new() { Name = name, Description = "d", Category = category, Price = 0 };

        private static ArticleService NewService() => new(new InMemoryArticleRepository());

        [Fact]
        public void Name_search_is_partial_and_case_insensitive()
        {
            var service = NewService();
            service.Create(Make("Branded Memory Stick"));
            service.Create(Make("Coffee Mug"));

            var result = service.Search("BRANDED", null);

            Assert.Single(result);
        }

        [Fact]
        public void Category_search_needs_complete_name()
        {
            var service = NewService();
            service.Create(Make("A", "USB flash drive"));

            Assert.Empty(service.Search(null, "USB"));
            Assert.Single(service.Search(null, "USB flash drive"));
        }

        [Fact]
        public void CreateMany_gives_every_article_a_unique_id()
        {
            var service = NewService();
            var requests = Enumerable.Range(0, 500).Select(i => Make($"A{i}")).ToList();

            var created = service.CreateMany(requests);

            Assert.Equal(500, created.Select(a => a.ArticleId).Distinct().Count());
        }

        [Fact]
        public void Update_with_old_version_returns_conflict()
        {
            var service = NewService();
            var created = service.Create(Make("A"));

            var first = service.Update(created.ArticleId, Make("B"), expectedVersion: 1);
            var second = service.Update(created.ArticleId, Make("C"), expectedVersion: 1);  // verze 1 už je zastaralá

            Assert.Equal(UpdateStatus.Updated, first.Status);
            Assert.Equal(UpdateStatus.Conflict, second.Status);
        }

        [Fact]
        public void Update_of_missing_article_returns_not_found()
            => Assert.Equal(UpdateStatus.NotFound, NewService().Update(999, Make("A"), null).Status);
    }
}
