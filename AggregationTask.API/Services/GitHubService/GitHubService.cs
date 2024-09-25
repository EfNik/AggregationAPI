using AggregationTask.API.Models.Weather;
using AggregationTask.API.Services.Statistics;
using AggregationTask.API.Services.StatisticsService;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using ServiceStack;

namespace AggregationTask.API.Services.GitHubService;
public class GitHubService : IGitHubService
{
    private readonly HttpClient httpClient;
    private readonly IMemoryCache memoryCache;
    private readonly TimeSpan cacheExpiration = TimeSpan.FromMinutes(10); // Cache duration
    private readonly IStatisticsService statisticsCollector;
    private readonly ILogger<GitHubService> logger;

    public GitHubService(HttpClient httpClient, IMemoryCache memoryCache, IStatisticsService statisticsCollector , 
                         ILogger<GitHubService> logger)
    {
        this.httpClient = httpClient;
        this.memoryCache = memoryCache;
        this.statisticsCollector = statisticsCollector;
        this.logger = logger;   
    }

    public async Task<List<GithubRepo>> GetRepositoriesAsync(string username, string sortBy = null, string filterBy = null)
    {
        // Create a unique cache key based on the username and filter/sort parameters
        var cacheKey = $"GitHubRepos_{username}_{sortBy}_{filterBy}";

        // Try to get data from cache
        if (!memoryCache.TryGetValue(cacheKey, out List<GithubRepo> repositories))
        {
            // Start timing
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // If not in cache, make API call
            var response = await httpClient.GetAsync($"users/{username}/repos");

            // Stop timing
            stopwatch.Stop();
            // Log the response time
            statisticsCollector.LogApiCall("GitHub", stopwatch.Elapsed);

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning($"Error fetching repositories: {ex.Message}");
                return new List<GithubRepo>
                {
                    new GithubRepo
                    {
                        Name = "Service unavailable.",
                        Description = "Unable to fetch repositories at the moment."
                    }
                };
            }

            var result = await response.Content.ReadAsStringAsync();

            repositories = JsonConvert.DeserializeObject<List<GithubRepo>>(result);

            // Filter and sort
            if (!string.IsNullOrEmpty(filterBy))
            {
                repositories = repositories.Where(r => r.Name.Contains(filterBy, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (!string.IsNullOrEmpty(sortBy))
            {
                repositories = sortBy.ToLower() switch
                {
                    "name" => repositories.OrderBy(r => r.Name).ToList(),
                    "created" => repositories.OrderByDescending(r => r.Created_At).ToList(),
                    "updated" => repositories.OrderByDescending(r => r.Updated_At).ToList(),
                    _ => repositories
                };
            }


            // Save to cache
            memoryCache.Set(cacheKey, repositories, cacheExpiration);
        }

        // Return cached or API-fetched data
        return repositories;
    
    //var response = await httpClient.GetAsync($"users/{username}/repos");
    //response.EnsureSuccessStatusCode();
    //var result = await response.Content.ReadAsStringAsync();
    //var repositories =  JsonConvert.DeserializeObject<List<GithubRepo>>(result);

    //// Filtering
    //if (!string.IsNullOrEmpty(filterBy))
    //{
    //    repositories = repositories.Where(r => r.Name.Contains(filterBy, StringComparison.OrdinalIgnoreCase)).ToList();
    //}

    //// Sorting
    //if (!string.IsNullOrEmpty(sortBy))
    //{
    //    repositories = sortBy.ToLower() switch
    //    {
    //        "name" => repositories.OrderBy(r => r.Name).ToList(),
    //        "created" => repositories.OrderByDescending(r => r.Created_At).ToList(),
    //        "updated" => repositories.OrderByDescending(r => r.Updated_At).ToList(),
    //        _ => repositories
    //    };
    //}

    //return repositories;
    }
}
