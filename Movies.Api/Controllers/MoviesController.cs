using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Controllers;

//TODO: Separate api versions in different controllers V1, V2
[ApiController]
[ApiVersion(1.0)]
//[ApiVersion(2.0)]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;
    private readonly IOutputCacheStore _outputCacheStore;

    public MoviesController(IMovieService movieService, IOutputCacheStore outputCacheStore)
    {
        _movieService = movieService;
        _outputCacheStore = outputCacheStore;
    }

    [Authorize(AuthConstants.TrustMemberPolicyName)]
    [HttpPost(ApiEndpoints.Movies.Create)]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationFaliureResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();

        var movie = request.MapToMovie();

        await _movieService.CreateAsync(movie, cancellationToken);

        await _outputCacheStore.EvictByTagAsync("movies", cancellationToken);

        var response = movie.MapToResponse();

        return CreatedAtAction(nameof(Get), new { idOrSlug = response.Id }, response);
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    [OutputCache(PolicyName = "MovieCache")]
    //[ResponseCache(Duration = 30, VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Client)]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] string idOrSlug, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();

        var movie = Guid.TryParse(idOrSlug, out var id)
            ? await _movieService.GetByIdAsync(id, userId, cancellationToken)
            : await _movieService.GetBySlugAsync(idOrSlug, userId, cancellationToken);

        if (movie is null)
        {
            return NotFound();
        }

        var response = movie.MapToResponse();

        return Ok(response);
    }

    //[MapToApiVersion(2.0)]
    //[HttpGet(ApiEndpoints.Movies.Get)]
    //public async Task<IActionResult> GetV2([FromRoute] string idOrSlug,
    //    [FromServices] LinkGenerator linkGenerator,
    //    CancellationToken cancellationToken)
    //{
    //    var userId = HttpContext.GetUserId();

    //    var movie = Guid.TryParse(idOrSlug, out var id)
    //        ? await _movieService.GetByIdAsync(id, userId, cancellationToken)
    //        : await _movieService.GetBySlugAsync(idOrSlug, userId, cancellationToken);

    //    if (movie is null)
    //    {
    //        return NotFound();
    //    }

    //    var response = movie.MapToResponse();

    //    var movieObj = new { id = movie.Id };
    //    response.Links.Add(new Link
    //    {
    //        Href = linkGenerator.GetPathByAction(HttpContext, nameof(GetV2), values: new { idOrSlug = movie.Id })!,
    //        Rel = "self",
    //        Type = "GET"
    //    });
    //    response.Links.Add(new Link
    //    {
    //        Href = linkGenerator.GetPathByAction(HttpContext, nameof(Update), values: movieObj)!,
    //        Rel = "self",
    //        Type = "PUT"
    //    });
    //    response.Links.Add(new Link
    //    {
    //        Href = linkGenerator.GetPathByAction(HttpContext, nameof(Delete), values: new { id = movie.Id })!,
    //        Rel = "self",
    //        Type = "DELETE"
    //    });

    //    return Ok(response);
    //}

    [ServiceFilter(typeof(ApiKeyAuthFilter))]
    [HttpGet(ApiEndpoints.Movies.GetAll)]
    [OutputCache(PolicyName = "MovieCache")]
    //[ResponseCache(Duration = 30, 
    //    VaryByQueryKeys = new[] { "title", "year", "sortBy", "page", "pageSize" }, 
    //    VaryByHeader = "Accept, Accept-Encoding", 
    //    Location = ResponseCacheLocation.Client)]
    [ProducesResponseType(typeof(MoviesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetAllMoviesRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var options = request.MapToOptions()
            .WithUser(userId);

        var movies = await _movieService.GetAllAsync(options, cancellationToken);
        var movieCount = await _movieService.GetCountAsync(options.Title, options.YearOfRelease, cancellationToken);

        var response = movies.MapToResponse(request.Page, request.PageSize, movieCount);

        return Ok(response);
    }

    [Authorize(AuthConstants.TrustMemberPolicyName)]
    [HttpPut(ApiEndpoints.Movies.Update)]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationFaliureResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovieRequest request,
        CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();

        var movie = request.MapToMovie(id);

        var updatedMovie = await _movieService.UpdateAsync(movie, userId, cancellationToken);
        if (updatedMovie is null)
        {
            return NotFound();
        }

        await _outputCacheStore.EvictByTagAsync("movies", cancellationToken);

        var response = movie.MapToResponse();

        return Ok(response);
    }

    [Authorize(AuthConstants.TrustMemberPolicyName)]
    [HttpDelete(ApiEndpoints.Movies.Delete)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _movieService.DeleteByIdAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        await _outputCacheStore.EvictByTagAsync("movies", cancellationToken);

        return Ok();
    }
}
