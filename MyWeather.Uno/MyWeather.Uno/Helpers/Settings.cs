// Helpers/Settings.cs
// Migration note: Replaced Xamarin.Essentials.Preferences with Windows.Storage.ApplicationData.Current.LocalSettings
// Reference: https://learn.microsoft.com/en-us/windows/uwp/get-started/settings-learning-track

using Windows.Storage;

namespace MyWeather.Uno.Helpers
{
    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
    public static class Settings
    {
        private static ApplicationDataContainer LocalSettings => ApplicationData.Current.LocalSettings;

        #region Setting Constants

        private const string IsImperialKey = "is_imperial";
        private static readonly bool IsImperialDefault = true;


        private const string UseCityKey = "use_city";
        private static readonly bool UseCityDefault = true;


        private const string CityKey = "city";
        private static readonly string CityDefault = "Seattle";

        #endregion


        public static bool IsImperial
        {
            get
            {
                if (LocalSettings.Values.ContainsKey(IsImperialKey))
                    return (bool)LocalSettings.Values[IsImperialKey];
                return IsImperialDefault;
            }
            set => LocalSettings.Values[IsImperialKey] = value;
        }


        public static bool UseCity
        {
            get
            {
                if (LocalSettings.Values.ContainsKey(UseCityKey))
                    return (bool)LocalSettings.Values[UseCityKey];
                return UseCityDefault;
            }
            set => LocalSettings.Values[UseCityKey] = value;
        }

        public static string City
        {
            get
            {
                if (LocalSettings.Values.ContainsKey(CityKey))
                    return (string)LocalSettings.Values[CityKey];
                return CityDefault;
            }
            set => LocalSettings.Values[CityKey] = value;
        }

    }
}
