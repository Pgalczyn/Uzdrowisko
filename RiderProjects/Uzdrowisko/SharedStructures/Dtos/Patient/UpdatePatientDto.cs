using System.ComponentModel.DataAnnotations;

namespace SharedStructures.Dtos.Patient;

public class UpdatePatientDto
{

    [MaxLength(30, ErrorMessage = "Imię nie może przekraczać 30 znaków.")]
    public string? Name { get; set; }
    
    [MaxLength(30, ErrorMessage = "Nazwisko nie może przekraczać 30 znaków.")]
    public string? Surname { get; set; }
    
    public DateOnly? DateOfBirth { get; set; }

    [Phone(ErrorMessage = "Niepoprawny format numeru telefonu.")]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [EmailAddress(ErrorMessage = "Niepoprawny format adresu e-mail.")]
    [MaxLength(40)]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? Address { get; set; }

    [MaxLength(50)]
    public string? City { get; set; }

    [RegularExpression(@"^\d{2}-\d{3}$", ErrorMessage = "Kod pocztowy musi mieć format XX-XXX.")]
    public string? PostalCode { get; set; }

    public bool? Active { get; set; }
}