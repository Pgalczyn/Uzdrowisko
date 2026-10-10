namespace SharedStructures.Dtos.Stay;

public class StayResponseDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientFullName { get; set; } = string.Empty;
    public string PatientPesel { get; set; } = string.Empty;

    public DateOnly DateFrom { get; set; }
    public DateOnly DateTo { get; set; }
    public string? Status { get; set; }
    public bool Active { get; set; }
    
    public int TotalDays => DateTo.DayNumber - DateFrom.DayNumber;
    public string PeriodDisplay => $"{DateFrom:dd.MM.yyyy} - {DateTo:dd.MM.yyyy}";
}