using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uzdrowisko.DesktopUI.Models;
using Uzdrowisko.DesktopUI.Services;
using Uzdrowisko.DesktopUI.Services.Abstractions;
using System.IO;
using System.Net.Http;
using Avalonia.Media.Imaging;
using Avalonia.Threading;


namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    public DateTime Today => DateTime.Today;
    private readonly IAppointmentService _service;
    private readonly IWeatherService _weatherService;
    [ObservableProperty] private bool _isEmpty;
    
    private readonly IMenuService _menuService;
    [ObservableProperty] private MenuItem? _todayMenu;
    
    private readonly Action<ViewModelBase> _navigateTo;
    public ObservableCollection<AppointmentItem> DayTreatments { get; } = new();
    private readonly DispatcherTimer _weatherTimer;

    [ObservableProperty] private string _temperature = "--C";
    [ObservableProperty] private string _weatherDescription = "Pobieranie.";

    public DashboardViewModel(IAppointmentService service, IWeatherService weatherService,IMenuService menuService,
        Action<ViewModelBase> navigateTo)
    {
        _service = service;
        _weatherService = weatherService;
        _navigateTo = navigateTo;
        _menuService = menuService;
        _ = LoadTreatmentsForToday(Today);
        _ = LoadWeatherAsync();
        _ =  LoadMenu();
        _weatherTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(15) };
        _weatherTimer.Tick += (sender, args) =>
        {
            _ = LoadWeatherAsync();
        };
        _weatherTimer.Start();
    }

    private async Task LoadTreatmentsForToday(DateTime today)
    {
        var items = await _service.GetForDayAsync(today);
        DayTreatments.Clear();
        if (!UserSession.IsEmployee)
        {
            items = items
                .Where(a => a.PatientId == UserSession.UserId)
                .ToList();
        }

        foreach (var t in items.OrderBy(t => t.TimeFrom))
        {
            DayTreatments.Add(t);
        }

        IsEmpty = DayTreatments.Count == 0;
    }
    [ObservableProperty] private Bitmap? _weatherIcon;
    [ObservableProperty] private string _weatherDetails = "";

    private async Task LoadWeatherAsync()
    {
        try
        {
            var weather = await _weatherService.GetWeather();

            if (weather?.Current?.Condition != null)
            {
                Temperature = $"{weather.Current.TempC}";
                WeatherDescription = weather.Current.Condition.Text;

                // ikonka 
                var iconUrl = weather.Current.Condition.IconUrl;
                if (!string.IsNullOrWhiteSpace(iconUrl))
                {
                    var fullUrl = iconUrl.StartsWith("http") ? iconUrl : $"https:{iconUrl}";
                
                    using var httpClient = new HttpClient();
                    var imageBytes = await httpClient.GetByteArrayAsync(fullUrl);
                    using var stream = new MemoryStream(imageBytes);
                
                    WeatherIcon = new Bitmap(stream);
                }

                var todayForecast = weather.Forecast?.Days?.FirstOrDefault()?.Day;
                if (todayForecast != null)
                {
                    WeatherDetails = $"Min: {todayForecast.MinTempC}°C | Max: {todayForecast.MaxTempC}°C | 🌧 Opady: {todayForecast.DailyChanceOfRain}%";
                }
            }
            else
            {
                WeatherDescription = "Brak sekcji 'current' w danych z serwera";
                Temperature = "--";
                WeatherDetails = "";
            }
        }
        catch (Exception ex)
        {
            WeatherDescription = $"Błąd: {ex.Message}";
            Console.WriteLine($"[DashboardViewModel] Błąd pobierania pogody: {ex}");
        }
    }

    private async Task LoadMenu()
    {
        try
        {
            var menu = await _menuService.GetMenu();
        
            if (menu != null)
            {
                using var httpClient = new HttpClient();
            
                var breakfastImageTask = DownloadImage(httpClient, menu.Breakfast.ImageUrl);
                var lunchImageTask = DownloadImage(httpClient, menu.Lunch.ImageUrl);
                var dinnerImageTask = DownloadImage(httpClient, menu.Dinner.ImageUrl);

                await Task.WhenAll(breakfastImageTask, lunchImageTask, dinnerImageTask);

                menu.Breakfast.Image = await breakfastImageTask;
                menu.Lunch.Image = await lunchImageTask;
                menu.Dinner.Image = await dinnerImageTask;
            }

            TodayMenu = menu;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DashboardViewModel] Błąd pobierania jadłospisu: {ex}");
        }
    }

    private async Task<Bitmap?> DownloadImage(HttpClient client, string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
    
        try
        {
            var bytes = await client.GetByteArrayAsync(url);
            using var stream = new MemoryStream(bytes);
            return new Bitmap(stream);
        }
        catch
        {
            return null; 
        }
    }




    [RelayCommand]
    private void NavigateToCalendar()
    {
        _navigateTo(new CalendarViewModel(
            _service,
            _navigateTo
        ));
    }
}