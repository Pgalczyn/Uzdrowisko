using System.ComponentModel.DataAnnotations;
namespace SharedStructures.Dtos.Treatment;

public class UpdateTreatmentDto
{
    [MaxLength(50, ErrorMessage = "Nazwa zabiegu nie może przekraczać 50 znaków.")]
    public string? Name { get; set; }

    [MaxLength(500, ErrorMessage = "Opis zabiegu nie może przekraczać 500 znaków.")]
    public string? Description { get; set; }
    
    [Range(5, 240, ErrorMessage = "Czas trwania zabiegu musi wynosić od 5 do 240 minut.")]
    public int? DurationMinutes { get; set; }

    [MaxLength(30, ErrorMessage = "Typ zabiegu nie może przekraczać 30 znaków.")]
    public string? Type { get; set; }

    public bool? Active { get; set; }
    
    public string? Flag { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; } = DateTime.Now;
}