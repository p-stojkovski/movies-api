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

    public async Task<IEnumerable<Movie>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var moviesQuery = @"select m.*, string_agg(g.name, ',') as genres
                            from movies m left join genres g on m.id = g.movieid
                            group by id";

        var result = await connection.QueryAsync(new CommandDefinition(moviesQuery, cancellationToken: cancellationToken));

        return result.Select(x => new Movie
        {
            Id = x.id,
            Title = x.title,
            YearOfRelease = x.yearofrelease,
            Genres = x.genres is null ? new List<string>() : Enumerable.ToList(x.genres.Split(','))
        });
    }

    public async Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        var query = @"
            select * from movies where id = @id;
            select name from genres where movieId = @id;
        ";

        var result = await connection
            .QueryMultipleAsync(new CommandDefinition(query, new { id }, cancellationToken: cancellationToken));

        var movie = await result.ReadSingleOrDefaultAsync<Movie>();
        if (movie is null)
        {
            return null;
        }

        movie.Genres = (await result.ReadAsync<string>()).ToList();
        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        var movieQuery = @"select * from movies where slug = @slug;";
        var movie = await connection
            .QuerySingleOrDefaultAsync<Movie>(new CommandDefinition(movieQuery, new { slug }, cancellationToken: cancellationToken));
        
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
