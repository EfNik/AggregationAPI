using AggregationTask.API.Models.Weather;
using AggregationTask.API.Services.NewsService;
using AggregationTask.API.Services.StatisticsService;
using AggregationTask.API.Services.WeatherService;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AggregationTask.Test.Services;
public class WeatherServiceTests
{
    private readonly Moq.Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly Moq.Mock<IConfiguration> _configurationMock;
    private readonly WeatherService _weatherService;
    private readonly Mock<IMemoryCache> _memoryCacheMock;
    private readonly IStatisticsService _statisticsService;
    private readonly Mock<ILogger<WeatherService>> _mockLogger;

    public WeatherServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/")
        };
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["APIKeys:Weather:Key"]).Returns("test-weather-api-key");
        _memoryCacheMock = new Mock<IMemoryCache>();
        _mockLogger = new Mock<ILogger<WeatherService>>();
        _statisticsService = Mock.Of<IStatisticsService>();
        _weatherService = new WeatherService(_httpClient, _configurationMock.Object, _memoryCacheMock.Object, _statisticsService, _mockLogger.Object);
    }

    [Fact]
    public async Task GetWeatherAsync_ReturnsWeatherData_SuccessfulResponse()
    {
        // Arrange
        var city = "London";
        var endDate = DateTime.Now.AddDays(1);
        var weatherApiResponse = new WeatherApiResponse
        {
            City = new CityInfo { Name = "London" },
            List = new List<WeatherForecast>
                {
                    new WeatherForecast
                    {
                        Dt = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds(),
                        Main = new MainWeatherData { Temp = 22, FeelsLike = 21, Humidity = 50 },
                        Weather = new List<WeatherDescription> { new WeatherDescription { Description = "clear sky" } },
                        Wind = new Wind { Speed = 5 },
                        Clouds = new Clouds { All = 10 }
                    }
                }
        };

        var responseContent = JsonConvert.SerializeObject(weatherApiResponse);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            });

        // Setup the cache mock
        object cachedValue = null;
        _memoryCacheMock
            .Setup(m => m.TryGetValue(It.IsAny<object>(), out cachedValue))
            .Returns(false); // Simulate cache miss

        // Use the callback to simulate setting the cache
        _memoryCacheMock
            .Setup(m => m.CreateEntry(It.IsAny<object>()))
            .Returns(Mock.Of<ICacheEntry>());

        // Act
        var result = await _weatherService.GetWeatherAsync(city, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("London", result.City);
        Assert.Single(result.Forecasts);
        var forecast = result.Forecasts[0];
        Assert.Equal(22, forecast.Temp);
        Assert.Equal("clear sky", forecast.Description);
    }

    [Fact]
    public async Task GetWeatherAsync_FiltersForecasts_ByEndDate()
    {
        // Arrange
        var city = "Paris";
        var endDate = DateTime.Now.AddDays(-1); // Filter out future data
        var weatherApiResponse = new WeatherApiResponse
        {
            City = new CityInfo { Name = "Paris" },
            List = new List<WeatherForecast>
                {
                    new WeatherForecast
                    {
                        Dt = ((DateTimeOffset)DateTime.Now.AddDays(-2)).ToUnixTimeSeconds(),
                        Main = new MainWeatherData { Temp = 15, FeelsLike = 14, Humidity = 70 },
                        Weather = new List<WeatherDescription> { new WeatherDescription { Description = "rain" } },
                        Wind = new Wind { Speed = 10 },
                        Clouds = new Clouds { All = 90 }
                    },
                    new WeatherForecast
                    {
                        Dt = ((DateTimeOffset)DateTime.Now.AddDays(1)).ToUnixTimeSeconds(),
                        Main = new MainWeatherData { Temp = 20, FeelsLike = 19, Humidity = 60 },
                        Weather = new List<WeatherDescription> { new WeatherDescription { Description = "cloudy" } },
                        Wind = new Wind { Speed = 7 },
                        Clouds = new Clouds { All = 50 }
                    }
                }
        };

        var responseContent = JsonConvert.SerializeObject(weatherApiResponse);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            });

        // Setup the cache mock
        object cachedValue = null;
        _memoryCacheMock
            .Setup(m => m.TryGetValue(It.IsAny<object>(), out cachedValue))
            .Returns(false); // Simulate cache miss

        // Use the callback to simulate setting the cache
        _memoryCacheMock
            .Setup(m => m.CreateEntry(It.IsAny<object>()))
            .Returns(Mock.Of<ICacheEntry>());

        // Act
        var result = await _weatherService.GetWeatherAsync(city, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Forecasts); // Only one forecast is before the endDate
        Assert.Equal(15, result.Forecasts[0].Temp);
        Assert.Equal("rain", result.Forecasts[0].Description);
    }

    [Fact]
    public async Task GetWeatherAsync_InformAboutServiceUnavailability_WhenRequestFails()
    {
        // Arrange
        var city = "InvalidCity";
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Setup the cache mock
        object cachedValue = null;
        _memoryCacheMock
            .Setup(m => m.TryGetValue(It.IsAny<object>(), out cachedValue))
            .Returns(false); // Simulate cache miss

        // Use the callback to simulate setting the cache
        _memoryCacheMock
            .Setup(m => m.CreateEntry(It.IsAny<object>()))
            .Returns(Mock.Of<ICacheEntry>());

        // Act
        var result = await _weatherService.GetWeatherAsync(city, null);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Forecasts.Count() == 0);
        Assert.Equal("Service unavailable. Unable to fetch weather at the moment.", result.City);
    }
}
