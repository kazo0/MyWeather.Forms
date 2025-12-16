# Side-by-Side Code Comparison
## Xamarin.Forms vs Uno Platform Migration

This document shows exact code comparisons for each migrated component.

---

## 1. App Entry Point

### Xamarin.Forms (App.cs)
```csharp
using MyWeather.View;
using Xamarin.Forms;

namespace MyWeather
{
    public class App : Application
    {
        public App()
        {
            MainPage = new NavigationPage(new MainTabs())
            {
                BarBackgroundColor = Color.FromHex("3498db"),
                BarTextColor = Color.White
            };
        }

        protected override void OnStart() { }
        protected override void OnSleep() { }
        protected override void OnResume() { }
    }
}
```

### Uno Platform (App.xaml.cs)
```csharp
using MyWeather.Uno.View;

namespace MyWeather.Uno;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
    }

    public static new Window MainWindow { get; set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new Window();
        
        if (MainWindow.Content is not Frame rootFrame)
        {
            rootFrame = new Frame();
            MainWindow.Content = rootFrame;
            rootFrame.NavigationFailed += OnNavigationFailed;
        }

        if (rootFrame.Content == null)
        {
            rootFrame.Navigate(typeof(MainTabs), args.Arguments);
        }

        MainWindow.Activate();
    }
}
```

**Key Differences:**
- XF: `MainPage` property | Uno: `Frame.Navigate()`
- XF: `NavigationPage` wrapper | Uno: `Frame` already handles navigation
- XF: Direct instantiation | Uno: Type-based navigation

---

## 2. Settings Helper

### Xamarin.Forms
```csharp
using Xamarin.Essentials;

namespace MyWeather.Helpers
{
    public static class Settings
    {
        private const string IsImperialKey = "is_imperial";
        private static readonly bool IsImperialDefault = true;

        public static bool IsImperial
        {
            get => Preferences.Get(IsImperialKey, IsImperialDefault);
            set => Preferences.Set(IsImperialKey, value);
        }

        public static string City
        {
            get => Preferences.Get(CityKey, CityDefault);
            set => Preferences.Set(CityKey, value);
        }
    }
}
```

### Uno Platform
```csharp
using Windows.Storage;

namespace MyWeather.Uno.Helpers
{
    public static class Settings
    {
        private static ApplicationDataContainer LocalSettings => 
            ApplicationData.Current.LocalSettings;

        private const string IsImperialKey = "is_imperial";
        private static readonly bool IsImperialDefault = true;

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
```

**Key Differences:**
- XF: `Preferences.Get/Set()` | Uno: `ApplicationData.Current.LocalSettings.Values[]`
- XF: Built-in default handling | Uno: Manual `ContainsKey` check
- XF: Generic type inference | Uno: Explicit casting

---

## 3. ViewModel GPS Logic

### Xamarin.Forms
```csharp
using Xamarin.Essentials;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

if (UseGPS)
{
    var hasPermission = await CheckPermissions();
    if (!hasPermission)
        return;

    var position = await Geolocation.GetLastKnownLocationAsync();

    if (position == null)
    {
        position = await Geolocation.GetLocationAsync(new GeolocationRequest
        {
            DesiredAccuracy = GeolocationAccuracy.Medium,
            Timeout = TimeSpan.FromSeconds(30)
        });
    }
    
    weatherRoot = await WeatherService.GetWeather(
        position.Latitude, 
        position.Longitude, 
        units);
}

async Task<bool> CheckPermissions()
{
    var permissionStatus = await CrossPermissions.Current
        .CheckPermissionStatusAsync(Permission.Location);
    
    if (permissionStatus == PermissionStatus.Denied)
    {
        if (Device.RuntimePlatform == Device.iOS)
        {
            var result = await Application.Current?.MainPage?.DisplayAlert(
                "Location Permission", 
                "To get your current city...", 
                "Settings", 
                "Maybe Later");
            if (result)
                CrossPermissions.Current.OpenAppSettings();
            return false;
        }
    }

    if (permissionStatus != PermissionStatus.Granted)
    {
        var newStatus = await CrossPermissions.Current
            .RequestPermissionsAsync(Permission.Location);
        if (!newStatus.ContainsKey(Permission.Location) || 
            newStatus[Permission.Location] != PermissionStatus.Granted)
        {
            // Show alert
            return false;
        }
    }

    return true;
}
```

### Uno Platform
```csharp
using Windows.Devices.Geolocation;
using Microsoft.UI.Xaml.Controls;

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

async Task<bool> CheckPermissions()
{
    var accessStatus = await Geolocator.RequestAccessAsync();
    
    if (accessStatus != GeolocationAccessStatus.Allowed)
    {
        await ShowAlertAsync(
            "Location Permission", 
            "To get your current city the location permission is required...",
            "OK");
        return false;
    }

    return true;
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
```

**Key Differences:**
- XF: `Geolocation.GetLastKnownLocationAsync()` with fallback | Uno: `Geolocator.GetGeopositionAsync()`
- XF: `Plugin.Permissions` | Uno: `Geolocator.RequestAccessAsync()`
- XF: `position.Latitude` | Uno: `position.Coordinate.Point.Position.Latitude`
- XF: `DisplayAlert()` | Uno: `ContentDialog`
- XF: Platform-specific iOS handling | Uno: Unified permission API

---

## 4. MainTabs (Tab Navigation)

### Xamarin.Forms
```xaml
<?xml version="1.0" encoding="utf-8" ?>
<TabbedPage xmlns="http://xamarin.com/schemas/2014/forms"
            xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
            xmlns:views="clr-namespace:MyWeather.View"
            xmlns:viewmodels="clr-namespace:MyWeather.ViewModels"
            x:Class="MyWeather.View.MainTabs"
            Title="My Weather"
            xmlns:android="clr-namespace:Xamarin.Forms.PlatformConfiguration.AndroidSpecific;assembly=Xamarin.Forms.Core"
            android:TabbedPage.ToolbarPlacement="Bottom">
    
    <TabbedPage.BindingContext>
        <viewmodels:WeatherViewModel/>
    </TabbedPage.BindingContext>

    <views:WeatherView/>
    <views:ForecastView/>

</TabbedPage>
```

```csharp
namespace MyWeather.View
{
    public partial class MainTabs : TabbedPage
    {
        public MainTabs()
        {
            InitializeComponent();
        }
    }
}
```

### Uno Platform
```xaml
<Page x:Class="MyWeather.Uno.View.MainTabs"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:views="using:MyWeather.Uno.View"
      xmlns:viewmodels="using:MyWeather.Uno.ViewModels">
    
    <Page.DataContext>
        <viewmodels:WeatherViewModel/>
    </Page.DataContext>

    <Pivot Title="My Weather">
        <PivotItem Header="Weather">
            <views:WeatherView DataContext="{Binding}"/>
        </PivotItem>
        <PivotItem Header="Forecast">
            <views:ForecastView DataContext="{Binding}"/>
        </PivotItem>
    </Pivot>
</Page>
```

```csharp
namespace MyWeather.Uno.View
{
    public sealed partial class MainTabs : Page
    {
        public MainTabs()
        {
            this.InitializeComponent();
        }
    }
}
```

**Key Differences:**
- XF: `TabbedPage` root | Uno: `Page` containing `Pivot`
- XF: Child pages added directly | Uno: `PivotItem` wrappers with `Header`
- XF: `xmlns:android` for platform config | Uno: Platform handles natively
- XF: `clr-namespace:` | Uno: `using:`
- XF: Implicit tab headers from page Title | Uno: Explicit `PivotItem.Header`

---

## 5. WeatherView (Input Form)

### Xamarin.Forms
```xaml
<ContentPage xmlns="http://xamarin.com/schemas/2014/forms"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="MyWeather.View.WeatherView"
             Title="Weather">
    <StackLayout Padding="10" Spacing="10">
        <Entry Text="{Binding Location}">
            <Entry.Triggers>
                <DataTrigger TargetType="Entry"
                             Binding="{Binding UseGPS}"
                             Value="true">
                    <Setter Property="IsEnabled" Value="false"/>
                </DataTrigger>
            </Entry.Triggers>
        </Entry>

        <StackLayout Orientation="Horizontal" Spacing="10">
            <Label Text="Use GPS" VerticalTextAlignment="Center" 
                   HorizontalOptions="EndAndExpand"/>
            <Switch IsToggled="{Binding UseGPS}"/>
        </StackLayout>

        <StackLayout Orientation="Horizontal" Spacing="10">
            <Label Text="Use Imperial" VerticalTextAlignment="Center" 
                   HorizontalOptions="EndAndExpand"/>
            <Switch IsToggled="{Binding IsImperial}"/>
        </StackLayout>

        <Button Text="Get Weather" Command="{Binding GetWeatherCommand}"/>
        <Label Text="{Binding Temp}" FontSize="24"/>
        <Label Text="{Binding Condition}"/>
        <ActivityIndicator IsVisible="{Binding IsBusy}" 
                           IsRunning="{Binding IsBusy}"/>
    </StackLayout>
</ContentPage>
```

### Uno Platform
```xaml
<Page x:Class="MyWeather.Uno.View.WeatherView"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <StackPanel Padding="10" Spacing="10">
        <TextBox Text="{Binding Location, Mode=TwoWay}"/>

        <StackPanel Orientation="Horizontal" Spacing="10">
            <TextBlock Text="Use GPS" VerticalAlignment="Center" 
                       HorizontalAlignment="Right"/>
            <ToggleSwitch IsOn="{Binding UseGPS, Mode=TwoWay}" 
                          HorizontalAlignment="Left"/>
        </StackPanel>

        <StackPanel Orientation="Horizontal" Spacing="10">
            <TextBlock Text="Use Imperial" VerticalAlignment="Center" 
                       HorizontalAlignment="Right"/>
            <ToggleSwitch IsOn="{Binding IsImperial, Mode=TwoWay}" 
                          HorizontalAlignment="Left"/>
        </StackPanel>

        <Button Content="Get Weather" Command="{Binding GetWeatherCommand}"/>
        <TextBlock Text="{Binding Temp}" FontSize="24"/>
        <TextBlock Text="{Binding Condition}"/>
        <ProgressRing IsActive="{Binding IsBusy}" Visibility="{Binding IsBusy}"/>
    </StackPanel>
</Page>
```

**Key Differences:**
| Xamarin.Forms | Uno Platform | Notes |
|---------------|--------------|-------|
| `ContentPage` | `Page` | Root element |
| `Entry` | `TextBox` | Text input |
| `Label` | `TextBlock` | Static text |
| `Switch` | `ToggleSwitch` | Toggle control |
| `Button.Text` | `Button.Content` | Property name |
| `ActivityIndicator` | `ProgressRing` | Loading indicator |
| `IsToggled` | `IsOn` | Toggle property |
| `DataTrigger` | *(Removed)* | WinUI doesn't support |
| `HorizontalOptions` | `HorizontalAlignment` | Layout property |
| `VerticalTextAlignment` | `VerticalAlignment` | Alignment property |

---

## 6. ForecastView (List)

### Xamarin.Forms
```xaml
<ContentPage xmlns="http://xamarin.com/schemas/2014/forms"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="MyWeather.View.ForecastView"
             Title="Forecast">
    <ListView ItemsSource="{Binding Forecast.Items}"
              HasUnevenRows="True"
              CachingStrategy="RecycleElement"
              IsPullToRefreshEnabled="True"
              RefreshCommand="{Binding GetWeatherCommand}"
              IsRefreshing="{Binding IsBusy, Mode=OneWay}"
              x:Name="ListViewWeather">
        <ListView.SeparatorColor>
            <OnPlatform x:TypeArguments="Color" iOS="Transparent"/>
        </ListView.SeparatorColor>
        <ListView.ItemTemplate>
            <DataTemplate>
                <ViewCell>
                    <StackLayout Orientation="Horizontal" Padding="10,0,0,0">
                        <Image HeightRequest="44" 
                               WidthRequest="44" 
                               Source="{Binding DisplayIcon}"/>
                        <StackLayout Padding="10" Spacing="5">
                            <Label Text="{Binding DisplayTemp}"
                                   TextColor="#3498db"
                                   Style="{DynamicResource ListItemTextStyle}"/>
                            <Label Text="{Binding DisplayDate}" 
                                   Style="{DynamicResource ListItemDetailTextStyle}"/>
                        </StackLayout>
                    </StackLayout>
                </ViewCell>
            </DataTemplate>
        </ListView.ItemTemplate>
    </ListView>
</ContentPage>
```

```csharp
namespace MyWeather.View
{
    public partial class ForecastView : ContentPage
    {
        public ForecastView()
        {
            InitializeComponent();
            if (Device.RuntimePlatform != Device.UWP)
                Icon = new FileImageSource { File = "tab2.png" };

            ListViewWeather.ItemTapped += (sender, args) => 
                ListViewWeather.SelectedItem = null;
        }
    }
}
```

### Uno Platform
```xaml
<Page x:Class="MyWeather.Uno.View.ForecastView"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid>
        <ListView ItemsSource="{Binding Forecast.Items}"
                  x:Name="ListViewWeather"
                  SelectionMode="None">
            <ListView.ItemTemplate>
                <DataTemplate>
                    <Grid Padding="10,5,10,5">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="Auto"/>
                            <ColumnDefinition Width="*"/>
                        </Grid.ColumnDefinitions>
                        
                        <Image Grid.Column="0"
                               Height="44" 
                               Width="44" 
                               Source="{Binding DisplayIcon}"
                               Margin="0,0,10,0"/>
                        
                        <StackPanel Grid.Column="1" 
                                    Padding="10" 
                                    Spacing="5"
                                    VerticalAlignment="Center">
                            <TextBlock Text="{Binding DisplayTemp}"
                                       Foreground="#3498db"
                                       Style="{StaticResource BodyTextBlockStyle}"/>
                            <TextBlock Text="{Binding DisplayDate}" 
                                       Style="{StaticResource CaptionTextBlockStyle}"/>
                        </StackPanel>
                    </Grid>
                </DataTemplate>
            </ListView.ItemTemplate>
        </ListView>
    </Grid>
</Page>
```

```csharp
namespace MyWeather.Uno.View
{
    public sealed partial class ForecastView : Page
    {
        public ForecastView()
        {
            this.InitializeComponent();
        }
    }
}
```

**Key Differences:**
| Xamarin.Forms | Uno Platform | Notes |
|---------------|--------------|-------|
| `ViewCell` wrapper | Direct content | WinUI pattern |
| `StackLayout` (horizontal) | `Grid` with columns | Layout preference |
| `HasUnevenRows` | *(Not needed)* | Auto-sizing default |
| `IsPullToRefreshEnabled` | *(Removed)* | Feature gap |
| `TextColor` | `Foreground` | Property name |
| `DynamicResource` | `StaticResource` | Resource lookup |
| `OnPlatform` | *(Not needed)* | Cross-platform handled |
| `ItemTapped` handler | `SelectionMode="None"` | Cleaner approach |
| Icon assignment | *(Not needed)* | Asset migration out of scope |

---

## 7. Command Definitions

### Xamarin.Forms
```csharp
using System.Windows.Input;
using Xamarin.Forms;

ICommand getWeather;
public ICommand GetWeatherCommand =>
    getWeather ??
    (getWeather = new Command(async () => await ExecuteGetWeatherCommand()));

private async Task ExecuteGetWeatherCommand()
{
    if (IsBusy)
        return;

    IsBusy = true;
    try
    {
        // ... weather logic
    }
    finally
    {
        IsBusy = false;
    }
}
```

### Uno Platform
```csharp
using System.Windows.Input;
using MvvmHelpers.Commands;

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
        // ... weather logic
    }
    finally
    {
        IsBusy = false;
    }
}
```

**Key Differences:**
- XF: `Xamarin.Forms.Command` | Uno: `MvvmHelpers.Commands.AsyncCommand`
- XF: Lambda wrapping async call | Uno: Direct method reference
- XF: Framework-specific | Uno: Framework-agnostic (MvvmHelpers package)

---

## 8. Package References

### Xamarin.Forms (MyWeather.csproj)
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" Version="11.0.2" />
    <PackageReference Include="Plugin.Permissions" Version="3.0.0.12" />
    <PackageReference Include="Refractored.MvvmHelpers" Version="1.3.0" />
    <PackageReference Include="Xamarin.Essentials" Version="0.9.1-preview" />
    <PackageReference Include="Xamarin.Forms" Version="3.1.0.697729" />
  </ItemGroup>
</Project>
```

### Uno Platform (MyWeather.Uno.csproj + Directory.Packages.props)

**MyWeather.Uno.csproj:**
```xml
<Project Sdk="Uno.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net10.0-android;net10.0-ios;net10.0-browserwasm;net10.0-desktop</TargetFrameworks>
    <OutputType>Exe</OutputType>
    <UnoSingleProject>true</UnoSingleProject>
    
    <UnoFeatures>
      SkiaRenderer;
    </UnoFeatures>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" />
    <PackageReference Include="Refractored.MvvmHelpers" />
  </ItemGroup>
</Project>
```

**Directory.Packages.props:**
```xml
<Project>
  <ItemGroup>
    <PackageVersion Include="Newtonsoft.Json" Version="13.0.3" />
    <PackageVersion Include="Refractored.MvvmHelpers" Version="1.6.2" />
  </ItemGroup>
</Project>
```

**Key Differences:**
- XF: `Microsoft.NET.Sdk` | Uno: `Uno.Sdk`
- XF: `netstandard2.0` | Uno: `net10.0-*` multi-targeting
- XF: Direct version in csproj | Uno: Central Package Management
- XF: Xamarin.Forms + Xamarin.Essentials | Uno: Built into SDK
- XF: Plugin.Permissions | Uno: WinRT APIs

---

## Summary Table

| Component | Xamarin.Forms | Uno Platform | Change Type |
|-----------|---------------|--------------|-------------|
| **Navigation** | NavigationPage + TabbedPage | Frame + Pivot | Structural |
| **Pages** | ContentPage | Page | Rename |
| **Text Input** | Entry | TextBox | Rename |
| **Text Display** | Label | TextBlock | Rename |
| **Toggle** | Switch | ToggleSwitch | Rename |
| **Loading** | ActivityIndicator | ProgressRing | Rename |
| **List** | ListView | ListView | Same! |
| **Tabs** | TabbedPage | Pivot | Control change |
| **Preferences** | Xamarin.Essentials.Preferences | Windows.Storage.ApplicationData | API change |
| **GPS** | Xamarin.Essentials.Geolocation | Windows.Devices.Geolocation | API change |
| **Permissions** | Plugin.Permissions | Geolocator.RequestAccessAsync | API change |
| **Alerts** | DisplayAlert | ContentDialog | API change |
| **TTS** | Xamarin.Essentials.TextToSpeech | *(Not implemented)* | Gap |
| **Commands** | Xamarin.Forms.Command | MvvmHelpers.Commands.AsyncCommand | Minor |
| **ViewModel** | MvvmHelpers.BaseViewModel | MvvmHelpers.BaseViewModel | Same! |
| **Models** | (No change) | (No change) | Same! |
| **Services** | (No change) | (No change) | Same! |

---

**Conclusion:** Most changes are simple renames and API replacements. Core architecture (MVVM, bindings, business logic) remains 100% intact.
