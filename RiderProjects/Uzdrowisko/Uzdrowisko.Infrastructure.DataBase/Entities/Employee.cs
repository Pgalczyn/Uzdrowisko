using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Uzdrowisko.Infrastructure.DataBase.Entities.Abstractions;

namespace Uzdrowisko.Infrastructure.DataBase.Entities;

public class Employee : IHasPassword
{
    public int Id { get; set; }
    [MaxLength(30)]
    public required string FirstName { get; set; }
    [MaxLength(30)]
    public required string LastName { get; set; }
    [MaxLength(40)]
    public required string Email { get; set; }
    [MaxLength(84)]
    public required  string PasswordHash { get; set; }  = string.Empty;
    [Required]
    public DateOnly? DateOfBirth { get; set; }
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    [MaxLength(40)]
    public string? Address { get; set; }
    [MaxLength(20)]
    public string? City { get; set; }
    [MaxLength(20)]
    public required string Role { get; set; }
    [MaxLength(20)]
    public string? PostalCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public required bool Active { get; set; } = true;
}