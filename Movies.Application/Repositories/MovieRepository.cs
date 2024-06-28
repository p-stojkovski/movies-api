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

    public Task<IEnumerable<Movie>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<Movie?> GetByIdAsync(Guid id)
    {
        //using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        //var movie = await connection.QuerySingleOrDefaultAsync<Movie>(new CommandDefinition("""
        //    select * from movies where id = @id
        //    """, new { id }));

        //if (movie is null)
        //{
        //    return null;
        //}

        //var genres = await connection.QueryAsync<string>(new CommandDefinition("""
        //    select name from genres where movieId = @id
        //    """, new { id }));

        //foreach (var genre in genres)
        //{
        //    movie.Genres.Add(genre);
        //}

        //return movie;

        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        var query = @"
            select * from movies where id = @id;
            select name from genres where movieId = @id;
        ";

        var multi = await connection.QueryMultipleAsync(query, new { id });

        var movie = await multi.ReadSingleOrDefaultAsync<Movie>();
        if (movie is null)
        {
            return null;
        }

        movie.Genres = (await multi.ReadAsync<string>()).ToList();
        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        var movieQuery = @"select * from movies where slug = @slug;";
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(movieQuery, new { slug });
        if (movie is null)
        {
            return null;
        }

        var genresQuery = @"select name from genres where movieId = @id;";
        var genres = await connection.QueryAsync<string>(genresQuery, new { id = movie.Id });

        movie.Genres = genres.ToList();

        return movie;
    }

    public async Task<bool> CreateAsync(Movie movie)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        using var transaction = connection.BeginTransaction();

        var result = await connection.ExecuteAsync(new CommandDefinition("""
            insert into movies (id, slug, title, yearofrelease)
            values (@Id, @Slug, @Title, @YearOfRelease)
            """, movie));

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

    public Task<bool> UpdateAsync(Movie movie)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}
