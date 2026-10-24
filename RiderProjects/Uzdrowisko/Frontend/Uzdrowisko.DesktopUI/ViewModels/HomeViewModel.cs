using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using Uzdrowisko.Services;

namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly Action _logout;
    private ITreatmentService _treatmentService;
    
    [ObservableProperty]
    private ViewModelBase _currentSubPage;
    public HomeViewModel(Action logout)
        : this(logout, new FakeTreatmentService())
    {
    }

    public HomeViewModel(Action logout, ITreatmentService treatmentService)
    {
        _logout = logout;
        _treatmentService = treatmentService;

        CurrentSubPage = new DashboardViewModel();
    }
    // public HomeViewModel() { }

    [RelayCommand]
    public void NavigateToDashboard()
    {
        CurrentSubPage = new DashboardViewModel();
    }

    [RelayCommand]
    public void NavigateToCalendar()
    {
        CurrentSubPage = new CalendarViewModel(_treatmentService);
    }

    [RelayCommand]
    public void NavigateToTreatments()
    {
        CurrentSubPage = new TreatmentsViewModel();
    }

    [RelayCommand]
    public void Logout()
    {
        _logout?.Invoke();
    }
}
