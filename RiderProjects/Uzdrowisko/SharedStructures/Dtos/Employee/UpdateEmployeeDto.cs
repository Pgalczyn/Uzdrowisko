using System.ComponentModel.DataAnnotations;

namespace SharedStructures.Dtos.Employee;

public class UpdateEmployeeDto
{
    [MaxLength(30, ErrorMessage = "Imię nie może przekraczać 30 znaków.")]
    public string? Name { get; set; }

    [MaxLength(30, ErrorMessage = "Nazwisko nie może przekraczać 30 znaków.")]
    public string? Surname { get; set; }

    [EmailAddress(ErrorMessage = "Podaj poprawny format adresu e-mail.")]
    [MaxLength(40, ErrorMessage = "Adres e-mail nie może przekraczać 40 znaków.")]
    public string? Email { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [Phone(ErrorMessage = "Niepoprawny format numeru telefonu.")]
    [MaxLength(20, ErrorMessage = "Numer telefonu nie może przekraczać 20 znaków.")]
    public string? PhoneNumber { get; set; }

    [MaxLength(40, ErrorMessage = "Adres nie może przekraczać 40 znaków.")]
    public string? Address { get; set; }

    [MaxLength(20, ErrorMessage = "Nazwa miejscowości nie może przekraczać 20 znaków.")]
    public string? City { get; set; }

    [RegularExpression(@"^\d{2}-\d{3}$", ErrorMessage = "Kod pocztowy musi mieć format XX-XXX.")]
    [MaxLength(20, ErrorMessage = "Kod pocztowy nie może przekraczać 20 znaków.")]
    public string? PostalCode { get; set; }

    [Required(ErrorMessage = "Rola pracownika jest wymagana.")]
    [MaxLength(20, ErrorMessage = "Nazwa roli nie może przekraczać 20 znaków.")]
    public string? Role { get; set; }

    public bool? Active { get; set; }
}