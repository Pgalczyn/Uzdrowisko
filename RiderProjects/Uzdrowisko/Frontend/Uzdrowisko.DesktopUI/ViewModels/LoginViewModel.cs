
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
        
        bool success = await FakeDataService.Login(Login, Password);
        
        if (success) {
            _Home?.Invoke();
        }
        else {
            ErrorMessage = "Nieprawidłowy login lub hasło";
        }
    }
}
