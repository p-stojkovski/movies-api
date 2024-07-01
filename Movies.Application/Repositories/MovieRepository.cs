using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public MovieRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(Guid? userId = default, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var moviesQuery = @"select m.*, 
                                string_agg(distinct g.name, ',') as genres,
                                round(avg(r.rating), 1) as rating, myr.rating as userrating
                            from movies m 
                            left join genres g on m.id = g.movieid                            
                            left join ratings r on m.id = r.movieid
                            left join ratings myr on m.id = myr.movieid
                                and myr.userid = @userId
                            group by id, userrating";

        var result = await connection.QueryAsync(new CommandDefinition(moviesQuery, new { userId }, cancellationToken: cancellationToken));

        return result.Select(x => new Movie
        {
            Id = x.id,
            Title = x.title,
            YearOfRelease = x.yearofrelease,
            Rating = (float?)x.rating,
            UserRating = (int?)x.userRating,
            Genres = x.genres is null ? new List<string>() : Enumerable.ToList(x.genres.Split(','))
        });
    }

    public async Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        var query = @"
            select m.*, round(avg(r.rating), 1) as rating, myr.rating as userrating
            from movies m
                left join ratings r on m.id = r.movieid
                left join ratings myr on m.id = myr.movieid
                    and myr.userid = @userId
            where id = @id
            group by id, userrating;
            select name from genres where movieId = @id;
        ";

        var result = await connection
            .QueryMultipleAsync(new CommandDefinition(query, new { id, userId }, cancellationToken: cancellationToken));

        var movie = await result.ReadSingleOrDefaultAsync<Movie>();
        if (movie is null)
        {
            return null;
        }

        movie.Genres = (await result.ReadAsync<string>()).ToList();
        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        var movieQuery = @"
            select m.*, round(avg(r.rating), 1) as rating, myr.rating as userrating
            from movies m
                left join ratings r on m.id = r.movieid
                left join ratings myr on m.id = myr.movieid
                    and myr.userid = @userId
            where slug = @slug
            group by id, userrating";
        var movie = await connection
            .QuerySingleOrDefaultAsync<Movie>(new CommandDefinition(movieQuery, new { slug, userId }, cancellationToken: cancellationToken));

        if (movie is null)
        {
            return null;
        }

        var genresQuery = @"select name from genres where movieId = @id;";
        var genres = await connection
            .QueryAsync<string>(new CommandDefinition(genresQuery, new { id = movie.Id }, cancellationToken: cancellationToken));

        movie.Genres = genres.ToList();

        return movie;
    }

    public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        using var transaction = connection.BeginTransaction();

        var result = await connection.ExecuteAsync(new CommandDefinition("""
            insert into movies (id, slug, title, yearofrelease)
            values (@Id, @Slug, @Title, @YearOfRelease)
            """, movie, cancellationToken: cancellationToken));

        if (result > 0)
        {
            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new CommandDefinition("""
                    insert into genres (movieId, name)
                    values (@MovieId, @Name)
                    """, new { MovieId = movie.Id, Name = genre }));
            }
        }

        transaction.Commit();

        return result > 0;
    }

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        using var transaction = connection.BeginTransaction();

        var deleteGenresQuery = @"delete from genres where movieid = @id";
        await connection
            .ExecuteAsync(new CommandDefinition(deleteGenresQuery, new { id = movie.Id }, cancellationToken: cancellationToken));

        var insertGenreQuery = "insert into genres (movieId, name) values (@MovieId, @Name)";
        var genreParams = movie.Genres.Select(genre => new { MovieId = movie.Id, Name = genre });
        await connection.ExecuteAsync(new CommandDefinition(insertGenreQuery, genreParams, transaction, cancellationToken: cancellationToken));

        var updateMovieQuery = @"update movies set 
                                    slug = @Slug,
                                    title = @Title,
                                    yearofrelease = @YearOfRelease
                                where id = @Id";
        var result = await connection
            .ExecuteAsync(new CommandDefinition(updateMovieQuery, movie, transaction, cancellationToken: cancellationToken));

        transaction.Commit();

        return result > 0;

    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        using var transaction = connection.BeginTransaction();

        var deleteGenresQuery = @"delete from genres where movieid = @id";
        await connection
            .ExecuteAsync(new CommandDefinition(deleteGenresQuery, new { id }, transaction, cancellationToken: cancellationToken));

        var deleteMoviesQuery = @"delete from movies where id = @id";
        var result = await connection
            .ExecuteAsync(new CommandDefinition(deleteMoviesQuery, new { id }, transaction, cancellationToken: cancellationToken));

        transaction.Commit();

        return result > 0;
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        var query = @"select count(1) from movies where id = @id";

        return await connection
            .ExecuteScalarAsync<bool>(new CommandDefinition(query, new { id }, cancellationToken: cancellationToken));
    }
}
