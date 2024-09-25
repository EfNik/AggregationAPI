using AggregationTask.API.Services.StatisticsService;
using AggregationTask.API.Models.Statistics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AggregationTask.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService statisticsService;

    public StatisticsController(IStatisticsService statisticsService)
    {
        this.statisticsService = statisticsService;
    }

    /// <summary>Retrieve API request statistics.</summary>
    /// <returns>Statistics about API request performance, including total requests, average response time, and counts of fast, average, and slow requests.</returns>
    /// <response code="200">Returns the API statistics</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<ApiStatisticsResponse>>> GetApiStatistics()
    {
        var stats = statisticsService.GetStatistics();

        var result = stats.Select(stat => new ApiStatisticsResponse
        {
            ApiName = stat.Key,
            TotalRequests = stat.Value.TotalRequests,
            AverageResponseTime = stat.Value.GetAverageResponseTime(),
            FastRequests = stat.Value.Fast,
            AverageRequests = stat.Value.Average,
            SlowRequests = stat.Value.Slow
        }).ToList();

        return Ok(result);
    }
}


