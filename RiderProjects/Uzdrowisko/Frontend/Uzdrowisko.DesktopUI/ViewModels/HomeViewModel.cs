using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly Action _logout;
    
    [ObservableProperty]
    private ViewModelBase _currentSubPage;

    public HomeViewModel(Action logout)
    {
        _logout = logout;
        
        CurrentSubPage = new DashboardViewModel();
    }

    public HomeViewModel() { }

    [RelayCommand]
    public void NavigateToDashboard()
    {
        CurrentSubPage = new DashboardViewModel();
    }

    [RelayCommand]
    public void NavigateToCalendar()
    {
        CurrentSubPage = new CalendarViewModel();
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
