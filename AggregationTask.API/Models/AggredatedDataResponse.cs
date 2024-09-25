using AggregationTask.API.Models.News;
using AggregationTask.API.Models.Weather;
using ServiceStack;

namespace AggregationTask.API.Models;
/// <summary>
/// Represents the aggregated response containing weather, news, and GitHub repository data.
/// </summary>
public class AggregatedDataResponse
{
    /// <summary>
    /// The weather data for the specified city.
    /// </summary>
    public WeatherData Weather { get; set; }

    /// <summary>
    /// The news data for the specified category and date range.
    /// </summary>
    public NewsData News { get; set; }

    /// <summary>
    /// A list of repositories retrieved for the specified GitHub user.
    /// </summary>
    public List<GithubRepo> Repositories { get; set; }
}