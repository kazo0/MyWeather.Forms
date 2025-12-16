namespace MyWeatherApp.UnoFull.Services;

public interface ISettingsService
{
    bool IsImperial { get; set; }
    bool UseCity { get; set; }
    string City { get; set; }
}

public class SettingsService : ISettingsService
{
    private const string IsImperialKey = "is_imperial";
    private const bool IsImperialDefault = true;

    private const string UseCityKey = "use_city";
    private const bool UseCityDefault = true;

    private const string CityKey = "city";
    private const string CityDefault = "Seattle";

    public bool IsImperial
    {
        get => GetPreference(IsImperialKey, IsImperialDefault);
        set => SetPreference(IsImperialKey, value);
    }

    public bool UseCity
    {
        get => GetPreference(UseCityKey, UseCityDefault);
        set => SetPreference(UseCityKey, value);
    }

    public string City
    {
        get => GetPreference(CityKey, CityDefault);
        set => SetPreference(CityKey, value);
    }

    private T GetPreference<T>(string key, T defaultValue)
    {
        try
        {
#if __ANDROID__ || __IOS__ || __MACOS__
            if (typeof(T) == typeof(bool))
            {
                return (T)(object)Microsoft.Maui.Storage.Preferences.Get(key, (bool)(object)defaultValue!);
            }
            else if (typeof(T) == typeof(string))
            {
                return (T)(object)Microsoft.Maui.Storage.Preferences.Get(key, (string)(object)defaultValue!);
            }
#elif WINDOWS || HAS_UNO_SKIA
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localSettings.Values.TryGetValue(key, out var value))
            {
                if (value is T typedValue)
                    return typedValue;
            }
#endif
        }
        catch
        {
            // Return default on error
        }
        
        return defaultValue;
    }

    private void SetPreference<T>(string key, T value)
    {
        try
        {
#if __ANDROID__ || __IOS__ || __MACOS__
            if (typeof(T) == typeof(bool))
            {
                Microsoft.Maui.Storage.Preferences.Set(key, (bool)(object)value!);
            }
            else if (typeof(T) == typeof(string))
            {
                Microsoft.Maui.Storage.Preferences.Set(key, (string)(object)value!);
            }
#elif WINDOWS || HAS_UNO_SKIA
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[key] = value;
#endif
        }
        catch
        {
            // Ignore errors during set
        }
    }
}
