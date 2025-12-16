using System.Text.Json.Serialization;

namespace MyWeatherApp.UnoFull.Models;

public class Coord
{
    [JsonPropertyName("lon")]
    public double Longitude { get; set; } = 0;

    [JsonPropertyName("lat")]
    public double Latitude { get; set; } = 0;
}

public class Sys
{
    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;
}

public class Weather
{
    [JsonPropertyName("id")]
    public int Id { get; set; } = 0;

    [JsonPropertyName("main")]
    public string Main { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;
}

public class Main
{
    [JsonPropertyName("temp")]
    public double Temperature { get; set; } = 0;
    
    [JsonPropertyName("pressure")]
    public double Pressure { get; set; } = 0;

    [JsonPropertyName("humidity")]
    public double Humidity { get; set; } = 0;
    
    [JsonPropertyName("temp_min")]
    public double MinTemperature { get; set; } = 0;

    [JsonPropertyName("temp_max")]
    public double MaxTemperature { get; set; } = 0;
}

public class Wind
{
    [JsonPropertyName("speed")]
    public double Speed { get; set; } = 0;

    [JsonPropertyName("deg")]
    public double WindDirectionDegrees { get; set; } = 0;
}

public class Clouds
{
    [JsonPropertyName("all")]
    public int CloudinessPercent { get; set; } = 0;
}

public class WeatherRoot
{
    [JsonPropertyName("coord")]
    public Coord Coordinates { get; set; } = new Coord();

    [JsonPropertyName("sys")]
    public Sys System { get; set; } = new Sys();

    [JsonPropertyName("weather")]
    public List<Weather> Weather { get; set; } = new List<Weather>();

    [JsonPropertyName("main")]
    public Main MainWeather { get; set; } = new Main();

    [JsonPropertyName("wind")]
    public Wind Wind { get; set; } = new Wind();

    [JsonPropertyName("clouds")]
    public Clouds Clouds { get; set; } = new Clouds();

    [JsonPropertyName("id")]
    public int CityId { get; set; } = 0;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("dt_txt")]
    public string Date { get; set; } = string.Empty;

    [JsonIgnore]
    public string DisplayDate => string.IsNullOrEmpty(Date) ? string.Empty : DateTime.Parse(Date).ToLocalTime().ToString("g");
    
    [JsonIgnore]
    public string DisplayTemp => $"Temp: {MainWeather?.Temperature ?? 0}° {Weather?[0]?.Main ?? string.Empty}";
    
    [JsonIgnore]
    public string DisplayIcon => $"http://openweathermap.org/img/w/{Weather?[0]?.Icon}.png";
}

public class WeatherForecastRoot
{
    [JsonPropertyName("city")]
    public City? City { get; set; }
    
    [JsonPropertyName("cod")]
    public string? Vod { get; set; }
    
    [JsonPropertyName("message")]
    public double Message { get; set; }
    
    [JsonPropertyName("cnt")]
    public int Cnt { get; set; }
    
    [JsonPropertyName("list")]
    public List<WeatherRoot>? Items { get; set; }
}

public class City
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("coord")]
    public Coord? Coord { get; set; }
    
    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;
    
    [JsonPropertyName("population")]
    public int Population { get; set; }
    
    [JsonPropertyName("sys")]
    public Sys? Sys { get; set; }
}
