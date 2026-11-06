using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharedStructures.Dtos.Employee;
using Uzdrowisko.DesktopUI.Models;
using Uzdrowisko.DesktopUI.Services.Abstractions;

namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class EmployeeViewModel : ViewModelBase
{
    private readonly IEmployeeService _employeeService;
    private readonly Action _goBack;
    private readonly Action _goToRegister;
    private readonly Action<EmployeeItem> _goToEdit;
    private CancellationTokenSource? _cts;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isDialogVisible;
    [ObservableProperty] private EmployeeItem? _selectedEmployee;

    public ObservableCollection<EmployeeItem> EmployeeListItemDtos { get; } = new();

    public EmployeeViewModel(IEmployeeService service, Action goBack, Action goToRegister, Action<EmployeeItem> goToEdit)
    {
        _employeeService = service;
        _goBack = goBack;
        _goToRegister = goToRegister;
        _goToEdit = goToEdit;
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
            var items = await _employeeService.GetAllEmployeesAsync(ct);
            EmployeeListItemDtos.Clear();
            if (items != null)
            {
                foreach (var item in items)
                {
                    EmployeeListItemDtos.Add(item);
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
    public void OpenRemoveDialog(EmployeeItem employee)
    {
        SelectedEmployee = employee;
        IsDialogVisible = true;
    }
    [RelayCommand]
    private void GoToEdit(EmployeeItem employee) => _goToEdit(employee);

    [RelayCommand]
    public async Task RemoveEmployeeAsync()
    {
        if (SelectedEmployee == null) return;
        
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;
        
        try
        {
            IsLoading = true;
            await _employeeService.DeleteEmployeeItemAsync(SelectedEmployee.Id, ct); 
            EmployeeListItemDtos.Remove(SelectedEmployee);
            CloseRemoveDialog();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Błąd usuwania pracownika: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void CloseRemoveDialog()
    {
        IsDialogVisible = false;
    }

    [RelayCommand]
    private void GoBack() => _goBack();

    [RelayCommand]
    private void GoToRegister() => _goToRegister();
}