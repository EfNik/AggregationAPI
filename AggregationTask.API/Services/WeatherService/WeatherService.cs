using Newtonsoft.Json;
using AggregationTask.API.Models.Weather;
using Microsoft.Extensions.Caching.Memory;
using AggregationTask.API.Services.StatisticsService;
using AggregationTask.API.Models.News;

namespace AggregationTask.API.Services.WeatherService;
public class WeatherService : IWeatherService
{
    private readonly HttpClient httpClient;
    private readonly IConfiguration configuration;
    private readonly IMemoryCache memoryCache;
    private readonly TimeSpan cacheExpiration = TimeSpan.FromMinutes(10); // Cache duration
    private readonly IStatisticsService statisticsCollector;
    private readonly ILogger<WeatherService> logger;

    public WeatherService(HttpClient httpClient, IConfiguration configuration, IMemoryCache memoryCache, 
                            IStatisticsService statisticsCollector, ILogger<WeatherService> logger)
    {
        this.httpClient = httpClient;
        this.configuration = configuration;
        this.memoryCache = memoryCache;
        this.statisticsCollector = statisticsCollector;
        this.logger = logger;
    }

    public async Task<WeatherData> GetWeatherAsync(string city, DateTime? endDate)
    {

        // Create a unique cache key based on the username and filter/sort parameters
        var cacheKey = $"Weather_{city}_{endDate}";
        // Try to get data from cache
        if (!memoryCache.TryGetValue(cacheKey, out WeatherData weatherData))
        {

            // Start timing
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var response = await httpClient.GetAsync($"forecast?q={city}&appid={configuration["APIKeys:Weather:Key"]}&units=metric");

            // Stop timing
            stopwatch.Stop();
            // Log the response time
            statisticsCollector.LogApiCall("Weather", stopwatch.Elapsed);

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning($"Error fetching weather: {ex.Message}");
                return new WeatherData
                {
                    City = "Service unavailable. Unable to fetch weather at the moment.",
                    Forecasts = new List<WeatherForecastDto>()
                };
            }

            var result = await response.Content.ReadAsStringAsync();


            var weatherResponse = JsonConvert.DeserializeObject<WeatherApiResponse>(result);

            var filteredForecasts = weatherResponse.List
                .Where(forecast =>
                {
                    var unixEnd = ((DateTimeOffset)(endDate ?? DateTime.Now.AddHours(24))).ToUnixTimeSeconds();
                    if (endDate.HasValue)
                    {
                        return forecast.Dt <= unixEnd;
                    }
                    return true;
                })
                .ToList();

            weatherData = new WeatherData
            {
                City = weatherResponse.City.Name,
                Forecasts = filteredForecasts.Select(f => new WeatherForecastDto
                {
                    Dt = f.Dt,
                    Temp = f.Main.Temp,
                    FeelsLike = f.Main.FeelsLike,
                    Description = f.Weather.FirstOrDefault()?.Description,
                    WindSpeed = f.Wind.Speed,
                    Cloudiness = f.Clouds.All,
                    Humidity = f.Main.Humidity
                }).ToList()
            };

            // Save to cache
            memoryCache.Set(cacheKey, weatherData, cacheExpiration);

        }
        return weatherData;
    }
}
