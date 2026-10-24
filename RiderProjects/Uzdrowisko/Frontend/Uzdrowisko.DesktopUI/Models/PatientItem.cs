using System;
using SharedStructures.Dtos.Patient;

namespace Uzdrowisko.DesktopUI.Models;

public class PatientItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Pesel { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
        
    public string FullName => $"{Name} {Surname}";

    public int Age
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var age = today.Year - DateOfBirth.Year;
            if (DateOfBirth > today.AddYears(-age)) age--;
            return age;
        }
    }
    
    public string FullAddress => $"{Address}, {City}, {PostalCode}";

    public string StatusDisplay => Active ? "Aktywny" : "Nieaktywny";
    
    
    public static PatientItem FromDto(PatientResponseDto dto)
    {
        return new PatientItem
        {
            Id = dto.Id,
            Name = dto.Name,
            Surname = dto.Surname,
            Pesel = dto.Pesel,
            DateOfBirth = dto.DateOfBirth,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            Address = dto.Address,
            City = dto.City,
            PostalCode = dto.PostalCode,
            Active = dto.Active,
            CreatedAt = dto.CreatedAt
        };
    }
}