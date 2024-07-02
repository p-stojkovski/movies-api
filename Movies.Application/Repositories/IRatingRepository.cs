namespace Movies.Application.Repositories;

public interface IRatingRepository
{
    Task<bool> RateMovieAsync(Guid movieId, int rating, Guid userId, CancellationToken cancellationToken);
    Task<float?> GetRatingAsync(Guid movieId, CancellationToken cancellationToken = default);
    Task<(float? Rating, int? UserRating)> GetRatingsAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken);
}
