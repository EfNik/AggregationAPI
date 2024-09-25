using AggregationTask.API.Models.Statistics;

namespace AggregationTask.API.Services.StatisticsService;
public interface IStatisticsService
{
    void LogApiCall(string apiName, TimeSpan responseTime);
    Dictionary<string, ApiStats> GetStatistics();
}

