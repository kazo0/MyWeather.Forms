using MyWeather.Uno.ViewModels;
using Microsoft.UI.Xaml.Controls;
using System.Windows.Input;

namespace MyWeather.Uno.View
{
    public sealed partial class ForecastView : Page
    {
        public ForecastView()
        {
            this.InitializeComponent();
        }

        private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            using var deferral = args.GetDeferral();
            
            if (DataContext is WeatherViewModel viewModel)
            {
                if (viewModel.GetWeatherCommand is ICommand command && command.CanExecute(null))
                {
                    command.Execute(null);
                    deferral.Complete();
                }
            }
        }
    }
}
