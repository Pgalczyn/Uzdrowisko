using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uzdrowisko.DesktopUI.Models;
using Uzdrowisko.DesktopUI.Services;
using Uzdrowisko.DesktopUI.Services.Abstractions;

namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class CalendarViewModel : ViewModelBase
{
    private readonly IAppointmentService _service;
    private CancellationTokenSource? _cts;
    
    private readonly Action<ViewModelBase> _navigateTo;

    [ObservableProperty] private DateTime? _selectedDate = DateTime.Today;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isEmpty;
    
    [ObservableProperty] private bool _isDialogVisible;
    [ObservableProperty] private bool _isPostponeDialogVisible;
    
    [ObservableProperty] private AppointmentItem? _selectedAppointment;
    [ObservableProperty] private AppointmentItem? _selectedTime; 

    public ObservableCollection<AppointmentItem> DayTreatments { get; } = new();
    public ObservableCollection<AppointmentItem> AvailableTimes { get; } = new();

    public CalendarViewModel(IAppointmentService service, Action<ViewModelBase> navigateTo)
    {
        _service = service;
        _navigateTo = navigateTo;
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
        catch (OperationCanceledException) { }
        finally
        {
            if (!ct.IsCancellationRequested) IsLoading = false;
        }
    }
    [RelayCommand]
    public void OpenAppointmentDialog(AppointmentItem appointmentItem)
    {
        SelectedAppointment = appointmentItem;
        IsDialogVisible = true;
    }
    [RelayCommand]
    public void CloseDialog()
    {
        IsDialogVisible = false;
    }
    
    [RelayCommand]
    public void OpenPostponeDialog(AppointmentItem appointmentItem)
    {
        _navigateTo(new ChangeAppointmentDateViewModel(_service, _navigateTo, appointmentItem)); 
    }

    [RelayCommand]
    public void ClosePostponeDialog()
    {
        IsPostponeDialogVisible = false;
        SelectedTime = null;
    }
}
