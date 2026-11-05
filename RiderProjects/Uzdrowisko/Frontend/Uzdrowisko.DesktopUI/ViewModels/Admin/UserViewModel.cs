using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedStructures.Dtos.Patient;
using Uzdrowisko.DesktopUI.Models;
using Uzdrowisko.DesktopUI.Services;
using Uzdrowisko.DesktopUI.Services.Abstractions;

namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class UserViewModel : ViewModelBase
{


    private readonly IPatientService _patientService;
    private CancellationTokenSource? _cts;
    [ObservableProperty] private bool _isLoading;
    public ObservableCollection<PatientItem> PatientListItemDtos { get; } = new();
    [ObservableProperty] private bool _isDialogVisible;
    [ObservableProperty] 
    private PatientItem? _selectedPatient;

    public UserViewModel(IPatientService service)
    {
        _patientService = service;
         _ = LoadAllUsersAsync();
        
    }


    [RelayCommand]
    public async Task LoadAllUsersAsync()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        try
        {
            IsLoading = true;
            var items = await _patientService.GetAllPatientsAsync(ct);
            PatientListItemDtos.Clear();
            if (items != null)
            {
                foreach (var item in items)
                {
                    PatientListItemDtos.Add(item);
                }
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            if (!ct.IsCancellationRequested) IsLoading = false;
        }
    }
    [RelayCommand]
    public async Task RemovePatientAsync()
    {
        if (SelectedPatient == null) return;
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;
        try
        {
            IsLoading = true;
            
            await _patientService.DeletePatientItemAsync(SelectedPatient.Id,ct); 

            PatientListItemDtos.Remove(SelectedPatient);
            CloseRemoveDialog();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Błąd usuwania pacjenta: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    [RelayCommand]
    public void OpenRemoveDialog(PatientItem patient)
    {
        SelectedPatient = patient;
        IsDialogVisible = true;
    }

    [RelayCommand]
    public async Task CloseRemoveDialog()
    {
        IsDialogVisible =  false;
    }
}