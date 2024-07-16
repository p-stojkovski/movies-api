using Movies.Api.Sdk;
using Refit;
using System.Text.Json;

var moviesApi = RestService.For<IMoviesApi>("http://localhost:5286/");

var movie = await moviesApi.GetMoviesAsync("toy-story-1995");

Console.WriteLine(JsonSerializer.Serialize(movie));