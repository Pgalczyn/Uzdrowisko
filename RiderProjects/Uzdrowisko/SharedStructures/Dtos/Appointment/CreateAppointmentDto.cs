using System.ComponentModel.DataAnnotations;
namespace SharedStructures.Dtos.Appointment;

public class CreateAppointmentDto : IValidatableObject
{
    [Required(ErrorMessage = "Identyfikator harmonogramu jest wymagany.")]
    [Range(1, int.MaxValue, ErrorMessage = "Nieprawidłowy identyfikator harmonogramu.")]
    public int ScheduleId { get; set; }

    [Required(ErrorMessage = "Wskazanie zabiegu jest wymagane.")]
    [Range(1, int.MaxValue, ErrorMessage = "Nieprawidłowy identyfikator zabiegu.")]
    public int TreatmentId { get; set; }

    // Opcjonalne przypisanie fizjoterapeuty na etapie planowania
    [Range(1, int.MaxValue, ErrorMessage = "Nieprawidłowy identyfikator pracownika.")]
    public int? EmployeeId { get; set; }

    // Opcjonalne przypisanie gabinetu
    [Range(1, int.MaxValue, ErrorMessage = "Nieprawidłowy identyfikator gabinetu.")]
    public int? RoomId { get; set; }

    [Required(ErrorMessage = "Data zabiegu jest wymagana.")]
    public DateOnly Date { get; set; }

    [Required(ErrorMessage = "Godzina rozpoczęcia jest wymagana.")]
    public TimeOnly TimeFrom { get; set; }

    [Required(ErrorMessage = "Godzina zakończenia jest wymagana.")]
    public TimeOnly TimeTo { get; set; }

    [MaxLength(30, ErrorMessage = "Status nie może przekraczać 30 znaków.")]
    public string Status { get; set; } = "Zaplanowana";

    public string? Notes { get; set; }

    // Walidacja logiczna sprawdzana automatycznie przez [ApiController]
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (TimeTo <= TimeFrom)
        {
            yield return new ValidationResult(
                "Godzina zakończenia zabiegu musi być późniejsza niż godzina rozpoczęcia.",
                new[] { nameof(TimeTo) }
            );
        }
    }
}