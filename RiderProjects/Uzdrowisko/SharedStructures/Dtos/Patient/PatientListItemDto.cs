namespace SharedStructures.Dtos.Patient;

public class PatientListItemDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Pesel { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public bool Active { get; set; }
}