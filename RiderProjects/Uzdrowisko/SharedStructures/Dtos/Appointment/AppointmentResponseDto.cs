namespace SharedStructures.Dtos.Appointment;

public class AppointmentResponseDto
{
    public int Id { get; set; }
    public int ScheduleId { get; set; }

    public int TreatmentId { get; set; }
    public string TreatmentName { get; set; } = string.Empty;
    public int TreatmentDurationMinutes { get; set; }
    
    public int PatientId { get; set; }
    public string PatientFullName { get; set; } = string.Empty;

    public int? EmployeeId { get; set; }
    public string? EmployeeFullName { get; set; }

    public int? RoomId { get; set; }
    public string? RoomNumber { get; set; }
    public string? RoomName { get; set; }

    public DateOnly Date { get; set; }
    public TimeOnly TimeFrom { get; set; }
    public TimeOnly TimeTo { get; set; }

    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }

    // Właściwości pomocnicze wyliczane na bieżąco dla widoków XAML
    public string TimeRangeDisplay => $"{TimeFrom:HH\\:mm} - {TimeTo:HH\\:mm}";
    public string LocationDisplay => RoomNumber != null ? $"Gabinet {RoomNumber}" : "Brak przypisanej sali";
    public string StaffDisplay => EmployeeFullName ?? "Do przydzielenia";
}