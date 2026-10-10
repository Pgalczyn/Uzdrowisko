namespace SharedStructures.Dtos.Schedule;
public class ScheduleResponseDto
{
    public int Id { get; set; }
    public int StayId { get; set; }
    
    public string PatientFullName { get; set; } = string.Empty;
    public DateOnly StayDateFrom { get; set; }
    public DateOnly StayDateTo { get; set; }
    
    public bool Active { get; set; }
    public int AppointmentsCount { get; set; }

    public string StayPeriodDisplay => $"{StayDateFrom:dd.MM.yyyy} - {StayDateTo:dd.MM.yyyy}";
}