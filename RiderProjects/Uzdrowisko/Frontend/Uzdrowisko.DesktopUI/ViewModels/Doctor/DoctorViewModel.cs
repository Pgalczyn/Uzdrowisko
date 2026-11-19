using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uzdrowisko.DesktopUI.Services.Abstractions;
namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class DoctorViewModel : ViewModelBase
{
    private readonly IPatientService _patientService;
    private readonly IEmployeeService _employeeService;
    private readonly Action<ViewModelBase> _navigateTo;
    private readonly IGenSchedule _genScheduleService;
    private readonly ITreatmentService _treatmentService;
    private readonly IAppointmentService _appointmentService;
    private readonly IStayService _stayService;

    public DoctorViewModel(
        IPatientService patientService, 
        IEmployeeService employeeService, 
        IGenSchedule genScheduleService,
        ITreatmentService treatmentService,
        Action<ViewModelBase> navigateTo,
        IAppointmentService appointmentService,
        IStayService stayService)
    {
        _patientService = patientService;
        _employeeService = employeeService;
        _genScheduleService = genScheduleService;
        _treatmentService = treatmentService;
        _navigateTo = navigateTo;
        _appointmentService = appointmentService;
        _stayService = stayService;
    }
    
    [RelayCommand] 
    private void OpenPatientView()
    {
        _navigateTo(new PatientViewModel(
            _patientService,

            () => _navigateTo(this),

            () => _navigateTo(new RegisterViewModel(
                _patientService,
                _stayService,
                () => OpenPatientView()
            )),

            (selectedPatient) => _navigateTo(new UpdateUserViewModel(
                selectedPatient,
                _patientService,
                _stayService,
                () => OpenPatientView()
            )),
            (selectedPatient) => _navigateTo(new GenerateScheduleViewModel(
                _treatmentService,
                _genScheduleService,
                selectedPatient,
                () => OpenPatientView()
            )),
            (selectedPatient) => _navigateTo(new PatientTreatmentsViewModel(
                _appointmentService,
                _treatmentService,
                _genScheduleService,
                _navigateTo,
                selectedPatient,
                () => OpenPatientView()
            ))
        ));
    }
}