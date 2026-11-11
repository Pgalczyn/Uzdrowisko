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
    private readonly IEmployeeService _employeeService;
    private readonly IWeatherService _weatherService;

    public MainViewModel(
        IAuthService authService, 
        IAppointmentService appointmentService, 
        ITreatmentService treatmentService,
        IPatientService patientService,
        IEmployeeService employeeService,
        IWeatherService weatherService)
    {
        _authService = authService;
        _appointmentService = appointmentService;
        _treatmentService = treatmentService;
        _patientService = patientService;
        _employeeService = employeeService;
        CurrentView = new LoginViewModel(_authService, NavigateToHome);
        _weatherService =  weatherService;
    }

    [RelayCommand]
    public void NavigateToHome()
    {
        CurrentView = new HomeViewModel(NavigateToLogin,_weatherService, _appointmentService, _treatmentService, _patientService,_employeeService);
    }

    [RelayCommand]
    public void NavigateToLogin()
    {
        CurrentView = new LoginViewModel(_authService, NavigateToHome);
    }
}