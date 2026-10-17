using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentView;

    public MainViewModel()
    {
        CurrentView = new LoginViewModel(NavigateToRegister, NavigateToHome);
    }

    [RelayCommand]
    public void NavigateToHome()
    {
        CurrentView = new HomeViewModel(NavigateToLogin);
    }

    [RelayCommand]
    public void NavigateToLogin()
    {
        CurrentView = new LoginViewModel(NavigateToRegister, NavigateToHome);
    }

    [RelayCommand]
    public void NavigateToRegister()
    {
        CurrentView = new RegisterViewModel();
    }
}
