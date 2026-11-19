using System;
using System.Threading.Tasks;
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
    private readonly ITreatmentService  _treatmentService;
    private readonly IStayService _stayService;

    public AdminViewModel(
        IPatientService patientService, 
        IEmployeeService employeeService,
        ITreatmentService treatmentService,
        IStayService stayService,
        Action<ViewModelBase> navigateTo)
    {
        _patientService = patientService;
        _employeeService = employeeService;
        _treatmentService = treatmentService;
        _stayService = stayService;
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
                _stayService,
                () => OpenUserView() 
            )),
            (selectedPatient) => _navigateTo(new UpdateUserViewModel(
                selectedPatient,
                _patientService, 
                _stayService,
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
    [RelayCommand] 
    private void OpenEditTreatmentsView() 
    {
        _navigateTo(new EditTreatmentsViewModel(
            _treatmentService, 
            _navigateTo,
        
            () => _navigateTo(this)
            
        ));
    }
    
}