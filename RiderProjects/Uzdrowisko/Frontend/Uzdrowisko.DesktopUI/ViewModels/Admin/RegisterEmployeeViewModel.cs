using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedStructures.Dtos.Employee;
using Uzdrowisko.DesktopUI.Services.Abstractions;

namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class RegisterEmployeeViewModel : ViewModelBase
{
    private readonly IEmployeeService _employeeService;

    public IReadOnlyList<string> Roles { get; } = new[] { "Lekarz", "Fizjoterapeuta", "Sekretarka" };

    [ObservableProperty]
    private string _errorMessage = string.Empty;
    [ObservableProperty]
    private string _successMessage = string.Empty;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _surname = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _dateOfBirth = string.Empty;
    [ObservableProperty] private string _phoneNumber = string.Empty;
    [ObservableProperty] private string? _selectedRole;
    [ObservableProperty] private bool _isDialogVisible;
    [ObservableProperty] private string _generatedPasswordDisplay = string.Empty;
    private readonly Action _goBack;

    public RegisterEmployeeViewModel(IEmployeeService employeeService, Action goBack)
    {
        _employeeService = employeeService;
        _goBack = goBack;
    }

    [RelayCommand]
    public async Task RegisterAction()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        string generatedPassword = Guid.NewGuid().ToString("N").Substring(0, 8);
        DateOnly parsedDate = DateOnly.TryParse(DateOfBirth, out var dt) ? dt : DateOnly.FromDateTime(DateTime.Today);

        var dto = new RegisterEmployeeDto
        {
            FirstName = Name,
            LastName = Surname,
            Email = Email,
            PhoneNumber = PhoneNumber,
            DateOfBirth = parsedDate,
            Role = SelectedRole ?? string.Empty,
            Password = generatedPassword
        };

        var result = await _employeeService.CreateEmployeeItemAsync(dto, default);

        if (result != null)
        {
            GeneratedPasswordDisplay = generatedPassword;
            IsDialogVisible = true;
            SuccessMessage = "Pracownik został pomyślnie zarejestrowany!";
        }
        else
        {
            ErrorMessage = "Rejestracja nie powiodła się. Sprawdź wpisane dane.";
        }
    }

    [RelayCommand]
    public void CloseDialog()
    {
        IsDialogVisible = false;
        Name = string.Empty;
        Surname = string.Empty;
        Email = string.Empty;
        DateOfBirth = string.Empty;
        PhoneNumber = string.Empty;
        SelectedRole = null;
    }
    [RelayCommand]
    private void GoBack() => _goBack();
}