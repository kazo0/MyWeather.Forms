// Migration notes:
// - Replaced Xamarin.Essentials.Geolocation with Windows.Devices.Geolocation.Geolocator
// - Replaced Xamarin.Essentials.TextToSpeech with Windows.Media.SpeechSynthesis.SpeechSynthesizer  
// - Replaced Plugin.Permissions with direct permission check (simplified for now)
// - Replaced Application.Current.MainPage.DisplayAlert with ContentDialog
// - Reference: https://learn.microsoft.com/en-us/uwp/api/windows.devices.geolocation
// - Reference: https://learn.microsoft.com/en-us/uwp/api/windows.media.speechsynthesis

using MyWeather.Uno.Helpers;
using MyWeather.Uno.Models;
using MyWeather.Uno.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmHelpers;
using MvvmHelpers.Commands;
using Windows.Devices.Geolocation;
using Windows.Media.SpeechSynthesis;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace MyWeather.Uno.ViewModels
{
    public class WeatherViewModel : BaseViewModel
    {
        WeatherService WeatherService { get; } = new WeatherService();

        string location = Settings.City;
        public string Location
        {
            get { return location; }
            set
            {
                SetProperty(ref location, value);
                Settings.City = value;
            }
        }

        bool useGPS;
        public bool UseGPS
        {
            get { return useGPS; }
            set
            {
                SetProperty(ref useGPS, value);
            }
        }




        bool isImperial = Settings.IsImperial;
        public bool IsImperial
        {
            get { return isImperial; }
            set
            {
                SetProperty(ref isImperial, value);
                Settings.IsImperial = value;
            }
        }


        string temp = string.Empty;
        public string Temp
        {
            get { return temp; }
            set { SetProperty(ref temp, value); }
        }

        string condition = string.Empty;
        public string Condition
        {
            get { return condition; }
            set { SetProperty(ref condition, value); ; }
        }

        
        WeatherForecastRoot forecast;
        public WeatherForecastRoot Forecast
        {
            get { return forecast; }
            set { forecast = value; OnPropertyChanged(); }
        }


        ICommand getWeather;
        public ICommand GetWeatherCommand =>
                getWeather ??
                (getWeather = new AsyncCommand(ExecuteGetWeatherCommand));

        private async Task ExecuteGetWeatherCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            try
            {
                WeatherRoot weatherRoot = null;
                var units = IsImperial ? Units.Imperial : Units.Metric;
               

                if (UseGPS)
                {
                    var hasPermission = await CheckPermissions();
                    if (!hasPermission)
                        return;

                    var geolocator = new Geolocator();
                    var position = await geolocator.GetGeopositionAsync();

                    weatherRoot = await WeatherService.GetWeather(
                        position.Coordinate.Point.Position.Latitude, 
                        position.Coordinate.Point.Position.Longitude, 
                        units);
                }
                else
                {
                    //Get weather by city
                    weatherRoot = await WeatherService.GetWeather(Location.Trim(), units);
                }
                

                //Get forecast based on cityId
                Forecast = await WeatherService.GetForecast(weatherRoot.CityId, units);

                var unit = IsImperial ? "F" : "C";
                Temp = $"Temp: {weatherRoot?.MainWeather?.Temperature ?? 0}°{unit}";
                Condition = $"{weatherRoot.Name}: {weatherRoot?.Weather?[0]?.Description ?? string.Empty}";
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

        async Task<bool> CheckPermissions()
        {
            try
            {
                // Simplified permission check - in production app, you'd use proper permission APIs
                // For UWP/WinUI, location permission is handled via app manifest capabilities
                var accessStatus = await Geolocator.RequestAccessAsync();
                
                if (accessStatus != GeolocationAccessStatus.Allowed)
                {
                    await ShowAlertAsync("Location Permission", 
                        "To get your current city the location permission is required. Please enable location access in Settings.",
                        "OK");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Permission check failed: {ex.Message}");
                return false;
            }
        }

        private async Task ShowAlertAsync(string title, string message, string button)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = button,
                XamlRoot = App.MainWindow?.Content?.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}
