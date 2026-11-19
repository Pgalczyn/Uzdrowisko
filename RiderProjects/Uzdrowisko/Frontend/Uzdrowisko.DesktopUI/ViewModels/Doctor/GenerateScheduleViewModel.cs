using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uzdrowisko.DesktopUI.Models;
using Uzdrowisko.DesktopUI.Services.Abstractions;

namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class GenerateScheduleViewModel : ViewModelBase
{
    private readonly ITreatmentService _treatmentService;
    private readonly IGenSchedule _genScheduleService;
    private readonly PatientItem _selectedPatient;
    private readonly Action _goBack;

    private List<TreatmentItem> _allTreatments = new();

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string? _statusMessage;
    [ObservableProperty] private bool _isDialogVisible;
    
    [ObservableProperty] 
    private string _searchText = string.Empty;

    public ObservableCollection<TreatmentItem> AvailableTreatments { get; } = new();

    public ObservableCollection<TreatmentItem> SelectedTreatments { get; } = new();
    private TreatmentItem? _pickedTreatment;
    public TreatmentItem? PickedTreatment
    {
        get => _pickedTreatment;
        set
        {
            if (value != null)
            {
                if (!SelectedTreatments.Any(t => t.Id == value.Id))
                {
                    SelectedTreatments.Add(value);
                }
                _pickedTreatment = null;
                OnPropertyChanged(nameof(PickedTreatment));
            }
        }
    }

    public GenerateScheduleViewModel(
        ITreatmentService treatmentService,
        IGenSchedule genScheduleService,
        PatientItem selectedPatient,
        Action goBack)
    {
        _treatmentService = treatmentService;
        _genScheduleService = genScheduleService;
        _selectedPatient = selectedPatient;
        _goBack = goBack;
        _ = LoadTreatmentsAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterTreatments();
    }

    private void FilterTreatments()
    {
        AvailableTreatments.Clear();
        
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allTreatments
            : _allTreatments.Where(t => t.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        foreach (var t in filtered)
        {
            AvailableTreatments.Add(t);
        }
    }

    private async Task LoadTreatmentsAsync()
    {
        try
        {
            IsLoading = true;
            var treatments = await _treatmentService.GetAllTreatmentsItemsAsync();
            
            _allTreatments.Clear();
            if (treatments != null)
            {
                _allTreatments.AddRange(treatments);
            }
            
            FilterTreatments();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Błąd: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void RemoveSelectedTreatment(TreatmentItem item)
    {
        if (SelectedTreatments.Contains(item))
        {
            SelectedTreatments.Remove(item);
        }
    }

    [RelayCommand]
    private async Task GenerateScheduleAsync()
    {
        var selectedIds = SelectedTreatments
            .Select(t => t.Id)
            .ToList();

        if (!selectedIds.Any())
        {
            StatusMessage = "Wybierz przynajmniej jeden zabieg!";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Generowanie planu...";
            int patientId = _selectedPatient.Id;

            var (isSuccess, errorMessage) = await _genScheduleService.GenScheduleResultAsync(patientId, selectedIds);

            if (isSuccess)
            {
                // StatusMessage = "Harmonogram wygenerowany pomyślnie!";
                IsDialogVisible =  true;
                
            }
            else
            {
                StatusMessage = $"Błąd: {errorMessage}";
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void GoBack() => _goBack();

    [RelayCommand]
    private void CLoseDialog()
    {
        IsDialogVisible= false;
    }
}