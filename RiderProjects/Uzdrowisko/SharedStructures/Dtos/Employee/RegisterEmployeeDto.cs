using System.ComponentModel.DataAnnotations;
using SharedStructures.Enums;

namespace SharedStructures.Dtos.Employee;

public class RegisterEmployeeDto
{
    [Required(ErrorMessage = "Imię jest wymagane.")]
    [MaxLength(30, ErrorMessage = "Imię nie może przekraczać 30 znaków.")]
    public required string FirstName { get; set; }

    [Required(ErrorMessage = "Nazwisko jest wymagane.")]
    [MaxLength(30, ErrorMessage = "Nazwisko nie może przekraczać 30 znaków.")]
    public required string LastName { get; set; }

    [Required(ErrorMessage = "Adres e-mail jest wymagany.")]
    [EmailAddress(ErrorMessage = "Podaj poprawny adres e-mail.")]
    [MaxLength(40, ErrorMessage = "Adres e-mail nie może przekraczać 40 znaków.")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Hasło jest wymagane.")]
    [MinLength(8, ErrorMessage = "Hasło musi mieć co najmniej 8 znaków.")]
    public required string Password { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [Phone(ErrorMessage = "Niepoprawny format numeru telefonu.")]
    [MaxLength(20, ErrorMessage = "Numer telefonu nie może przekraczać 20 znaków.")]
    public string? PhoneNumber { get; set; }

    [MaxLength(40, ErrorMessage = "Adres nie może przekraczać 40 znaków.")]
    public string? Address { get; set; }

    [MaxLength(20, ErrorMessage = "Miasto nie może przekraczać 20 znaków.")]
    public string? City { get; set; }

    [RegularExpression(@"^\d{2}-\d{3}$", ErrorMessage = "Kod pocztowy musi mieć format XX-XXX.")]
    public string? PostalCode { get; set; }

    [Required(ErrorMessage = "Musisz wybrać rolę dla pracownika.")]
    [EnumDataType(typeof(EmployeeRole), ErrorMessage = "Wybrana rola jest nieprawidłowa.")]
    public required string Role { get; set; }
}