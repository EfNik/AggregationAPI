using AggregationTask.API.Models.Weather;

namespace AggregationTask.API.Services.WeatherService;
public interface IWeatherService
{
    Task<WeatherData> GetWeatherAsync(string city, DateTime? endDate);
}
 