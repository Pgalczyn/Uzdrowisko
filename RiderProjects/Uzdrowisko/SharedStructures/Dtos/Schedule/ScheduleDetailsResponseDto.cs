using SharedStructures.Dtos.Appointment;
namespace SharedStructures.Dtos.Schedule;

public class ScheduleDetailsResponseDto
{
    public int Id { get; set; }
    public int StayId { get; set; }

    public int PatientId { get; set; }
    public string PatientFullName { get; set; } = string.Empty;
    public string PatientPesel { get; set; } = string.Empty;

    public DateOnly StayDateFrom { get; set; }
    public DateOnly StayDateTo { get; set; }
    public bool Active { get; set; }

    public List<AppointmentResponseDto> Appointments { get; set; } = new();
}