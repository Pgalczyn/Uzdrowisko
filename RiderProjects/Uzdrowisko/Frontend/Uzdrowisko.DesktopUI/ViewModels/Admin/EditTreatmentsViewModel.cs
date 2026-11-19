using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedStructures.Dtos.Treatment;
using Uzdrowisko.DesktopUI.Models;
using Uzdrowisko.DesktopUI.Services.Abstractions;

namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class EditTreatmentsViewModel : ViewModelBase
{
    private readonly ITreatmentService _treatmentService;
    private readonly Action<ViewModelBase> _navigateTo;
    private readonly Action? _goBack;
    
    private CancellationTokenSource? _cts;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isEmpty;
    [ObservableProperty] private string? _statusMessage;

    [ObservableProperty] private bool _isDialogVisible;
    [ObservableProperty] private bool _isAddDialogVisible;
    [ObservableProperty] private TreatmentItem? _selectedTreatment;

    [ObservableProperty] private string _newName = string.Empty;
    [ObservableProperty] private string _newDescription = string.Empty;
    [ObservableProperty] private string _newDurationMinutes = "15";
    [ObservableProperty] private string _newType = string.Empty;
    [ObservableProperty] private string _newPrice = "50";
    [ObservableProperty] private int _newRequiredRoleId = 1;
    [ObservableProperty] private string _newFlag = "0_S_1";

    public ObservableCollection<TreatmentItem> AllTreatments { get; } = new();

    public EditTreatmentsViewModel(
        ITreatmentService treatmentService,
        Action<ViewModelBase> navigateTo,
        Action? goBack = null)
    {
        _treatmentService = treatmentService;
        _navigateTo = navigateTo;
        _goBack = goBack;
        _ = LoadAllAsync();
    }

    [RelayCommand]
    public async Task LoadAllAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = null;

            var items = await _treatmentService.GetAllTreatmentsItemsAsync();
            AllTreatments.Clear();

            if (items != null)
            {
                foreach (var t in items.OrderBy(t => t.Name))
                {
                    AllTreatments.Add(t);
                }
            }

            IsEmpty = AllTreatments.Count == 0;
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
    public void OpenTreatmentDialog(TreatmentItem treatment)
    {
        SelectedTreatment = treatment;
        IsDialogVisible = true;
    }

    [RelayCommand]
    public void CloseDialog()
    {
        IsDialogVisible = false;
        SelectedTreatment = null;
    }

    [RelayCommand]
    public void OpenAddDialog()
    {
        NewName = string.Empty;
        NewDescription = string.Empty;
        NewDurationMinutes = "15";
        NewType = string.Empty;
        NewPrice = "50";
        IsAddDialogVisible = true;
        NewRequiredRoleId = 1;
        NewFlag = "0_S_1";
    }

    [RelayCommand]
    public void CloseAddDialog()
    {
        IsAddDialogVisible = false;
    }

    [RelayCommand]
    public async Task AddTreatmentAsync()
    {
        if (string.IsNullOrWhiteSpace(NewName))
        {
            StatusMessage = "Podaj nazwę zabiegu.";
            return;
        }

        if (!int.TryParse(NewDurationMinutes, out var duration) || duration <= 0)
        {
            StatusMessage = "Podaj poprawny czas trwania w minutach.";
            return;
        }

        int.TryParse(NewPrice, out var price);

        try
        {
            IsLoading = true;
            StatusMessage = null;
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;

            var newTreatment = new CreateTreatmentDto
            {
                Name = NewName.Trim(),
                Description = NewDescription.Trim(),
                DurationMinutes = duration,
                Type = NewType.Trim(),
                Price = price,
                RequiredRoleId = NewRequiredRoleId,
                Flag = NewFlag.Trim()
            };

            await _treatmentService.CreateTreatmentItemAsync(newTreatment, ct);
            IsAddDialogVisible = false;
            await LoadAllAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Błąd dodawania zabiegu: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task DeleteTreatmentAsync(TreatmentItem? treatment)
    {
        var target = treatment ?? SelectedTreatment;
        if (target == null) return;

        try
        {
            IsLoading = true;
            StatusMessage = null;
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;
            await _treatmentService.DeleteTreatmentItemAsync(target.Id,ct);
            AllTreatments.Remove(target);
            IsEmpty = AllTreatments.Count == 0;
            IsDialogVisible = false;
            SelectedTreatment = null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Błąd usuwania zabiegu: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void GoBack() => _goBack?.Invoke();
}