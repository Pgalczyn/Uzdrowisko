using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uzdrowisko.DesktopUI.Services.Abstractions;

namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly Action _logout;
    private readonly IAppointmentService _appointmentService;
    private readonly ITreatmentService _treatmentService;
    private readonly IPatientService _patientService;

    [ObservableProperty]
    private ViewModelBase _currentSubPage;

    public HomeViewModel(Action logout, IAppointmentService appointmentService, ITreatmentService treatmentService, IPatientService patientService)
    {
        _logout = logout;
        _appointmentService = appointmentService;
        _treatmentService = treatmentService;
        _patientService = patientService;

        CurrentSubPage = new DashboardViewModel(_appointmentService);
    }

    [RelayCommand]
    public void NavigateToDashboard()
    {
        CurrentSubPage = new DashboardViewModel(_appointmentService);
    }

    [RelayCommand]
    public void NavigateToCalendar()
    {
        CurrentSubPage = new CalendarViewModel(_appointmentService);
    }

    [RelayCommand]
    public void NavigateToTreatments()
    {
        CurrentSubPage = new TreatmentsViewModel(_treatmentService);
    }

    [RelayCommand]
    public void NavigateToRegister()
    {
        CurrentSubPage = new RegisterViewModel(_patientService);
    }

    [RelayCommand]
    public void Logout()
    {
        _logout?.Invoke();
    }
}
