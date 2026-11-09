using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedStructures.Dtos.Employee;
using Uzdrowisko.DesktopUI.Models;
using Uzdrowisko.DesktopUI.Services.Abstractions;

namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class UpdateEmployeeViewModel : ViewModelBase
{
    private readonly IEmployeeService _employeeService;
    private readonly Action _goBack;
    private readonly int _employeeId; 

    public IReadOnlyList<string> Roles { get; } = new[] { "Lekarz", "Fizjoterapeuta", "Sekretarka", "Admin" };

    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _successMessage = string.Empty;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _surname = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _dateOfBirth = string.Empty;
    [ObservableProperty] private string _phoneNumber = string.Empty;
    [ObservableProperty] private string? _selectedRole;
    [ObservableProperty] private bool _isDialogVisible;
    
    private CancellationTokenSource? _cts;
    public UpdateEmployeeViewModel(EmployeeItem employeeToEdit, IEmployeeService employeeService, Action goBack)
    {
        _employeeService = employeeService;
        _goBack = goBack;
        _employeeId = employeeToEdit.Id;
        Name = employeeToEdit.Name;
        Surname = employeeToEdit.Surname;
        Email = employeeToEdit.Email;
        PhoneNumber = employeeToEdit.PhoneNumber;
        SelectedRole = employeeToEdit.Role;
    }

    [RelayCommand]
    public async Task UpdateAction()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;
        DateOnly? parsedDate = null;
        if (DateOnly.TryParse(DateOfBirth, out var dt))
        {
            parsedDate = dt;
        }
        var dto = new UpdateEmployeeDto
        {
            Name = Name,
            Surname = Surname,
            Email = Email,
            PhoneNumber = PhoneNumber,
            DateOfBirth = parsedDate,
            Role = SelectedRole
        };

        try
        {
            await _employeeService.UpdateEmployeeItemAsync(_employeeId, dto,ct);
            
            SuccessMessage = "Dane pracownika zostały pomyślnie zaktualizowane!";
            IsDialogVisible = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Edycja nie powiodła się. Sprawdź wpisane dane.";
        }
    }

    

    [RelayCommand]
    private void GoBack() => _goBack();
}