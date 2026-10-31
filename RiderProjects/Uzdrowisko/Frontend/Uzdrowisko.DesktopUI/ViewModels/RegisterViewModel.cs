using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedStructures.Dtos.Patient;
using Uzdrowisko.DesktopUI.Services.Abstractions;

namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class RegisterViewModel : ViewModelBase
{
    private readonly IPatientService _patientService;

    [ObservableProperty]
    private string _errorMessage = string.Empty;
    [ObservableProperty] 
    private string _successMessage = string.Empty;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _surname = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _generatedPassword = string.Empty; 
    [ObservableProperty] private string _pesel = string.Empty;
    [ObservableProperty] private string _dateOfBirth = string.Empty;
    [ObservableProperty] private string _phoneNumber = string.Empty;
    [ObservableProperty] private string _address = string.Empty;
    [ObservableProperty] private string _city = string.Empty;
    [ObservableProperty] private string _postalCode = string.Empty;
    [ObservableProperty] private bool _isDialogVisible;
    [ObservableProperty] private string _generatedPasswordDisplay = string.Empty;

    public RegisterViewModel(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [RelayCommand]
    public async Task RegisterAction()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        string generatedPassword = Guid.NewGuid().ToString("N").Substring(0, 8);
        DateOnly parsedDate = DateOnly.TryParse(DateOfBirth, out var dt) ? dt : DateOnly.FromDateTime(DateTime.Today);

        var dto = new CreatePatientDto
        {
            Email = Email,
            PasswordRaw = generatedPassword,
            Name = Name,
            Surname = Surname,
            Pesel = Pesel,
            DateOfBirth = parsedDate,
            PhoneNumber = PhoneNumber,
            Address = Address,
            City = City,
            PostalCode = PostalCode
        };

        var result = await _patientService.CreatePatientItemAsync(dto, default);

        if (result != null)
        {
            GeneratedPasswordDisplay = generatedPassword;
            IsDialogVisible = true;
            SuccessMessage = "Pacjent został pomyślnie zarejestrowany!";
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
        Pesel = string.Empty;
        DateOfBirth = string.Empty;
        PhoneNumber = string.Empty;
        Address = string.Empty;
        City = string.Empty;
        PostalCode = string.Empty;
        
    }

    
}