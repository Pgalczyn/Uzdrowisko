using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uzdrowisko.DesktopUI.Services.Abstractions;

namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly Action _register;
    private readonly Action _home;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _login = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public LoginViewModel(IAuthService authService, Action home)
    {
        _authService = authService;
        _home = home;
    }

    [RelayCommand]
    public void GoToRegister()
    {
        _register?.Invoke();
    }

    [RelayCommand]
    public async Task LoginAction()
    {
        _home?.Invoke();
        // ErrorMessage = string.Empty;
        //
        // if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
        // {
        //     ErrorMessage = "Wprowadź e-mail i hasło.";
        //     return;
        // }
        //
        // var (success, error) = await _authService.LoginAsync(Login, Password);
        //
        // if (success)
        // {
        //     _home?.Invoke();
        // }
        // else
        // {
        //     ErrorMessage = error ?? "Logowanie nie powiodło się.";
        // }
    }
}
