using System;
using SharedStructures.Dtos.Schedule;

namespace Uzdrowisko.DesktopUI.Models;

public class ScheduleItem
{
    public int Id { get; set; }

    public int StayId { get; set; }

    public string PatientFullName { get; set; } = string.Empty;

    public string PatientPesel { get; set; } = string.Empty;

    public DateOnly StayDateFrom { get; set; }

    public DateOnly StayDateTo { get; set; }

    public int AppointmentsCount { get; set; }

    public bool Active { get; set; }


    public string StayPeriodDisplay => $"{StayDateFrom:dd.MM.yyyy} – {StayDateTo:dd.MM.yyyy}";

    public string PatientDisplay => !string.IsNullOrWhiteSpace(PatientPesel)
        ? $"{PatientFullName} (PESEL: {PatientPesel})"
        : PatientFullName;

    public string DisplayName => $"{PatientFullName} – Harmonogram [{StayPeriodDisplay}]";

    public string AppointmentsCountDisplay => AppointmentsCount switch
    {
        1 => "1 zabieg",
        >= 2 and <= 4 => $"{AppointmentsCount} zabiegi",
        _ => $"{AppointmentsCount} zabiegów"
    };

    public string ActiveDisplay => Active ? "Aktywny" : "Anulowany";


    public static ScheduleItem FromDto(ScheduleResponseDto dto)
    {
        return new ScheduleItem
        {
            Id = dto.Id,
            StayId = dto.StayId,
            PatientFullName = dto.PatientFullName,
            StayDateFrom = dto.StayDateFrom,
            StayDateTo = dto.StayDateTo,
            AppointmentsCount = dto.AppointmentsCount,
            Active = dto.Active
        };
    }
}
