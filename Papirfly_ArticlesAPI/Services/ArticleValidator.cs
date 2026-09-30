using Papirfly_ArticlesAPI.Models;

namespace Papirfly_ArticlesAPI.Services
{
    public static class ArticleValidator
    {
        public static IReadOnlyList<string> Validate(ArticleRequest article)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(article.Name))
                errors.Add("name is required.");
            else if (article.Name.Length > 64)
                errors.Add("name must be at most 64 characters.");

            if (string.IsNullOrWhiteSpace(article.Description))
                errors.Add("description is required.");
            else if (article.Description.Length > 2048)
                errors.Add("description must be at most 2048 characters.");

            if (article.Category is { Length: > 64 })
                errors.Add("category must be at most 64 characters.");

            if (article.Price < 0)
                errors.Add("price must be greater than or equal to 0.");

            var hasCurrency = !string.IsNullOrWhiteSpace(article.Currency);

            if (article.Price > 0 && !hasCurrency)
                errors.Add("currency is required when price is greater than 0.");

            if (hasCurrency && !Iso4217.IsValid(article.Currency!))
                errors.Add("currency must be a valid ISO 4217 code (e.g. NOK, CZK, EUR).");

            return errors;
        }
    }
}
