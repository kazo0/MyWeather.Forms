namespace MyWeatherApp.UnoFull.Services;

public record GeolocationPosition(double Latitude, double Longitude);

public interface IGeolocationService
{
    Task<GeolocationPosition?> GetLastKnownLocationAsync();
    Task<GeolocationPosition?> GetLocationAsync(TimeSpan timeout);
}

public interface IPermissionService
{
    Task<bool> CheckAndRequestLocationPermissionAsync();
}

public interface ITextToSpeechService
{
    Task SpeakAsync(string text);
}
