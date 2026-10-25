using System.ComponentModel.DataAnnotations;
namespace SharedStructures.Dtos.Stay;

public class CreateStayDto : IValidatableObject
{
    [Required(ErrorMessage = "Wskazanie pacjenta jest wymagane.")]
    [Range(1, int.MaxValue, ErrorMessage = "Nieprawidłowy identyfikator pacjenta.")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "Data rozpoczęcia pobytu jest wymagana.")]
    public DateOnly DateFrom { get; set; }

    [Required(ErrorMessage = "Data zakończenia pobytu jest wymagana.")]
    public DateOnly DateTo { get; set; }

    [MaxLength(20, ErrorMessage = "Status nie może przekraczać 20 znaków.")]
    public string? Status { get; set; } = "Zaplanowany";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateTo < DateFrom)
        {
            yield return new ValidationResult(
                "Data zakończenia pobytu nie może być wcześniejsza niż data rozpoczęcia.",
                new[] { nameof(DateTo) }
            );
        }
    }
}