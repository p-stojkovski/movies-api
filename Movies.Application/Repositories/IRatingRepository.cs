namespace Movies.Application.Repositories;

public interface IRatingRepository
{
    Task<float?> GetRatingAsync(Guid movieId, CancellationToken cancellationToken = default);
    Task<(float? Rating, int? UserRating)> GetRatingsAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default);
}
