using System.Text.Json.Serialization;
using Papirfly_ArticlesAPI.Repositories;
using Papirfly_ArticlesAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
     {
         // Only numbers, not strings => if the client sends a string instead of a number, it's a 400 error.
         o.JsonSerializerOptions.NumberHandling = JsonNumberHandling.Strict;
     });

// Singleton = one instance for the whole application lifetime. InMemoryArticleRepository is thread-safe, so it's safe to use as a singleton.
builder.Services.AddSingleton<IArticleRepository, InMemoryArticleRepository>();
// Scoped = one instance per request. ArticleService is not thread-safe, so we need a new instance for each request.
builder.Services.AddScoped<ArticleService>();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "Articles API v1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { } 
