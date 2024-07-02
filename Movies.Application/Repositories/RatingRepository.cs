using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class RatingRepository : IRatingRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public RatingRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<float?> GetRatingAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<float?>(new CommandDefinition(@"
            select round(avg(r.rating), 1) 
            from ratings r
            where movieId = @movieId",
        new { movieId }, cancellationToken: cancellationToken));
    }

    public async Task<(float? Rating, int? UserRating)> GetRatingsAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<(float?, int?)>(new CommandDefinition(@"
            select round(avg(r.rating), 1),
                  (select rating
                    from ratings
                    where movieid = @movieId and userid = @userId
                    limit 1)
            from ratings r
            where movieId = @movieId",
        new { movieId, userId }, cancellationToken: cancellationToken));
    }

    public async Task<bool> RateMovieAsync(Guid movieId, int rating, Guid userId, CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var result = await connection.ExecuteAsync(new CommandDefinition(@"
            insert into ratings(userid, movieid, rating)
            values (@userId, @movieId, @rating)
            on conflict (userid, movieid) do update
                set rating = @rating",
            new { userId, movieId, rating }, cancellationToken: cancellationToken));

        return result > 0;
    }

    public async Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var result = await connection.ExecuteAsync(new CommandDefinition(@"
            delete from ratings
            where movieid = @movieid and userid = @userid",
            new { userId, movieId }, cancellationToken: cancellationToken));

        return result > 0;
    }

    public async Task<IEnumerable<MovieRating>> GetRatingsForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<MovieRating>(new CommandDefinition(@"
            select r.rating, r.movieid, m.slug
            from ratings r
            inner join movies m on r.movieid = m.id
            where userid = @userid",
        new { userId }, cancellationToken: cancellationToken));

    }
}
