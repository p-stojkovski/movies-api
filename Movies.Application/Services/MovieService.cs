using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Services;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _movieReposiotry;
    private readonly IValidator<Movie> _movieValidator;
    private readonly IRatingRepository _ratingRepository;

    public MovieService(IMovieRepository movieReposiotry, IValidator<Movie> movieValidator, IRatingRepository ratingRepository)
    {
        _movieReposiotry = movieReposiotry;
        _movieValidator = movieValidator;
        _ratingRepository = ratingRepository;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(Guid? userId = default, CancellationToken cancellationToken = default)
    {
        return await _movieReposiotry.GetAllAsync(userId, cancellationToken);
    }

    public async Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken cancellationToken = default)
    {
        return await _movieReposiotry.GetByIdAsync(id, userId, cancellationToken);
    }

    public async Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default, CancellationToken cancellationToken = default)
    {
        return await _movieReposiotry.GetBySlugAsync(slug, userId, cancellationToken);
    }

    public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken)
    {
        await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken);

        return await _movieReposiotry.CreateAsync(movie, cancellationToken);
    }

    public async Task<Movie?> UpdateAsync(Movie movie, Guid? userId = default, CancellationToken cancellationToken = default)
    {
        await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken);

        var movieExists = await _movieReposiotry.ExistsByIdAsync(movie.Id, cancellationToken);
        if (!movieExists)
        {
            return null;
        }

        await _movieReposiotry.UpdateAsync(movie, cancellationToken);

        if (!userId.HasValue)
        {
            var rating = await _ratingRepository.GetRatingAsync(movie.Id, cancellationToken);
            movie.Rating = rating;

            return movie;
        }

        var ratings = await _ratingRepository.GetRatingsAsync(movie.Id, userId.Value, cancellationToken);
        movie.Rating = ratings.Rating;
        movie.UserRating = ratings.UserRating;

        return movie;
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _movieReposiotry.DeleteByIdAsync(id, cancellationToken);
    }
}