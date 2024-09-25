using Newtonsoft.Json;
using AggregationTask.API.Models.News;
using AggregationTask.API.Models.Weather;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using ServiceStack;
using AggregationTask.API.Services.StatisticsService;


namespace AggregationTask.API.Services.NewsService;
public class NewsService : INewsService
{
    private readonly HttpClient httpClient;
    private readonly IConfiguration configuration;
    private readonly IMemoryCache memoryCache;
    private readonly TimeSpan cacheExpiration = TimeSpan.FromMinutes(10); // Cache duration
    private readonly IStatisticsService statisticsCollector;
    private readonly ILogger<NewsService> logger;

    public  NewsService(HttpClient httpClient, IConfiguration configuration, IMemoryCache memoryCache, 
                        IStatisticsService statisticsCollector, ILogger<NewsService> logger)
    {
        this.httpClient = httpClient;
        this.configuration = configuration;
        this.memoryCache = memoryCache;
        this.statisticsCollector = statisticsCollector;
        this.logger = logger;   
    }

    public async Task<NewsData> GetNewsAsync(string category, DateTime? From)
    {
        // Create a unique cache key based on the username and filter/sort parameters
        var cacheKey = $"NewsData_{category}_{From}";
        // Try to get data from cache
        if (!memoryCache.TryGetValue(cacheKey, out NewsData newsResponse))
        {
            // Start timing
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // If not in cache, make API call
            var response = await httpClient.GetAsync($"everything?q={category}&{AddFromDate(From)}sortBy=popularity&language=en&apiKey={configuration["APIKeys:News:Key"]}");

            // Stop timing
            stopwatch.Stop();
            // Log the response time
            statisticsCollector.LogApiCall("News", stopwatch.Elapsed);

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning($"Error fetching news: {ex.Message}");
                return new NewsData
                {
                    TotalResults = 0,
                    Articles = new List<NewsArticleDto>
                    {
                        new NewsArticleDto
                        {
                            Title = "Service unavailable",
                            Description = "Unable to fetch news at the moment."
                        }
                    }
                };
            }
            var result = await response.Content.ReadAsStringAsync();

            newsResponse = JsonConvert.DeserializeObject<NewsData>(result);

            var filteredArticles = newsResponse.Articles?
                .Where(article => !IsRemoved(article))
                .ToList() ?? new List<NewsArticleDto>();

            newsResponse.Articles = filteredArticles;

            // Save to cache
            memoryCache.Set(cacheKey, newsResponse, cacheExpiration);
        }
        return newsResponse;
    }

    private string AddFromDate(DateTime? from)
    {
        if (from == null)
        {
            from = DateTime.UtcNow.AddDays(-7);
        }
        return $"from={from}";
    }

    private bool IsRemoved(NewsArticleDto article)
    {
        return article.Title == "[Removed]" ||
               article.Description == "[Removed]" ||
               article.Content == "[Removed]" ;
    }


}