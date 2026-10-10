using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Uzdrowisko.Infrastructure.DataBase.Entities;

[Index(nameof(Pesel), IsUnique = true)]
public class Patient
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(30)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(30)]
    public required string Surname { get; set; }

    [Required]
    public DateOnly DateOfBirth { get; set; }

    [Required]
    [MaxLength(11)]
    public required string Pesel { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(40)]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? Address { get; set; }

    [MaxLength(50)]
    public string? City { get; set; }

    [MaxLength(10)]
    public string? PostalCode { get; set; }

    public bool Active { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}