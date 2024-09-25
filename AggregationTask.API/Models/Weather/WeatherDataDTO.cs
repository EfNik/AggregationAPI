namespace AggregationTask.API.Models.Weather;
/// <summary>
/// Represents weather data for a specified city.
/// </summary>
public class WeatherData
{
    /// <summary>
    /// The name of the city for which the weather forecast is provided.
    /// </summary>
    public string City { get; set; }

    /// <summary>
    /// A list of weather forecasts for the upcoming days.
    /// </summary>
    public List<WeatherForecastDto> Forecasts { get; set; }
}

/// <summary>
/// Represents a single weather forecast for a specific date and time.
/// </summary>
public class WeatherForecastDto
{
    /// <summary>
    /// The timestamp of the forecast in UNIX format.
    /// </summary>
    public long Dt { get; set; }

    /// <summary>
    /// The forecasted temperature in Celsius.
    /// </summary>
    public double Temp { get; set; }

    /// <summary>
    /// What the temperature feels like, in Celsius.
    /// </summary>
    public double FeelsLike { get; set; }

    /// <summary>
    /// A brief description of the weather conditions (e.g., "clear sky").
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The wind speed in meters per second.
    /// </summary>
    public double WindSpeed { get; set; }

    /// <summary>
    /// The percentage of cloud cover.
    /// </summary>
    public int Cloudiness { get; set; }

    /// <summary>
    /// The humidity percentage.
    /// </summary>
    public int Humidity { get; set; }
}