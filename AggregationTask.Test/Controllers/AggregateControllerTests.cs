using AggregationTask.API.Controllers;
using AggregationTask.API.Services.GitHubService;
using AggregationTask.API.Services.NewsService;
using AggregationTask.API.Services.WeatherService;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using AggregationTask.API.Models.News;
using AggregationTask.API.Models.Weather;
using ServiceStack;
using AggregationTask.API.Models;

namespace AggregationTask.Test.Controllers
{
    public class AggregateControllerTests
    {
        private readonly Mock<IWeatherService> _mockWeatherService;
        private readonly Mock<INewsService> _mockNewsService;
        private readonly Mock<IGitHubService> _mockGitHubService;
        private readonly AggregateController _controller;

        public AggregateControllerTests()
        {
            _mockWeatherService = new Mock<IWeatherService>();
            _mockNewsService = new Mock<INewsService>();
            _mockGitHubService = new Mock<IGitHubService>();

            _controller = new AggregateController(_mockNewsService.Object, _mockWeatherService.Object, _mockGitHubService.Object);
        }

        [Fact]
        public async Task GetAggregatedData_ReturnsOkResult_WhenAllServicesReturnData()
        {
            // Arrange
            var city = "London";
            var weatherEndDate = DateTime.Now;
            var category = "Technology";
            var newsFrom = DateTime.Now.AddDays(-7);
            var githubUser = "testUser";

            _mockWeatherService.Setup(s => s.GetWeatherAsync(city, weatherEndDate))
                .ReturnsAsync(new WeatherData { City = city });

            _mockNewsService.Setup(s => s.GetNewsAsync(category, newsFrom))
                .ReturnsAsync(new NewsData { TotalResults = 10 });

            _mockGitHubService.Setup(s => s.GetRepositoriesAsync(githubUser, null, null))
                .ReturnsAsync(new List<GithubRepo> { new GithubRepo { Name = "Repo1" } });

            // Act
            var actionResult = await _controller.GetAggregatedData(city, weatherEndDate, category, newsFrom, githubUser);

            //// Assert
            Assert.IsType<OkObjectResult>(actionResult.Result);
            var aggregatedData = GetObjectResultContent<AggregatedDataResponse>(actionResult);

            Assert.NotNull(aggregatedData);
            Assert.Equal("London", aggregatedData.Weather.City);
            Assert.Equal(10, aggregatedData.News.TotalResults);
            Assert.Single(aggregatedData.Repositories);
        }

        [Fact]
        public async Task GetAggregatedData_ReturnsEmptyResult_WhenNoDataFromServices()
        {
            // Arrange
            _mockWeatherService.Setup(s => s.GetWeatherAsync(It.IsAny<string>(), It.IsAny<DateTime?>()))
                .ReturnsAsync((WeatherData)null);
            _mockNewsService.Setup(s => s.GetNewsAsync(It.IsAny<string>(), It.IsAny<DateTime?>()))
                .ReturnsAsync((NewsData)null);
            _mockGitHubService.Setup(s => s.GetRepositoriesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<GithubRepo>());

            // Act
            var actionResult = await _controller.GetAggregatedData("London", DateTime.Now, "Technology", DateTime.Now.AddDays(-7), "testUser");

            // Assert
            Assert.IsType<OkObjectResult>(actionResult.Result);
            var aggregatedData = GetObjectResultContent<AggregatedDataResponse>(actionResult);

            // Additional assertions for the properties of aggregatedData
            Assert.Null(aggregatedData.Weather);
            Assert.Null(aggregatedData.News);
            Assert.True(aggregatedData.Repositories.Count() == 0 );
        }

        private static T GetObjectResultContent<T>(ActionResult<T> result)
        {
            return (T)((ObjectResult)result.Result).Value;
        }
    }
}
