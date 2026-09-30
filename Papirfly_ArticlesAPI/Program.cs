using Microsoft.EntityFrameworkCore;
using Papirfly_ArticlesAPI.Data;
using Papirfly_ArticlesAPI.Repositories;
using Papirfly_ArticlesAPI.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
     {
         // Only numbers, not strings => if the client sends a string instead of a number, it's a 400 error.
         o.JsonSerializerOptions.NumberHandling = JsonNumberHandling.Strict;
     });

builder.Services.AddDbContext<ArticlesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IArticleRepository, SqlArticleRepository>();
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
