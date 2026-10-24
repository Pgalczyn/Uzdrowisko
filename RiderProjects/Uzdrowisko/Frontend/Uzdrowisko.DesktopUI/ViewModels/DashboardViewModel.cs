using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Uzdrowisko.DesktopUI.Models;
using Uzdrowisko.Services;
namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    public DateTime Today => DateTime.Today;
    private readonly ITreatmentService _service;
    [ObservableProperty] private bool _isEmpty;
    public ObservableCollection<TreatmentItem> DayTreatments { get; } = new();
    public DashboardViewModel() : this(new FakeTreatmentService()) { }

    public DashboardViewModel(ITreatmentService service)
    {
        _service = service;
        _ = LoadTreatmentsForToday( Today);
    }

    private async Task LoadTreatmentsForToday(DateTime today)
    {
        var items = await _service.GetForDayAsync(today);
        DayTreatments.Clear();
        foreach (var t in items.OrderBy(t => t.Start))
        {
            DayTreatments.Add(t);
        }
        IsEmpty = DayTreatments.Count == 0;
    }

}
