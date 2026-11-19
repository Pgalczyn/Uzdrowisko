using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedStructures.Dtos.Patient;
using SharedStructures.Dtos.Stay;
using Uzdrowisko.DesktopUI.Models;
using Uzdrowisko.DesktopUI.Services.Abstractions;

namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class UpdateUserViewModel : ViewModelBase
{
    private readonly IPatientService _patientService;
    private readonly IStayService _stayService;
    private readonly Action _goBack;
    private readonly int _patientId;

    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _successMessage = string.Empty;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _surname = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _pesel = string.Empty;
    [ObservableProperty] private string _dateOfBirth = string.Empty;
    [ObservableProperty] private string _phoneNumber = string.Empty;
    [ObservableProperty] private string _address = string.Empty;
    [ObservableProperty] private string _city = string.Empty;
    [ObservableProperty] private string _postalCode = string.Empty;
    [ObservableProperty] private bool _isDialogVisible;
    [ObservableProperty] private DateTime? _stayDateFrom = DateTime.Today;
    [ObservableProperty] private DateTime? _stayDateTo = DateTime.Today.AddDays(14);
    [ObservableProperty] private string? _stayStatusMessage;

    private CancellationTokenSource? _cts;

    public UpdateUserViewModel(
        PatientItem patientToEdit,
        IPatientService patientService,
        IStayService stayService,
        Action goBack)
    {
        _patientService = patientService;
        _stayService = stayService;
        _goBack = goBack;
        _patientId = patientToEdit.Id;

        Name = patientToEdit.Name;
        Surname = patientToEdit.Surname;
        Email = patientToEdit.Email;
        Pesel = patientToEdit.Pesel;
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

        var dto = new UpdatePatientDto
        {
            Name = Name,
            Surname = Surname,
            Email = Email,
            DateOfBirth = parsedDate,
            PhoneNumber = PhoneNumber,
            Address = Address,
            City = City,
            PostalCode = PostalCode
        };

        try
        {
            await _patientService.UpdatePatientItemAsync(_patientId, dto, ct);
            SuccessMessage = "Dane pacjenta zostały pomyślnie zaktualizowane!";
            IsDialogVisible = true;
        }
        catch (Exception)
        {
            ErrorMessage = "Edycja nie powiodła się. Sprawdź wpisane dane.";
        }
    }

    [RelayCommand]
    public void CloseDialog()
    {
        IsDialogVisible = false;
        GoBack();
    }

    [RelayCommand]
    private void GoBack() => _goBack();

    [RelayCommand]
    private async Task AddStayAsync()
    {
        if (StayDateFrom == null || StayDateTo == null)
        {
            StayStatusMessage = "Wybierz obie daty pobytu.";
            return;
        }

        if (StayDateTo < StayDateFrom)
        {
            StayStatusMessage = "Data zakończenia nie może być wcześniejsza niż rozpoczęcia.";
            return;
        }

        var dto = new UpdateStayDto
        {
            DateFrom = DateOnly.FromDateTime(StayDateFrom.Value),
            DateTo = DateOnly.FromDateTime(StayDateTo.Value),
            Status = "Aktywny",
            Active = true
        };

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        bool isSuccess = await _stayService.UpdateStay(_patientId, dto, ct);
        StayStatusMessage = isSuccess
            ? "Pobyt i harmonogram zostały zapisane."
            : $"Błąd";
    }
}