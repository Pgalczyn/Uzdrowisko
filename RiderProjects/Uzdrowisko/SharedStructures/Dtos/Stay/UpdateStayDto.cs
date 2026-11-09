using System.ComponentModel.DataAnnotations;
namespace SharedStructures.Dtos.Stay;

public class UpdateStayDto : IValidatableObject
{
    public DateOnly? DateFrom { get; set; }

    public DateOnly? DateTo { get; set; }

    [MaxLength(20, ErrorMessage = "Status nie może przekraczać 20 znaków.")]
    public string? Status { get; set; }

    public bool? Active { get; set; }

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