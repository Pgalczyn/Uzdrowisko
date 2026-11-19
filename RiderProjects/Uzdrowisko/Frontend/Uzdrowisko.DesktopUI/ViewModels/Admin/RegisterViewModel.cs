using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedStructures.Dtos.Patient;
using SharedStructures.Dtos.Stay;
using Uzdrowisko.DesktopUI.Services.Abstractions;

namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class RegisterViewModel : ViewModelBase
{
    private readonly IPatientService _patientService;
    private readonly IStayService _stayService;
    private readonly Action _goBack;

    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _successMessage = string.Empty;

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

    [ObservableProperty] private DateTime? _stayDateFrom = DateTime.Today;
    [ObservableProperty] private DateTime? _stayDateTo = DateTime.Today.AddDays(14);
    [ObservableProperty] private string? _stayStatusMessage;

    private CancellationTokenSource? _cts;

    public RegisterViewModel(IPatientService patientService, IStayService stayService, Action goBack)
    {
        _patientService = patientService;
        _stayService = stayService;
        _goBack = goBack;
    }

    [RelayCommand]
    public async Task RegisterAction()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
        StayStatusMessage = null;

        if (StayDateFrom == null || StayDateTo == null)
        {
            ErrorMessage = "Wybierz daty pobytu pacjenta.";
            return;
        }

        if (StayDateTo < StayDateFrom)
        {
            ErrorMessage = "Data zakończenia pobytu nie może być wcześniejsza niż rozpoczęcia.";
            return;
        }

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

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

        var result = await _patientService.CreatePatientItemAsync(dto, ct);

        if (result == null)
        {
            ErrorMessage = "Rejestracja nie powiodła się. Sprawdź wpisane dane.";
            return;
        }

        var stayDto = new CreateStayDto()
        {
            PatientId = result.Id,
            DateFrom = DateOnly.FromDateTime(StayDateFrom.Value),
            DateTo = DateOnly.FromDateTime(StayDateTo.Value),
            Status = "Aktywny",
        };

        bool staySuccess = await _stayService.CreateStay(stayDto, ct);

        GeneratedPasswordDisplay = generatedPassword;
        IsDialogVisible = true;
        SuccessMessage = staySuccess
            ? "Pacjent oraz jego pobyt zostały pomyślnie zarejestrowane!"
            : $"Pacjent zarejestrowany, ale nie udało się dodać pobytu";
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
        StayDateFrom = DateTime.Today;
        StayDateTo = DateTime.Today.AddDays(14);
    }

    [RelayCommand]
    private void GoBack() => _goBack();
}