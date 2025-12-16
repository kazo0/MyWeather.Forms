using MyWeatherApp.UnoFull.Models;
using MyWeatherApp.UnoFull.Services;

namespace MyWeatherApp.UnoFull.Presentation;

public partial record WeatherModel(
    IWeatherService WeatherService,
    ISettingsService Settings,
    IGeolocationService GeolocationService,
    IPermissionService PermissionService,
    ITextToSpeechService TextToSpeechService)
{
    public string Location
    {
        get => Settings.City;
        set => Settings.City = value;
    }
    
    public bool UseGPS { get; set; }
    
    public bool IsImperial
    {
        get => Settings.IsImperial;
        set => Settings.IsImperial = value;
    }
    
    public string Temp { get; set; } = string.Empty;
    
    public string Condition { get; set; } = string.Empty;
    
    public bool IsBusy { get; set; }
    
    public List<WeatherRoot> Forecast { get; set; } = new();

    public async ValueTask GetWeather()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        
        try
        {
            WeatherRoot? weatherRoot = null;
            var units = IsImperial ? Units.Imperial : Units.Metric;

            if (UseGPS)
            {
                // Check and request location permission
                var hasPermission = await PermissionService.CheckAndRequestLocationPermissionAsync();
                if (!hasPermission)
                {
                    Temp = "Location permission denied";
                    return;
                }

                // Try to get last known location first
                var position = await GeolocationService.GetLastKnownLocationAsync();
                
                // If no cached location, get current location
                if (position == null)
                {
                    position = await GeolocationService.GetLocationAsync(TimeSpan.FromSeconds(30));
                }

                if (position != null)
                {
                    weatherRoot = await WeatherService.GetWeatherAsync(position.Latitude, position.Longitude, units);
                }
            }
            else
            {
                weatherRoot = await WeatherService.GetWeatherAsync(Location.Trim(), units);
            }

            if (weatherRoot != null)
            {
                // Get forecast based on cityId
                var forecast = await WeatherService.GetForecastAsync(weatherRoot.CityId, units);
                Forecast = forecast?.Items ?? new List<WeatherRoot>();

                var unit = IsImperial ? "F" : "C";
                Temp = $"Temp: {weatherRoot?.MainWeather?.Temperature ?? 0}°{unit}";
                Condition = $"{weatherRoot.Name}: {weatherRoot?.Weather?[0]?.Description ?? string.Empty}";

                // Speak the weather information
                await TextToSpeechService.SpeakAsync($"{Temp} {Condition}");
            }
        }
        catch (Exception ex)
        {
            Temp = "Unable to get Weather";
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
}

