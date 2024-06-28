using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Services;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _movieReposiotry;
    private readonly IValidator<Movie> _movieValidator;

    public MovieService(IMovieRepository movieReposiotry, IValidator<Movie> movieValidator)
    {
        _movieReposiotry = movieReposiotry;
        _movieValidator = movieValidator;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _movieReposiotry.GetAllAsync(cancellationToken);
    }

    public async Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _movieReposiotry.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Movie?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        return await _movieReposiotry.GetBySlugAsync(slug, cancellationToken);
    }

    public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken)
    {
        await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken);

        return await _movieReposiotry.CreateAsync(movie, cancellationToken);
    }

    public async Task<Movie?> UpdateAsync(Movie movie, CancellationToken cancellationToken)
    {
        await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken);

        var movieExists = await _movieReposiotry.ExistsByIdAsync(movie.Id, cancellationToken);
        if (!movieExists)
        {
            return null;
        }

        await _movieReposiotry.UpdateAsync(movie, cancellationToken);

        return movie;
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _movieReposiotry.DeleteByIdAsync(id, cancellationToken);
    }
}