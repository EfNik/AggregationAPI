namespace AggregationTask.API.Models.Statistics;
public class ApiStats
{
    public int TotalRequests { get; set; }
    public double TotalResponseTime { get; set; } // in ms
    public int Fast { get; set; }
    public int Average { get; set; }
    public int Slow { get; set; }

    public double GetAverageResponseTime() => TotalRequests == 0 ? 0 : TotalResponseTime / TotalRequests;
}
