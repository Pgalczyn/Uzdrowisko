using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uzdrowisko.DesktopUI.Services.Abstractions;

namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentView;

    private readonly IAuthService _authService;
    private readonly IAppointmentService _appointmentService;
    private readonly ITreatmentService _treatmentService;
    private readonly IPatientService _patientService;

    public MainViewModel(
        IAuthService authService, 
        IAppointmentService appointmentService, 
        ITreatmentService treatmentService,
        IPatientService patientService)
    {
        _authService = authService;
        _appointmentService = appointmentService;
        _treatmentService = treatmentService;
        _patientService = patientService;

        CurrentView = new LoginViewModel(_authService, NavigateToHome);
    }

    [RelayCommand]
    public void NavigateToHome()
    {
        CurrentView = new HomeViewModel(NavigateToLogin, _appointmentService, _treatmentService, _patientService);
    }

    [RelayCommand]
    public void NavigateToLogin()
    {
        CurrentView = new LoginViewModel(_authService, NavigateToHome);
    }
}