using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uzdrowisko.DesktopUI.Models;
using Uzdrowisko.DesktopUI.Services.Abstractions;

namespace Uzdrowisko.DesktopUI.ViewModels;

public partial class ChooseAppointmentViewModel : ViewModelBase
{
    private readonly ITreatmentService _treatmentService;
    private readonly IAppointmentService _appointmentService;
    private readonly Action<ViewModelBase> _navigateTo;

    [ObservableProperty] private bool _isPostponeDialogVisible = true;
    [ObservableProperty] private TreatmentItem? _selectedAppointment;
    [ObservableProperty] private AppointmentItem? _selectedTime;

    public ObservableCollection<AppointmentItem> AvailableTimes { get; } = new();

    private readonly List<AppointmentItem> _allHardcodedSlots = new()
    {
        new AppointmentItem
        {
            TreatmentName = "Kąpiel siarczkowa",
            Date = DateOnly.FromDateTime(DateTime.Today).AddDays(1),
            TimeFrom = new TimeOnly(10, 0),
            TimeTo = new TimeOnly(10, 20),
            PatientFullName = "Wolny termin"
        },
        new AppointmentItem
        {
            TreatmentName = "Kąpiel siarczkowa",
            Date = DateOnly.FromDateTime(DateTime.Today).AddDays(2),
            TimeFrom = new TimeOnly(12, 30),
            TimeTo = new TimeOnly(12, 50),
            PatientFullName = "Wolny termin"
        },
        new AppointmentItem
        {
            TreatmentName = "Masaż klasyczny częściowy",
            Date = DateOnly.FromDateTime(DateTime.Today).AddDays(1),
            TimeFrom = new TimeOnly(11, 0),
            TimeTo = new TimeOnly(11, 45),
            PatientFullName = "Wolny termin"
        },
        new AppointmentItem
        {
            TreatmentName = "Inhalacje solankowe",
            Date = DateOnly.FromDateTime(DateTime.Today),
            TimeFrom = new TimeOnly(16, 0),
            TimeTo = new TimeOnly(16, 15),
            PatientFullName = "Wolny termin"
        }
    };

    public ChooseAppointmentViewModel(
        ITreatmentService treatmentService,
        IAppointmentService service,
        Action<ViewModelBase> navigateTo,
        TreatmentItem appointmentToMove)
    {
        _treatmentService = treatmentService;
        _appointmentService = service;
        _navigateTo = navigateTo;
        SelectedAppointment = appointmentToMove;

        LoadAvailableTimes();
    }

    private void LoadAvailableTimes()
    {
        AvailableTimes.Clear();

        if (SelectedAppointment == null) return;

        var matchedSlots = _allHardcodedSlots
            .Where(slot =>
                string.Equals(slot.TreatmentName, SelectedAppointment.Name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var slot in matchedSlots)
        {
            AvailableTimes.Add(slot);
        }
    }

    [RelayCommand]
    public void CloseAppointmentDateView()
    {
        _navigateTo(new TreatmentsViewModel(_treatmentService, _appointmentService, _navigateTo));
    }
}