using AggregationTask.API.Models.News;
using AggregationTask.API.Services.NewsService;
using AggregationTask.API.Services.Statistics;
using AggregationTask.API.Services.StatisticsService;
using Castle.Core.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AggregationTask.Test.Services
{
    public class NewsServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly NewsService _newsService;
        private readonly IStatisticsService _statisticsService;
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly Mock<ILogger<NewsService>> _mockLogger;

        public NewsServiceTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://newsapi.org/v2/")
            };
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(c => c["APIKeys:News:Key"]).Returns("test-news-api-key");
            _memoryCacheMock = new Mock<IMemoryCache>();
            _mockLogger = new Mock<ILogger<NewsService>>();
            _statisticsService = Mock.Of<IStatisticsService>();
            _newsService = new NewsService(_httpClient, _configurationMock.Object, _memoryCacheMock.Object, _statisticsService, _mockLogger.Object);
        }

        [Fact]
        public async Task GetNewsAsync_ReturnsNewsData_SuccessfulResponse()
        {
            // Arrange
            var category = "technology";
            var from = DateTime.Now.AddDays(-2);
            var newsApiResponse = new
            {
                Status = "ok",
                TotalResults = 1,
                Articles = new List<NewsArticleDto>
                {
                    new NewsArticleDto
                    {
                        SourceName =  "Test Source" , // Ensure Source is not null
                        Author = "Test Author",
                        Title = "Test Title",
                        Description = "Test Description",
                        Url = "https://example.com/article",
                        UrlToImage = "https://example.com/image.jpg",
                        PublishedAt = DateTime.Now, // Ensure PublishedAt is not null
                        Content = "Test Content"
                    }
                }
            };

            var responseContent = JsonConvert.SerializeObject(newsApiResponse);

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
            var result = await _newsService.GetNewsAsync(category, from);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalResults);
            Assert.Single(result.Articles);
            var article = result.Articles[0];
            Assert.Equal("Test Source", article.SourceName);
            Assert.Equal("Test Author", article.Author);
            Assert.Equal("Test Title", article.Title);
            Assert.Equal("Test Description", article.Description);
        }

        [Fact]
        public async Task GetNewsAsync_FiltersOutRemovedArticles()
        {
            // Arrange
            var category = "general";
            var from = DateTime.Now.AddDays(-2);

            var newsApiResponse = new
            {
                Status = "ok",
                TotalResults = 2,
                Articles = new List<object>
                {
                    new
                    {
                        Source = new { Name = "Valid Source" },
                        Author = "Valid Author",
                        Title = "Valid Title",
                        Description = "Valid Description",
                        Url = "https://example.com/valid-article",
                        UrlToImage = "https://example.com/valid-image.jpg",
                        PublishedAt = DateTime.Now.AddDays(-1),
                        Content = "Valid Content"
                    },
                    new
                    {
                        Source = new { Name = "[Removed]" },
                        Author = "Removed Author",
                        Title = "[Removed]",
                        Description = "[Removed]",
                        Url = "https://example.com/removed-article",
                        UrlToImage = "https://example.com/removed-image.jpg",
                        PublishedAt = DateTime.Now.AddDays(-1),
                        Content = "[Removed]"
                    }
                }
            };

            var responseContent = JsonConvert.SerializeObject(newsApiResponse);

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
            var result = await _newsService.GetNewsAsync(category, from);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Articles); // Only the valid article should remain after filtering
            var validArticle = result.Articles[0];
            Assert.Equal("Valid Title", validArticle.Title);
            Assert.Equal("Valid Description", validArticle.Description);
            Assert.Equal("Valid Content", validArticle.Content);
        }

        [Fact]
        public async Task GetNewsAsync_InformAboutServiceUnavailability_WhenRequestFails()
        {
            // Arrange
            var category = "InvalidCategory";
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
            var result = await _newsService.GetNewsAsync(category, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.TotalResults == 0);
            Assert.True(result.Articles.Count == 1);
            var returnedArticle = result.Articles[0];
            Assert.Equal("Service unavailable", returnedArticle.Title);
        }
    }
}
