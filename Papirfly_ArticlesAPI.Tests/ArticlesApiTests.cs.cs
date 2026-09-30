using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

using Xunit;

namespace Papirfly_ArticlesAPI.Tests
{
    public class ArticlesApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client = factory.CreateClient();

        private static StringContent Json(string json) => new(json, Encoding.UTF8, "application/json");

        private const string Valid =
            """{"name":"Stick","description":"d","category":"USB","price":17.89,"currency":"NOK"}""";

        [Fact]
        public async Task Post_valid_article_returns_200_with_id()
        {
            var response = await _client.PostAsync("/api/articles", Json(Valid));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(body.GetProperty("article_id").GetInt32() > 0);
        }

        [Theory]
        [InlineData("""{"name":"X","description":"Y","price":"17.89","currency":"NOK"}""")]  // price as a string
        [InlineData("""{"article_id":5,"name":"X","description":"Y","price":0}""")]            // article_id in POST
        [InlineData("""{"name":"X","description":"Y","price":10}""")]                          // currency is missing when price > 0
        [InlineData("""{"description":"Y","price":0}""")]                                       // name is missing
        [InlineData("""{"name":"X","description":"Y","price":-1,"currency":"NOK"}""")]        // negative price
        public async Task Post_invalid_article_returns_400(string payload)
        {
            var response = await _client.PostAsync("/api/articles", Json(payload));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Get_unknown_id_returns_404()
        {
            var response = await _client.GetAsync("/api/articles/999999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Search_without_match_returns_empty_array()
        {
            var response = await _client.GetAsync("/api/articles?name=neexistuje-nic");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(0, body.GetArrayLength());
        }

        [Fact]
        public async Task Concurrent_endpoint_rejects_whole_batch_when_one_is_invalid()
        {
            var payload = $"[{Valid},{{\"name\":\"\",\"description\":\"d\",\"price\":0}}]";

            var response = await _client.PostAsync("/api/articles-concurrent", Json(payload));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Put_with_stale_if_match_returns_409()
        {
            var created = await (await _client.PostAsync("/api/articles", Json(Valid)))
                .Content.ReadFromJsonAsync<JsonElement>();
            var id = created.GetProperty("article_id").GetInt32();

            HttpRequestMessage Put()
            {
                var request = new HttpRequestMessage(HttpMethod.Put, $"/api/articles/{id}") { Content = Json(Valid) };
                request.Headers.TryAddWithoutValidation("If-Match", "\"1\"");
                return request;
            }

            Assert.Equal(HttpStatusCode.OK, (await _client.SendAsync(Put())).StatusCode);        // version 1 → 2
            Assert.Equal(HttpStatusCode.Conflict, (await _client.SendAsync(Put())).StatusCode);  // still "1" = old
        }
    }
}
