using Movies.Api.Sdk;
using Movies.Contracts.Requests;
using Refit;
using System.Text.Json;

var moviesApi = RestService.For<IMoviesApi>("http://localhost:5286/");

var movie = await moviesApi.GetMovieAsync("toy-story-1995");

var movies = await moviesApi.GetMoviesAsync(new GetAllMoviesRequest
{
    Title = null,
    Year = null,
    SortBy = null,
    Page = 1,
    PageSize = 3
});

Console.WriteLine(JsonSerializer.Serialize(movie));

foreach (var item in movies.Items)
{
    Console.WriteLine(JsonSerializer.Serialize(item));
}
