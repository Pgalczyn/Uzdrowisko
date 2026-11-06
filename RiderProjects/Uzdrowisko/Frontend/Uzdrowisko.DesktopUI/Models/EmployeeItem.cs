using System;
using SharedStructures.Dtos.Employee;

namespace Uzdrowisko.DesktopUI.Models;

public class EmployeeItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public DateOnly? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Role { get; set; }

    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    

    public string FullName => $"{Name} {Surname}";
    
    public string FullAddress => $"{Address}, {City}, {PostalCode}";

    public string StatusDisplay => Active ? "Aktywny" : "Nieaktywny";

    public string RoleDisplay => string.IsNullOrWhiteSpace(Role) ? "Brak roli" : Role;

    public static EmployeeItem FromDto(EmployeeResponseDto dto)
    {
        return new EmployeeItem
        {
            Id = dto.Id,
            Name = dto.Name,
            Surname = dto.Surname,
            Email = dto.Email,
            DateOfBirth = dto.DateOfBirth,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address,
            City = dto.City,
            PostalCode = dto.PostalCode,
            Role = dto.Role,
            Active = dto.Active,
            CreatedAt = dto.CreatedAt
        };
    }
}