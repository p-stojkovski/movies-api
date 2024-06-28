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

    public async Task<IEnumerable<Movie>> GetAllAsync()
    {
        return await _movieReposiotry.GetAllAsync();
    }

    public async Task<Movie?> GetByIdAsync(Guid id)
    {
        return await _movieReposiotry.GetByIdAsync(id);
    }

    public async Task<Movie?> GetBySlugAsync(string slug)
    {
        return await _movieReposiotry.GetBySlugAsync(slug);
    }

    public async Task<bool> CreateAsync(Movie movie)
    {
        await _movieValidator.ValidateAndThrowAsync(movie);

        return await _movieReposiotry.CreateAsync(movie);
    }

    public async Task<Movie?> UpdateAsync(Movie movie)
    {
        await _movieValidator.ValidateAndThrowAsync(movie);

        var movieExists = await _movieReposiotry.ExistsByIdAsync(movie.Id);
        if (!movieExists)
        {
            return null;
        }

        await _movieReposiotry.UpdateAsync(movie);

        return movie;
    }

    public async Task<bool> DeleteByIdAsync(Guid id)
    {
        return await _movieReposiotry.DeleteByIdAsync(id);
    }
}