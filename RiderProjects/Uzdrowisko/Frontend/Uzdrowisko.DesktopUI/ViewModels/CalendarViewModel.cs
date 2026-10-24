using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Uzdrowisko.DesktopUI.Models;
using Uzdrowisko.Services;

namespace Uzdrowisko.DesktopUI.ViewModels;
using System;
using CommunityToolkit.Mvvm.ComponentModel;

public partial class CalendarViewModel : ViewModelBase
{
    private readonly ITreatmentService _service;
    private CancellationTokenSource? _cts;

    [ObservableProperty] private DateTime? _selectedDate = DateTime.Today;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isEmpty;
    public ObservableCollection<TreatmentItem> DayTreatments { get; } = new();

    public CalendarViewModel(ITreatmentService service)
    {
        _service = service;
        _ = LoadAsync(SelectedDate);
    }

    partial void OnSelectedDateChanged(DateTime? value) => _ = LoadAsync(value);

    private async Task LoadAsync(DateTime? day)
    {
        if (day is null) return;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        try
        {
            IsLoading = true;
            var items = await _service.GetForDayAsync(day.Value, ct);

            DayTreatments.Clear();
            foreach (var t in items.OrderBy(t => t.Start))
                DayTreatments.Add(t);

            IsEmpty = DayTreatments.Count == 0;
        }
        catch (OperationCanceledException) { }
        finally
        {
            if (!ct.IsCancellationRequested) IsLoading = false;
        }
    }


}
