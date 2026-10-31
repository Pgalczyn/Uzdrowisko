using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Uzdrowisko.DesktopUI.Models;
using Uzdrowisko.DesktopUI.Services.Abstractions;

namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    public DateTime Today => DateTime.Today;
    private readonly IAppointmentService _service;
    [ObservableProperty] private bool _isEmpty;
    public ObservableCollection<AppointmentItem> DayTreatments { get; } = new();

    public DashboardViewModel(IAppointmentService service)
    {
        _service = service;
        _ = LoadTreatmentsForToday(Today);
    }

    private async Task LoadTreatmentsForToday(DateTime today)
    {
        var items = await _service.GetForDayAsync(today);
        DayTreatments.Clear();
        foreach (var t in items.OrderBy(t => t.TimeFrom))
        {
            DayTreatments.Add(t);
        }
        IsEmpty = DayTreatments.Count == 0;
    }
}
