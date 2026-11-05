using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uzdrowisko.DesktopUI.Services.Abstractions;
using Uzdrowisko.DesktopUI.ViewModels;
namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class AdminViewModel : ViewModelBase
{
    private readonly IPatientService _patientService;
    private readonly IEmployeeService _employeeService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDialogOpen))]
    private ViewModelBase? _currentDialog;

    public bool IsDialogOpen => CurrentDialog != null;

    public AdminViewModel(IPatientService patientService, IEmployeeService employeeService)
    {
        _patientService = patientService;
        _employeeService = employeeService;
    }

    [RelayCommand] private void OpenRegister()  => CurrentDialog = new RegisterViewModel(_patientService);
    [RelayCommand] private void OpenEmployees() => CurrentDialog = new RegisterEmployeeViewModel(_employeeService);

    [RelayCommand] private void CloseDialog() => CurrentDialog = null;
}