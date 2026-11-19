using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uzdrowisko.DesktopUI.Services;
using Uzdrowisko.DesktopUI.Services.Abstractions;

namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly Action _logout;
    private readonly IAppointmentService _appointmentService;
    private readonly ITreatmentService _treatmentService;
    private readonly IPatientService _patientService;
    private readonly IEmployeeService _employeeService;
    private readonly IWeatherService _weatherService;
    private readonly IMenuService  _menuService;
    private readonly IGenSchedule _genScheduleService;
    private readonly IStayService _stayService;
    [ObservableProperty]
    private ViewModelBase _currentSubPage;

    public HomeViewModel(Action logout,IWeatherService weatherService,IMenuService menuService, IAppointmentService appointmentService, ITreatmentService treatmentService, IPatientService patientService, IEmployeeService employeeService, IGenSchedule genScheduleService, IStayService stayService)
    {
        _logout = logout;
        _appointmentService = appointmentService;
        _treatmentService = treatmentService;
        _patientService = patientService;
        _employeeService = employeeService;
        _weatherService  = weatherService;
        _menuService = menuService;
        _genScheduleService = genScheduleService;
        _stayService = stayService;
        CurrentSubPage = new DashboardViewModel(_appointmentService,_weatherService,_menuService,targetViewModel => CurrentSubPage = targetViewModel);
    }

    public bool IsAdmin => UserSession.IsEmployee;
    [RelayCommand]
    public void NavigateToDashboard()
    {
        CurrentSubPage = new DashboardViewModel(_appointmentService,_weatherService,_menuService,targetViewModel => CurrentSubPage = targetViewModel);
    }

    [RelayCommand]
    public void NavigateToCalendar()
    {
        CurrentSubPage = new CalendarViewModel(_appointmentService,targetViewModel => CurrentSubPage = targetViewModel);
    }

    [RelayCommand]
    public void NavigateToTreatments()
    {
        CurrentSubPage = new TreatmentsViewModel(_treatmentService,_appointmentService,targetViewModel => CurrentSubPage = targetViewModel
        );
    }

    
    [RelayCommand]
    private void NavigateToAdmin()
    {
        CurrentSubPage = new AdminViewModel(
            _patientService, 
            _employeeService, 
            _treatmentService,
            _stayService,
            targetViewModel => CurrentSubPage = targetViewModel
        );
    }

    [RelayCommand]
    public void NavigateToDoctor()
    {
        CurrentSubPage = new DoctorViewModel(
            _patientService, 
            _employeeService, 
            _genScheduleService,
            _treatmentService,
            targetViewModel => CurrentSubPage = targetViewModel ,
            _appointmentService,
            _stayService
        );
    }
    [RelayCommand]
    public void Logout()
    {
        _logout?.Invoke();
    }
}
