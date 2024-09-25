namespace AggregationTask.API.Models.Weather;
public class WeatherApiResponse
{
    public string Cod { get; set; }
    public int Message { get; set; }
    public int Cnt { get; set; }
    public List<WeatherForecast> List { get; set; }
    public CityInfo City { get; set; }
}

public class WeatherForecast
{
    public long Dt { get; set; }
    public MainWeatherData Main { get; set; }
    public List<WeatherDescription> Weather { get; set; }
    public Clouds Clouds { get; set; }
    public Wind Wind { get; set; }
    public int Visibility { get; set; }
    public double Pop { get; set; } // Probability of precipitation
    public Sys Sys { get; set; }
    public string DtTxt { get; set; } // Date and time of forecast
}

public class MainWeatherData
{
    public double Temp { get; set; }
    public double FeelsLike { get; set; }
    public double TempMin { get; set; }
    public double TempMax { get; set; }
    public int Pressure { get; set; }
    public int SeaLevel { get; set; }
    public int GrndLevel { get; set; }
    public int Humidity { get; set; }
}

public class WeatherDescription
{
    public int Id { get; set; }
    public string Main { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
}

public class Clouds
{
    public int All { get; set; }
}

public class Wind
{
    public double Speed { get; set; }
    public int Deg { get; set; }
    public double Gust { get; set; }
}

public class Sys
{
    public string Pod { get; set; }
}

public class CityInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Coord Coord { get; set; }
    public string Country { get; set; }
    public int Population { get; set; }
    public int Timezone { get; set; }
    public long Sunrise { get; set; }
    public long Sunset { get; set; }
}

public class Coord
{
    public double Lat { get; set; }
    public double Lon { get; set; }
}
