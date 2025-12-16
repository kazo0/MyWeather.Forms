using System.Text.Json;
using MyWeatherApp.UnoFull.Models;

namespace MyWeatherApp.UnoFull.Services;

public enum Units
{
    Imperial,
    Metric
}

public interface IWeatherService
{
    Task<WeatherRoot?> GetWeatherAsync(double latitude, double longitude, Units units = Units.Imperial);
    Task<WeatherRoot?> GetWeatherAsync(string city, Units units = Units.Imperial);
    Task<WeatherForecastRoot?> GetForecastAsync(int cityId, Units units = Units.Imperial);
}

public class WeatherService : IWeatherService
{
    private const string WeatherCoordinatesUri = "https://api.openweathermap.org/data/2.5/weather?lat={0}&lon={1}&units={2}&appid=INSERT-KEY-HERE";
    private const string WeatherCityUri = "https://api.openweathermap.org/data/2.5/weather?q={0}&units={1}&appid=INSERT-KEY-HERE";
    private const string ForecastUri = "https://api.openweathermap.org/data/2.5/forecast?id={0}&units={1}&appid=INSERT-KEY-HERE";

    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public WeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<WeatherRoot?> GetWeatherAsync(double latitude, double longitude, Units units = Units.Imperial)
    {
        var url = string.Format(WeatherCoordinatesUri, latitude, longitude, units.ToString().ToLower());
        var json = await _httpClient.GetStringAsync(url);

        if (string.IsNullOrWhiteSpace(json))
            return null;

        return JsonSerializer.Deserialize<WeatherRoot>(json, _jsonOptions);
    }

    public async Task<WeatherRoot?> GetWeatherAsync(string city, Units units = Units.Imperial)
    {
        var url = string.Format(WeatherCityUri, city, units.ToString().ToLower());
        var json = await _httpClient.GetStringAsync(url);

        if (string.IsNullOrWhiteSpace(json))
            return null;

        return JsonSerializer.Deserialize<WeatherRoot>(json, _jsonOptions);
    }

    public async Task<WeatherForecastRoot?> GetForecastAsync(int cityId, Units units = Units.Imperial)
    {
        var url = string.Format(ForecastUri, cityId, units.ToString().ToLower());
        var json = await _httpClient.GetStringAsync(url);

        if (string.IsNullOrWhiteSpace(json))
            return null;

        return JsonSerializer.Deserialize<WeatherForecastRoot>(json, _jsonOptions);
    }
}
