namespace AggregationTask.API.Models.Statistics;
public class ApiStatisticsResponse
{
    /// <summary>The name of the API (e.g., "News", "Weather").</summary>
    public string ApiName { get; set; }

    /// <summary>The total number of requests made to the API.</summary>
    public int TotalRequests { get; set; }

    /// <summary>The average response time for the API (in milliseconds).</summary>
    public double AverageResponseTime { get; set; }

    /// <summary>The number of fast requests (less than 100ms).</summary>
    public int FastRequests { get; set; }

    /// <summary>The number of average-speed requests (between 100-200ms).</summary>
    public int AverageRequests { get; set; }

    /// <summary>The number of slow requests (greater than 200ms).</summary>
    public int SlowRequests { get; set; }
}
