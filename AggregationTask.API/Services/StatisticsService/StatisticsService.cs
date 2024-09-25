using AggregationTask.API.Models.Statistics;
using AggregationTask.API.Services.StatisticsService;

namespace AggregationTask.API.Services.Statistics;
public class StatisticsService : IStatisticsService
{
    private readonly Dictionary<string, ApiStats> _apiStatistics = new();

    public void LogApiCall(string apiName, TimeSpan responseTime)
    {
        if (!_apiStatistics.ContainsKey(apiName))
        {
            _apiStatistics[apiName] = new ApiStats();
        }

        var stats = _apiStatistics[apiName];
        stats.TotalRequests++;
        stats.TotalResponseTime += responseTime.TotalMilliseconds;

        if (responseTime.TotalMilliseconds < 100)
        {
            stats.Fast++;
        }
        else if (responseTime.TotalMilliseconds <= 200)
        {
            stats.Average++;
        }
        else
        {
            stats.Slow++;
        }
    }

    public Dictionary<string, ApiStats> GetStatistics()
    {
        return _apiStatistics;
    }
}
