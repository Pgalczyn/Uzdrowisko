using System;
using SharedStructures.Dtos.Stay;

namespace Uzdrowisko.DesktopUI.Models;

public class StayItem
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public string PatientFullName { get; set; } = string.Empty;

    public string PatientPesel { get; set; } = string.Empty;

    public DateOnly DateFrom { get; set; }

    public DateOnly DateTo { get; set; }

    public string? Status { get; set; }

    public bool Active { get; set; }


    public int TotalDays => DateTo.DayNumber >= DateFrom.DayNumber 
        ? DateTo.DayNumber - DateFrom.DayNumber 
        : 0;

    public string PeriodDisplay => $"{DateFrom:dd.MM.yyyy} – {DateTo:dd.MM.yyyy}";

    public string DurationDisplay => $"{TotalDays} {(TotalDays == 1 ? "dzień" : "dni")}";

    public string PatientDisplay => !string.IsNullOrWhiteSpace(PatientPesel)
        ? $"{PatientFullName} (PESEL: {PatientPesel})"
        : PatientFullName;

    public string DisplayName => $"{PatientFullName} [{PeriodDisplay}]";

    public string StatusDisplay => string.IsNullOrWhiteSpace(Status)
        ? "Zaplanowany"
        : Status;

    public string ActiveDisplay => Active ? "Aktywny" : "Anulowany";


    public static StayItem FromDto(StayResponseDto dto)
    {
        return new StayItem
        {
            Id = dto.Id,
            PatientId = dto.PatientId,
            PatientFullName = dto.PatientFullName,
            PatientPesel = dto.PatientPesel,
            DateFrom = dto.DateFrom,
            DateTo = dto.DateTo,
            Status = dto.Status,
            Active = dto.Active
        };
    }
}
