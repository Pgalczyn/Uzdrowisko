
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class LoginViewModel: ViewModelBase
{
    private readonly System.Action _Register;
    private readonly System.Action _Home;

    [ObservableProperty]
    private string _login;

    [ObservableProperty]
    private string _password;

    [ObservableProperty]
    private string _errorMessage;
    private readonly Services.AuthService _authService = new();


    public LoginViewModel(System.Action Register, System.Action Home)
    {
        _Register = Register;
        _Home = Home;
    }

    [RelayCommand]
    public void GoToRegister()
    {
        _Register?.Invoke();
    }

    [RelayCommand]
public async Task LoginAction()
{
    ErrorMessage = "";

    if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
    {
        ErrorMessage = "Wprowadź e-mail i hasło.";
        return;
    }

    var (success, error) = await _authService.LoginAsync(Login, Password);

    if (success)
    {
        _Home?.Invoke();
    }
    else
    {
        ErrorMessage = error ?? "Logowanie nie powiodło się.";
    }
}

}
