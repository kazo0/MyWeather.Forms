# Uno Platform & Microsoft Documentation References

## Documentation Query Log

This document logs all documentation queries made during the migration to ensure decisions were based on official guidance.

---

## Query 1: Navigation Pattern for TabbedPage

**Question:** What is the WinUI equivalent of Xamarin.Forms TabbedPage?

**Source:** Microsoft Learn (WinUI/UWP Documentation)

**Query:** "Pivot NavigationView TabbedPage migration"

**Response:** [Pivot Control Documentation](https://learn.microsoft.com/en-us/windows/apps/develop/ui/controls/pivot)

**Key Findings:**
> "The Pivot control enables touch-swiping between a small set of content sections."

> "Pivot is an ItemsControl, so it can contain a collection of items of any type. Any item you add to the Pivot that is not explicitly a PivotItem is implicitly wrapped in a PivotItem. Because a Pivot is often used to navigate between pages of content..."

**Decision:**
- ? Use `Pivot` control as the closest 1:1 mapping to `TabbedPage`
- ? Preserves swipe gesture navigation
- ? Provides tab headers automatically
- ?? Note: Pivot deprecated for Windows 11 but still fully supported

**Alternative Considered:**
- `NavigationView` with top tabs - Rejected: Requires more structural changes
- `TabView` - Rejected: Designed for document/browser-style tabs, not app sections

**Implementation:**
```xaml
<Pivot Title="My Weather">
    <PivotItem Header="Weather">
        <views:WeatherView DataContext="{Binding}"/>
    </PivotItem>
    <PivotItem Header="Forecast">
        <views:ForecastView DataContext="{Binding}"/>
    </PivotItem>
</Pivot>
```

---

## Query 2: Preferences Storage API

**Question:** What is the WinRT equivalent of Xamarin.Essentials.Preferences?

**Source:** Microsoft Learn (UWP Documentation)

**Query:** "Windows.Storage.ApplicationData LocalSettings preferences key-value"

**Response:** [Save and load settings in a UWP app](https://learn.microsoft.com/en-us/windows/uwp/get-started/settings-learning-track)

**Key Findings:**
> "Use app settings to store configuration data such as user preferences and app state."

> "Windows.Storage.ApplicationData.Current.LocalSettings gets the application settings container from the local app data store. Settings stored here are kept on the device."

> "ApplicationDataContainer is a container that represents app settings as key/value pairs."

**Code Example from Docs:**
```csharp
ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

// Save a setting locally on the device
localSettings.Values["test setting"] = "a device specific setting";
```

**Decision:**
- ? Use `ApplicationData.Current.LocalSettings.Values[]` as direct replacement
- ? Supports same data types (bool, string, int, etc.)
- ? Persists across app sessions
- ?? Requires manual `ContainsKey()` check for defaults (unlike Xamarin.Essentials)

**Implementation:**
```csharp
private static ApplicationDataContainer LocalSettings => 
    ApplicationData.Current.LocalSettings;

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
```

---

## Query 3: Geolocation API

**Question:** What is the WinRT equivalent of Xamarin.Essentials.Geolocation?

**Source:** Microsoft Learn (UWP Documentation)

**Query:** "Windows.Devices.Geolocation GPS location WinRT"

**Response:** [Windows.Devices.Geolocation Namespace](https://learn.microsoft.com/en-us/uwp/api/windows.devices.geolocation)

**Key Findings:**
> "Provides APIs for getting the geographic location of the user."

**Key Classes:**
- `Geolocator`: Main class for accessing location
- `Geoposition`: Represents a location with latitude/longitude
- `GeolocationAccessStatus`: Permission states

**API Methods:**
- `Geolocator.GetGeopositionAsync()`: Get current position
- `Geolocator.RequestAccessAsync()`: Request location permission

**Code Example (derived from docs):**
```csharp
var geolocator = new Geolocator();
var accessStatus = await Geolocator.RequestAccessAsync();

if (accessStatus == GeolocationAccessStatus.Allowed)
{
    var position = await geolocator.GetGeopositionAsync();
    double latitude = position.Coordinate.Point.Position.Latitude;
    double longitude = position.Coordinate.Point.Position.Longitude;
}
```

**Decision:**
- ? Use `Geolocator.GetGeopositionAsync()` directly
- ? Use `Geolocator.RequestAccessAsync()` for permissions
- ?? Property path deeper: `position.Coordinate.Point.Position.Latitude`

**Implementation:**
```csharp
var geolocator = new Geolocator();
var position = await geolocator.GetGeopositionAsync();

weatherRoot = await WeatherService.GetWeather(
    position.Coordinate.Point.Position.Latitude, 
    position.Coordinate.Point.Position.Longitude, 
    units);
```

---

## Query 4: Permission Handling

**Question:** How does WinRT handle runtime permissions compared to Plugin.Permissions?

**Source:** Microsoft Learn (UWP Documentation)

**Query:** "GeolocationAccessStatus permission request"

**Response:** [Geolocator.RequestAccessAsync Method](https://learn.microsoft.com/en-us/uwp/api/windows.devices.geolocation.geolocator.requestaccessasync)

**Key Findings:**
> "Requests permission to access location data."

**Enum Values:**
- `GeolocationAccessStatus.Allowed`: Permission granted
- `GeolocationAccessStatus.Denied`: Permission denied
- `GeolocationAccessStatus.Unspecified`: Not yet determined

**Decision:**
- ? Replace Plugin.Permissions with built-in `RequestAccessAsync()`
- ? Simpler API: single method call instead of check + request
- ? Cross-platform: WinRT handles iOS/Android/Windows differences

**Implementation:**
```csharp
async Task<bool> CheckPermissions()
{
    var accessStatus = await Geolocator.RequestAccessAsync();
    
    if (accessStatus != GeolocationAccessStatus.Allowed)
    {
        await ShowAlertAsync("Location Permission", 
            "To get your current city the location permission is required...",
            "OK");
        return false;
    }
    return true;
}
```

---

## Query 5: Alert Dialogs

**Question:** What is the WinUI equivalent of DisplayAlert?

**Source:** Microsoft Learn (WinUI Documentation)

**Query:** "ContentDialog WinUI alert message"

**Response:** [ContentDialog Class](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.contentdialog)

**Key Findings:**
> "Represents a dialog box that can be customized to contain any XAML content."

**Required Property (WinUI 3):**
- `XamlRoot`: Must be set for dialogs to display correctly in WinUI 3

**Code Example from Docs:**
```csharp
ContentDialog dialog = new ContentDialog();
dialog.Title = "Title";
dialog.Content = "Content";
dialog.CloseButtonText = "Close";
dialog.XamlRoot = this.XamlRoot;  // WinUI 3 requirement
await dialog.ShowAsync();
```

**Decision:**
- ? Use `ContentDialog` as replacement for `DisplayAlert`
- ? Set `XamlRoot` property (critical for WinUI 3)
- ? Use `CloseButtonText` for single-button alerts
- ?? Requires access to `XamlRoot` from current visual tree

**Implementation:**
```csharp
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

---

## Query 6: Text-to-Speech

**Question:** What is the WinRT equivalent of Xamarin.Essentials.TextToSpeech?

**Source:** Microsoft Learn (UWP Documentation)

**Query:** "Windows.Media.SpeechSynthesis TTS"

**Response:** [SpeechSynthesizer Class](https://learn.microsoft.com/en-us/uwp/api/windows.media.speechsynthesis.speechsynthesizer)

**Key Findings:**
> "Provides access to the functionality of a speech synthesis engine."

**Code Example from Docs:**
```csharp
var synthesizer = new SpeechSynthesizer();
var stream = await synthesizer.SynthesizeTextToStreamAsync("Hello world");
// Play stream with MediaElement
```

**Uno Platform Status:**
```
?? Warning Uno0001: Windows.Media.SpeechSynthesis.SpeechSynthesizer is not implemented in Uno
```

**Decision:**
- ?? API exists in WinRT but **not yet implemented in Uno Platform**
- ? Added try-catch to prevent crashes
- ? Added debug logging as workaround
- ? Future: Wait for Uno Platform implementation or use platform-specific code

**Implementation (with fallback):**
```csharp
private async Task SpeakTextAsync(string text)
{
    try
    {
        var synthesizer = new SpeechSynthesizer();
        var stream = await synthesizer.SynthesizeTextToStreamAsync(text);
        // Note: Would need MediaElement for playback
        System.Diagnostics.Debug.WriteLine($"TTS: {text}");
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"TTS failed: {ex.Message}");
    }
}
```

---

## Query 7: WinUI Control Mappings

**Question:** What are the WinUI equivalents of common Xamarin.Forms controls?

**Source:** Microsoft Learn (WinUI Documentation)

**Query:** "WinUI controls list equivalents"

**Response:** [Controls for Windows apps](https://learn.microsoft.com/en-us/windows/apps/develop/ui/controls/)

**Key Mappings:**

| Xamarin.Forms | WinUI | Notes |
|---------------|-------|-------|
| ContentPage | Page | Root container |
| Entry | TextBox | Text input |
| Label | TextBlock | Static text |
| Switch | ToggleSwitch | On/off toggle |
| Button | Button | Same name! |
| ActivityIndicator | ProgressRing | Loading indicator |
| ListView | ListView | Same name! |
| Image | Image | Same name! |
| StackLayout | StackPanel | Layout container |
| Grid | Grid | Same name! |

**Decision:**
- ? Direct 1:1 mappings for most controls
- ? Property names sometimes differ (`Text` vs `Content`, `IsToggled` vs `IsOn`)
- ? XAML namespace changes (`http://xamarin.com/...` ? `http://schemas.microsoft.com/...`)

---

## Query 8: Navigation Design Patterns

**Question:** What are the recommended navigation patterns in WinUI apps?

**Source:** Microsoft Learn (WinUI Documentation)

**Query:** "WinUI navigation patterns Frame NavigationView"

**Response:** [Navigation design basics](https://learn.microsoft.com/en-us/windows/apps/design/basics/navigation-basics)

**Key Findings:**
> "With few exceptions, any app that has multiple pages uses a frame. Typically, an app has a main page that contains the frame and a primary navigation element, such as a navigation view control. When the user selects a page, the frame loads and displays it."

**Navigation Controls:**
- `Frame`: Core navigation container (like NavigationPage)
- `NavigationView`: Top/left navigation menus
- `TabView`: Document-style tabs
- `Pivot`: Content section swiping (our choice)

**Decision:**
- ? Use `Frame` for page navigation (matches Xamarin.Forms NavigationPage)
- ? Use `Pivot` for tab navigation (matches TabbedPage)
- ? Frame.Navigate() replaces MainPage assignment

**Implementation:**
```csharp
// App.xaml.cs
var rootFrame = new Frame();
MainWindow.Content = rootFrame;
rootFrame.Navigate(typeof(MainTabs), args.Arguments);
```

---

## Uno Platform Specific Queries

### Query A: Uno Platform API Implementation Status

**Question:** How to check which WinRT APIs are implemented in Uno Platform?

**Source:** Uno Platform Documentation

**Attempted Queries:**
- "Uno Platform WinRT API compatibility"
- "SpeechSynthesizer Uno Platform"
- "Geolocation Uno Platform"

**Response:** *(No results from Uno Docs MCP - used compiler warnings instead)*

**Finding:**
Uno Platform provides compiler warnings for unimplemented APIs:
```
warning Uno0001: Windows.Media.SpeechSynthesis.SpeechSynthesizer is not implemented in Uno
(https://aka.platform.uno/notimplemented?m=Windows.Media.SpeechSynthesis.SpeechSynthesizer)
```

**Decision:**
- ? Use compiler warnings as source of truth
- ? Visit provided links for implementation status
- ? Add try-catch blocks for unimplemented APIs

### Query B: Uno Platform Project Templates

**Question:** How to create a new Uno Platform project?

**Command Used:**
```powershell
dotnet new list uno
```

**Response:**
```
Template Name                    Short Name     Language  Tags
Uno Platform App                 unoapp         [C#]      Multi-platform/Uno Platform/...
```

**Command Executed:**
```powershell
dotnet new unoapp -o MyWeather.Uno --preset blank --force
```

**Decision:**
- ? Use `unoapp` template
- ? Use `--preset blank` for minimal starting point
- ? Uno SDK handles multi-targeting automatically

---

## Documentation Coverage Summary

| Topic | Source | Status |
|-------|--------|--------|
| Navigation (TabbedPage ? Pivot) | Microsoft Learn (Pivot) | ? Complete |
| Preferences Storage | Microsoft Learn (ApplicationData) | ? Complete |
| Geolocation | Microsoft Learn (Geolocator) | ? Complete |
| Permissions | Microsoft Learn (GeolocationAccessStatus) | ? Complete |
| Alerts | Microsoft Learn (ContentDialog) | ? Complete |
| Text-to-Speech | Microsoft Learn (SpeechSynthesizer) | ?? Not implemented |
| Control Mappings | Microsoft Learn (Controls) | ? Complete |
| Frame Navigation | Microsoft Learn (Navigation Basics) | ? Complete |
| Uno SDK Usage | Uno Platform CLI | ? Complete |
| API Status | Uno Compiler Warnings | ? Complete |

---

## Key Documentation Principles Followed

1. ? **Microsoft Docs First:** All WinRT API decisions based on official Microsoft Learn documentation
2. ? **Code Examples:** Used official code samples from Microsoft Docs
3. ? **Platform Guidance:** Followed recommended patterns (Frame + Pivot navigation)
4. ? **API Coverage:** Verified each replacement has official documentation
5. ? **Error Handling:** Checked for known limitations (TTS not implemented)

---

## Documentation Links Reference

### Microsoft Learn (WinUI/UWP)
- Pivot Control: https://learn.microsoft.com/en-us/windows/apps/develop/ui/controls/pivot
- Navigation Basics: https://learn.microsoft.com/en-us/windows/apps/design/basics/navigation-basics
- ApplicationData: https://learn.microsoft.com/en-us/windows/uwp/get-started/settings-learning-track
- Geolocation API: https://learn.microsoft.com/en-us/uwp/api/windows.devices.geolocation
- ContentDialog: https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.contentdialog
- SpeechSynthesizer: https://learn.microsoft.com/en-us/uwp/api/windows.media.speechsynthesis.speechsynthesizer
- Controls Index: https://learn.microsoft.com/en-us/windows/apps/develop/ui/controls/

### Uno Platform
- Official Docs: https://platform.uno/docs/
- API Status: https://aka.platform.uno/notimplemented

---

**Conclusion:** All migration decisions were grounded in official Microsoft documentation for WinRT/WinUI APIs, with Uno Platform compiler warnings used to identify implementation gaps.
