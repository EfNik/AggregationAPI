using AggregationTask.API.Services.GitHubService;
using AggregationTask.API.Services.NewsService;
using AggregationTask.API.Services.StatisticsService;
using global::AggregationTask.API.Services.GitHubService;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AggregationTask.Test.Services;
   

public class GitHubServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly GitHubService _gitHubService;
    private readonly Mock<IMemoryCache> _memoryCacheMock;
    private readonly IStatisticsService _statisticsService;
    private readonly Mock<ILogger<GitHubService>> _mockLogger;

    public GitHubServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://api.github.com/")
        };
        _memoryCacheMock = new Mock<IMemoryCache>();
        _mockLogger = new Mock<ILogger<GitHubService>>();
        _statisticsService = Mock.Of<IStatisticsService>();
        _gitHubService = new GitHubService(_httpClient, _memoryCacheMock.Object, _statisticsService, _mockLogger.Object);
    }

    [Fact]
    public async Task GetRepositoriesAsync_ReturnsRepositories_SuccessfulResponse()
    {
        // Arrange
        var username = "testuser";
        var responseContent = JsonConvert.SerializeObject(new List<GithubRepo>
            {
                new GithubRepo { Name = "Repo1", Created_At = DateTime.UtcNow, Updated_At = DateTime.UtcNow },
                new GithubRepo { Name = "Repo2", Created_At = DateTime.UtcNow, Updated_At = DateTime.UtcNow }
            });

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
        var result = await _gitHubService.GetRepositoriesAsync(username);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Repo1", result[0].Name);
        Assert.Equal("Repo2", result[1].Name);
    }

    [Fact]
    public async Task GetRepositoriesAsync_FiltersRepositories_ByName()
    {
        // Arrange
        var username = "testuser";
        var responseContent = JsonConvert.SerializeObject(new List<GithubRepo>
            {
                new GithubRepo { Name = "Repo1", Created_At = DateTime.UtcNow, Updated_At = DateTime.UtcNow },
                new GithubRepo { Name = "FilteredRepo", Created_At = DateTime.UtcNow, Updated_At = DateTime.UtcNow }
            });

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
        var result = await _gitHubService.GetRepositoriesAsync(username, filterBy: "Filtered");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result); // Only one repository should match the filter
        Assert.Equal("FilteredRepo", result[0].Name);
    }

    [Fact]
    public async Task GetRepositoriesAsync_SortsRepositories_ByName()
    {
        // Arrange
        var username = "testuser";
        var responseContent = JsonConvert.SerializeObject(new List<GithubRepo>
            {
                new GithubRepo { Name = "RepoB", Created_At = DateTime.UtcNow, Updated_At = DateTime.UtcNow },
                new GithubRepo { Name = "RepoA", Created_At = DateTime.UtcNow, Updated_At = DateTime.UtcNow }
            });

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
        var result = await _gitHubService.GetRepositoriesAsync(username, sortBy: "name");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("RepoA", result[0].Name); // RepoA should come before RepoB
        Assert.Equal("RepoB", result[1].Name);
    }

    [Fact]
    public async Task GetRepositoriesAsync_InformAboutServiceUnavailability_WhenRequestFails()
    {
        // Arrange
        var username = "invaliduser";
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

        // Act
        var result = await _gitHubService.GetRepositoriesAsync(username);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count() == 1);
        var returnedRepo = result[0];
        Assert.Equal("Service unavailable.", returnedRepo.Name);
    }

}


