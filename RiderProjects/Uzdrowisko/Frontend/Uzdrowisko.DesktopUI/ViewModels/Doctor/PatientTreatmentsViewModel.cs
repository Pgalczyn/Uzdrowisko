using System;
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

public partial class PatientTreatmentsViewModel : ViewModelBase
{
    private readonly IAppointmentService _service;
    private readonly ITreatmentService _treatmentService;
    private readonly IGenSchedule _genScheduleService;
    private readonly Action<ViewModelBase> _navigateTo;
    private readonly PatientItem? _selectedPatient;
    private CancellationTokenSource? _cts;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isEmpty;
    [ObservableProperty] private string? _statusMessage;
    [ObservableProperty] private bool _isDialogVisible;
    [ObservableProperty] private bool _isPostponeDialogVisible;
    [ObservableProperty] private AppointmentItem? _selectedAppointment;
    [ObservableProperty] private AppointmentItem? _selectedTime;
    private readonly Action _goBack;

    public ObservableCollection<AppointmentItem> AllTreatments { get; } = new();
    public ObservableCollection<AppointmentItem> AvailableTimes { get; } = new();

    public PatientTreatmentsViewModel(
        IAppointmentService service,
        ITreatmentService treatmentService,
        IGenSchedule genScheduleService,
        Action<ViewModelBase> navigateTo,
        PatientItem? selectedPatient = null,
        Action? goBack = null)
    {
        _service = service;
        _treatmentService = treatmentService;
        _genScheduleService = genScheduleService;
        _navigateTo = navigateTo;
        _selectedPatient = selectedPatient;
        _goBack = goBack;
        _ = LoadAllAsync();
    }
    [RelayCommand]
    public async Task RemoveAppointmentAsync()
    {
        if (SelectedAppointment == null) return;
        
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;
        
        try
        {
            IsLoading = true;
            await _service.DeleteAppointmentAsync(SelectedAppointment.Id, ct);
            AllTreatments.Remove(SelectedAppointment);
            IsEmpty = AllTreatments.Count == 0;
            SelectedAppointment = null;
            CloseDialog();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Błąd usuwania wizyty: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    [RelayCommand]
    public async Task LoadAllAsync()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        try
        {
            IsLoading = true;
            StatusMessage = null;
            var items = await _service.GetAllAppointments();
            AllTreatments.Clear();

            if (_selectedPatient != null)
            {
                items = items
                    .Where(a => a.PatientId == _selectedPatient.Id)
                    .Where(a => a.Status != "Usuniety")
                    .ToList();
            }
            else if (!UserSession.IsEmployee)
            {
                items = items
                    .Where(a => a.PatientId == UserSession.UserId)
                    .ToList();
            }

            foreach (var t in items.OrderBy(t => t.Date).ThenBy(t => t.TimeFrom))
            {
                AllTreatments.Add(t);
            }

            IsEmpty = AllTreatments.Count == 0;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            StatusMessage = $"Błąd: {ex.Message}";
        }
        finally
        {
            if (!ct.IsCancellationRequested) IsLoading = false;
        }
    }

    [RelayCommand]
    public void GoToGenerateSchedule()
    {
        var patient = _selectedPatient ?? new PatientItem { Id = UserSession.UserId };
        _navigateTo(new GenerateScheduleViewModel(
            _treatmentService,
            _genScheduleService,
            patient,
            () =>
            {
                _navigateTo(this);
                _ = LoadAllAsync();
            }));
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

    [RelayCommand]
    private void GoBack() => _goBack();
}