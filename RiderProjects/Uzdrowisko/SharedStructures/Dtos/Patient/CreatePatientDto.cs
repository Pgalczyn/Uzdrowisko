using System.ComponentModel.DataAnnotations;

namespace SharedStructures.Dtos.Patient;

public class CreatePatientDto
{
    
    [Required(ErrorMessage = "Imie jest wymagane.")]
    [MaxLength(30, ErrorMessage = "Imie  nie może przekraczać 30 znaków.")]
    public required string Name { get; set; }
    
    [Required(ErrorMessage = "Nazwisko jest wymagane.")]
    [MaxLength(30, ErrorMessage = "Nazwisko nie może przekraczać 30 znaków.")]
    public required string Surname { get; set; }

    [Required(ErrorMessage = "Data urodzenia jest wymagana.")]
    public DateOnly DateOfBirth { get; set; }
    
    [Required(ErrorMessage = "Hasło jest wymagane.")]
    [MinLength(8, ErrorMessage = "Hasło musi mieć co najmniej 8 znaków.")]
    public required string PasswordRaw { get; set; }
    
    [Required(ErrorMessage = "Numer PESEL jest wymagany.")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "PESEL musi składać się z dokładnie 11 cyfr.")]
    public required string Pesel { get; set; }

    [Phone(ErrorMessage = "Niepoprawny format numeru telefonu.")]
    [MaxLength(20, ErrorMessage = "Numer telefonu nie może przekraczać 20 znaków.")]
    public string? PhoneNumber { get; set; }

    [EmailAddress(ErrorMessage = "Niepoprawny format adresu e-mail.")]
    [MaxLength(40, ErrorMessage = "Adres e-mail nie może przekraczać 40 znaków.")]
    public string? Email { get; set; }

    [MaxLength(50, ErrorMessage = "Adres nie może przekraczać 50 znaków.")]
    public string? Address { get; set; }

    [MaxLength(50, ErrorMessage = "Miasto nie może przekraczać 50 znaków.")]
    public string? City { get; set; }

    [RegularExpression(@"^\d{2}-\d{3}$", ErrorMessage = "Kod pocztowy musi mieć format XX-XXX.")]
    public string? PostalCode { get; set; }
}