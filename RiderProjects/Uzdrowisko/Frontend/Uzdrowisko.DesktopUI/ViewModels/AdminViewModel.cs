using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uzdrowisko.DesktopUI.Services.Abstractions;
using Uzdrowisko.Views;

namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class AdminViewModel : ViewModelBase
{
    private readonly IPatientService _patientService;
    private readonly IEmployeeService _employeeService;
    private readonly Action<ViewModelBase> _navigateTo;

    public AdminViewModel(
        IPatientService patientService, 
        IEmployeeService employeeService, 
        Action<ViewModelBase> navigateTo)
    {
        _patientService = patientService;
        _employeeService = employeeService;
        _navigateTo = navigateTo;
    }

    // [RelayCommand] 
    // private void OpenRegister() 
    //     => _navigateTo(new RegisterViewModel(_patientService,() => _navigateTo(this)));
    //
    // [RelayCommand] 
    // private void OpenEmployees() 
    //     => _navigateTo(new RegisterEmployeeViewModel(_employeeService, () => _navigateTo(this)));

    [RelayCommand] 
    private void OpenUserView() 
    {
        _navigateTo(new UserViewModel(
            _patientService, 
            () => _navigateTo(this), 
        
            () => _navigateTo(new RegisterViewModel(
                _patientService, 
                () => OpenUserView() 
            )),
            (selectedPatient) => _navigateTo(new UpdateUserViewModel(
                selectedPatient,
                _patientService, 
                () => OpenUserView()
            ))
        ));
    }
    [RelayCommand] 
    private void OpenEmployeeView() 
    {
        _navigateTo(new EmployeeViewModel(
            _employeeService, 
            () => _navigateTo(this), 
        
            () => _navigateTo(new RegisterEmployeeViewModel(
                _employeeService, 
                () => OpenEmployeeView() 
            )),
            (selectedEmployee) => _navigateTo(new UpdateEmployeeViewModel(
                selectedEmployee,
                _employeeService, 
                () => OpenEmployeeView()
            ))
        ));
    }
    
}