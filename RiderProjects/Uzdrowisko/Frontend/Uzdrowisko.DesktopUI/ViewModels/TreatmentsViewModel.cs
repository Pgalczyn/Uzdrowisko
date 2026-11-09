using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uzdrowisko.DesktopUI.Models;
using Uzdrowisko.DesktopUI.Services.Abstractions;

namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class TreatmentsViewModel : ViewModelBase
{
    private readonly ITreatmentService _treatmentService;
    [ObservableProperty] private TreatmentItem _selectedTreatment;
    private readonly IAppointmentService _appointmentService;
    public ObservableCollection<TreatmentItem> Treatments { get; } = new();
    [ObservableProperty] private bool _isLoading;
    private readonly Action<ViewModelBase> _navigateTo;
    

    public TreatmentsViewModel(ITreatmentService treatmentService ,IAppointmentService appointmentService, Action<ViewModelBase> navigateTo)
    {
        _treatmentService = treatmentService;
        _appointmentService = appointmentService;
        _navigateTo = navigateTo; 
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
    [RelayCommand]
    public void OpenForm(TreatmentItem? treatment)
    {
        if (treatment is null) return;
        _navigateTo(new ChooseAppointmentViewModel(_treatmentService, _appointmentService, _navigateTo, treatment));
    }
}
