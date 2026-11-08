using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Uzdrowisko.DesktopUI.Models;
using Uzdrowisko.DesktopUI.Services.Abstractions;

namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class TreatmentsViewModel : ViewModelBase
{
    private readonly ITreatmentService? _treatmentService;
    [ObservableProperty] private TreatmentItem _selectedTreatment;

    public ObservableCollection<TreatmentItem> Treatments { get; } = new();
    [ObservableProperty] private bool _isLoading;
    

    public TreatmentsViewModel(ITreatmentService treatmentService)
    {
        _treatmentService = treatmentService;
        _ = LoadTreatmentsAsync();
    }

    public async Task LoadTreatmentsAsync()
    {
        if (_treatmentService is null) return;

        IsLoading = true;
        try
        {
            var items = await _treatmentService.GetAllTreatmentsItemsAsync();
            Treatments.Clear();
            foreach (var item in items)
            {
                Treatments.Add(item);
            }

            if (Treatments.Count > 0)
            {
                SelectedTreatment = Treatments[0];
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"BŁĄD POBIERANIA: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public int DurationMinutes { get; set; }
    public string DurationDisplay
    {
        get
        {
            if (DurationMinutes <= 0) return "0 min";
        
            int hours = DurationMinutes / 60;
            int minutes = DurationMinutes % 60;

            if (hours > 0 && minutes > 0)
                return $"{hours}h {minutes} min";
            if (hours > 0)
                return $"{hours}h";
            
            return $"{minutes} min";
        }
    }
}
