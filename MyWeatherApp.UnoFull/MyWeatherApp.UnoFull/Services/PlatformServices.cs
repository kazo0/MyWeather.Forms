namespace MyWeatherApp.UnoFull.Services;

public class GeolocationService : IGeolocationService
{
    public async Task<GeolocationPosition?> GetLastKnownLocationAsync()
    {
#if __ANDROID__ || __IOS__ || __MACOS__
        try
        {
            var location = await Microsoft.Maui.Devices.Sensors.Geolocation.Default.GetLastKnownLocationAsync();
            if (location != null)
            {
                return new GeolocationPosition(location.Latitude, location.Longitude);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unable to get last known location: {ex.Message}");
        }
#endif
        return null;
    }

    public async Task<GeolocationPosition?> GetLocationAsync(TimeSpan timeout)
    {
#if __ANDROID__ || __IOS__ || __MACOS__
        try
        {
            var request = new Microsoft.Maui.Devices.Sensors.GeolocationRequest(
                Microsoft.Maui.Devices.Sensors.GeolocationAccuracy.Medium,
                timeout);
            
            var location = await Microsoft.Maui.Devices.Sensors.Geolocation.Default.GetLocationAsync(request);
            if (location != null)
            {
                return new GeolocationPosition(location.Latitude, location.Longitude);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unable to get location: {ex.Message}");
        }
#endif
        return null;
    }
}

public class PermissionService : IPermissionService
{
    public async Task<bool> CheckAndRequestLocationPermissionAsync()
    {
#if __ANDROID__ || __IOS__ || __MACOS__
        try
        {
            var status = await Microsoft.Maui.ApplicationModel.Permissions.CheckStatusAsync<Microsoft.Maui.ApplicationModel.Permissions.LocationWhenInUse>();
            
            if (status != Microsoft.Maui.ApplicationModel.PermissionStatus.Granted)
            {
                status = await Microsoft.Maui.ApplicationModel.Permissions.RequestAsync<Microsoft.Maui.ApplicationModel.Permissions.LocationWhenInUse>();
            }
            
            return status == Microsoft.Maui.ApplicationModel.PermissionStatus.Granted;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Permission check failed: {ex.Message}");
            return false;
        }
#else
        // For desktop/wasm, assume permission is granted
        return true;
#endif
    }
}

public class TextToSpeechService : ITextToSpeechService
{
    public async Task SpeakAsync(string text)
    {
#if __ANDROID__ || __IOS__ || __MACOS__
        try
        {
            await Microsoft.Maui.Media.TextToSpeech.Default.SpeakAsync(text);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Text to speech failed: {ex.Message}");
        }
#else
        // Not supported on other platforms
        await Task.CompletedTask;
#endif
    }
}
