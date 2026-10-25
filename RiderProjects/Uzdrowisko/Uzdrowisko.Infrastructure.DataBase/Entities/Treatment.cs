using System.ComponentModel.DataAnnotations;

namespace Uzdrowisko.Infrastructure.DataBase.Entities;

public class Treatment
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(1, 480, ErrorMessage = "Czas trwania musi wynosić od 1 do 480 minut.")]
    public int DurationMinutes { get; set; }

    [MaxLength(30)]
    public string? Type { get; set; }

    public bool Active { get; set; } = true;
    
    public string Flag { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
}