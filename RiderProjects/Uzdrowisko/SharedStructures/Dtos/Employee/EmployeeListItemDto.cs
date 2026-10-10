namespace SharedStructures.Dtos.Employee;

public class EmployeeListItemDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string? PhoneNumber { get; set; }
    public bool Active { get; set; }
}