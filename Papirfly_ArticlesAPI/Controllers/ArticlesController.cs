using Microsoft.AspNetCore.Mvc;
using Papirfly_ArticlesAPI.Models;
using Papirfly_ArticlesAPI.Services;

namespace Papirfly_ArticlesAPI.Controllers;

[ApiController]
[Route("api")]
public class ArticlesController(ArticleService service) : ControllerBase
{
    /// <summary>Creates one new article.</summary>
    [HttpPost("articles")]
    [ProducesResponseType<ArticleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<ArticleResponse> Create(ArticleRequest request)
    {
        var errors = ArticleValidator.Validate(request);
        if (errors.Count > 0) return Invalid(errors);

        var created = service.Create(request);
        SetETag(created);
        return Ok(ArticleResponse.From(created));
    }

    /// <summary>Returns an article by its ID.</summary>
    [HttpGet("articles/{id:int}")]
    [ProducesResponseType<ArticleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ArticleResponse> GetById(int id)
    {
        var article = service.Get(id);
        if (article is null) return NotFound();

        SetETag(article);
        return Ok(ArticleResponse.From(article));
    }

    /// <summary>Searches for articles. name = partial match regardless of case, category = exact match.</summary>
    [HttpGet("articles")]
    [ProducesResponseType<IEnumerable<ArticleResponse>>(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<ArticleResponse>> Search([FromQuery] string? name, [FromQuery] string? category)
        => Ok(service.Search(name, category).Select(ArticleResponse.From));

    /// <summary> Instering multiple articles concurrently. If any are invalid, none are inserted.</summary>
    [HttpPost("articles-concurrent")]
    [ProducesResponseType<IEnumerable<ArticleResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<IEnumerable<ArticleResponse>> CreateConcurrent(List<ArticleRequest> requests)
    {
        if (requests.Any(r => r is null)) return Invalid(["Array contains a null item."]);

        //Validating all
        var errors = requests
            .SelectMany((r, i) => ArticleValidator.Validate(r).Select(e => $"[{i}] {e}"))
            .ToList();
        if (errors.Count > 0) return Invalid(errors);

        return Ok(service.CreateMany(requests).Select(ArticleResponse.From));
    }

    /// <summary>Updates an article. The If-Match header (value from the ETag) protects against overwriting foreign changes.</summary>
    [HttpPut("articles/{id:int}")]
    [ProducesResponseType<ArticleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Update(int id, ArticleRequest request, [FromHeader(Name = "If-Match")] string? ifMatch)
    {
        var errors = ArticleValidator.Validate(request);
        if (errors.Count > 0) return Invalid(errors);

        int? expectedVersion = null;
        if (ifMatch is not null)
        {
            if (!int.TryParse(ifMatch.Trim('"'), out var version)) return Invalid(["If-Match header is not valid."]);
            expectedVersion = version;
        }

        var result = service.Update(id, request, expectedVersion);
        switch (result.Status)
        {
            case UpdateStatus.NotFound:
                return NotFound();
            case UpdateStatus.Conflict:
                return Conflict();
            default:
                SetETag(result.Article!);
                return Ok(ArticleResponse.From(result.Article!));
        }
    }

    // Returns a 400 Bad Request with a ProblemDetails body containing the validation errors.
    private BadRequestObjectResult Invalid(IEnumerable<string> errors)
        => BadRequest(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed",
            Detail = string.Join(" ", errors)
        });

    private void SetETag(Article article) => Response.Headers.ETag = $"\"{article.Version}\"";
}