using System;
using SharedStructures.Dtos.Appointment;

namespace Uzdrowisko.DesktopUI.Models;

public class AppointmentItem
{
    public int Id { get; set; }

    public int ScheduleId { get; set; }

    public int TreatmentId { get; set; }
    public string TreatmentName { get; set; } = string.Empty;

    public int PatientId { get; set; }
    public string PatientFullName { get; set; } = string.Empty;

    public int? EmployeeId { get; set; }
    public string? EmployeeFullName { get; set; }

    public int? RoomId { get; set; }
    public string? RoomNumber { get; set; }

    public DateOnly Date { get; set; }
    public TimeOnly TimeFrom { get; set; }
    public TimeOnly TimeTo { get; set; }

    public string Status { get; set; } = "Zaplanowana";

    public string? Notes { get; set; }


    public string DateDisplay => Date.ToString("dd.MM.yyyy");

    public string TimeRangeDisplay => $"{TimeFrom:HH:mm} – {TimeTo:HH:mm}";

    public string FullDateTimeDisplay => $"{DateDisplay} {TimeRangeDisplay}";

    public int DurationMinutes => (int)(TimeTo - TimeFrom).TotalMinutes;

    public string DurationDisplay => $"{DurationMinutes} min";

    public string EmployeeDisplay => !string.IsNullOrWhiteSpace(EmployeeFullName)
        ? EmployeeFullName
        : "Nieprzypisany pracownik";

    public string RoomDisplay => !string.IsNullOrWhiteSpace(RoomNumber)
        ? $"Gabinet {RoomNumber}"
        : "Brak przypisanego gabinetu";

    public string NotesDisplay => string.IsNullOrWhiteSpace(Notes)
        ? "Brak dodatkowych uwag."
        : Notes;

    public string DisplayName => $"{TimeRangeDisplay}: {TreatmentName} – {PatientFullName} ({RoomDisplay})";


    public static AppointmentItem FromDto(AppointmentResponseDto dto)
    {
        return new AppointmentItem
        {
            Id = dto.Id,
            ScheduleId = dto.ScheduleId,
            TreatmentId = dto.TreatmentId,
            TreatmentName = dto.TreatmentName,
            PatientId = dto.PatientId,
            PatientFullName = dto.PatientFullName,
            EmployeeId = dto.EmployeeId,
            EmployeeFullName = dto.EmployeeFullName,
            RoomId = dto.RoomId,
            RoomNumber = dto.RoomNumber,
            Date = dto.Date,
            TimeFrom = dto.TimeFrom,
            TimeTo = dto.TimeTo,
            Status = dto.Status,
            Notes = dto.Notes
        };
    }
}
