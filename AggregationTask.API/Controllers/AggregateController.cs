using AggregationTask.API.Services.GitHubService;
using AggregationTask.API.Services.NewsService;
using AggregationTask.API.Services.WeatherService;
using AggregationTask.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceStack;

namespace AggregationTask.API.Controllers;
[Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
[ApiController]
public class AggregateController : ControllerBase
{

    private readonly INewsService newsService;
    private readonly IWeatherService weatherService;
    private readonly IGitHubService gitHubService;


    public AggregateController( INewsService newsService, IWeatherService weatherService, IGitHubService gitHubService)
    {
        this.newsService = newsService;
        this.weatherService = weatherService;
        this.gitHubService = gitHubService;
    }

    /// <summary>Get the aggregated data</summary>
    /// <param name="city">The name of the city for which to retrieve weather data (e.g., "London").</param>
    /// <param name="weatherEndDate">Optional. The end date up to which weather data will be fetched. If not provided, it fetches data for the next 24 hours.</param>
    /// <param name="category">The category of news to retrieve (e.g., "technology", "sports").</param>
    /// <param name="newsFrom">Optional. The start date from which to retrieve news articles. If not provided, it retrieves last weeks news.</param>
    /// <param name="githubUser">The GitHub username to fetch repositories for.</param>
    /// <param name="reposSortBy">Optional. The field by which to sort repositories. Allowed values: "name", "created", "updated".</param>
    /// <param name="reposFilterBy">Optional. A keyword to filter repositories by name.</param>
    /// <returns>News, weather, github repositories</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<AggregatedDataResponse>> GetAggregatedData(
            string city, DateTime? weatherEndDate, 
            string category, DateTime? newsFrom, 
            string githubUser, string reposSortBy = null, string reposFilterBy = null)
    {
        var weatherTask = weatherService.GetWeatherAsync(city, weatherEndDate);
        var newsTask = newsService.GetNewsAsync(category,newsFrom);
        var gitHubTask = gitHubService.GetRepositoriesAsync(githubUser, reposSortBy, reposFilterBy);

        await Task.WhenAll(weatherTask, newsTask, gitHubTask);

        var result = new AggregatedDataResponse
        {
            Weather = await weatherTask,
            News = await newsTask,
            Repositories = await gitHubTask
        };

        return Ok(result);
    }
}

