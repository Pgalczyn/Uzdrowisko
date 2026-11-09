using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Uzdrowisko.DesktopUI.Models;
using Uzdrowisko.DesktopUI.Services;
using Uzdrowisko.DesktopUI.Services.Abstractions;

namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    public DateTime Today => DateTime.Today;
    private readonly IAppointmentService _service;
    private readonly IWeatherService _weatherService;
    [ObservableProperty] private bool _isEmpty;
    public ObservableCollection<AppointmentItem> DayTreatments { get; } = new();

    [ObservableProperty] private string _temperature = "--C";
    [ObservableProperty] private string _weatherDescription = "Pobieranie.";

    public DashboardViewModel(IAppointmentService service, IWeatherService weatherService)
    {
        _service = service;
        _weatherService = weatherService;
        _ = LoadTreatmentsForToday(Today);
        _ = LoadWeatherAsync();
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

    private async Task LoadWeatherAsync()
    {
        try
        {
            var weather = await _weatherService.GetWeather();

            if (weather != null)
            {
                Temperature = $"{weather.Current.TempC}";
                WeatherDescription = $"{weather.Current.Condition.Text}";
            }
            else
            {
                WeatherDescription = "Brak danych o pogodzie";
            }
        }
        catch (Exception)
        {
            WeatherDescription = "Błąd połączenia";
        }
    }
}
