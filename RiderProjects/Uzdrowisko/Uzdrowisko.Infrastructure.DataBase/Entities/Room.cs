using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Uzdrowisko.Infrastructure.DataBase.Entities;

[Index(nameof(Number), IsUnique = true)]
public class Room
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string? Name { get; set; }

    [Required]
    [MaxLength(20)]
    public required string Number { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }

    public bool Active { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}