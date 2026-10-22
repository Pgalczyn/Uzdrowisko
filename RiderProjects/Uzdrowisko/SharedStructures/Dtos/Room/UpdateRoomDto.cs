using System.ComponentModel.DataAnnotations;
namespace SharedStructures.Dtos.Room;

public class UpdateRoomDto
{
    [MaxLength(20, ErrorMessage = "Numer gabinetu nie może przekraczać 20 znaków.")]
    public string? Number { get; set; }

    [MaxLength(100, ErrorMessage = "Nazwa gabinetu nie może przekraczać 100 znaków.")]
    public string? Name { get; set; }

    [MaxLength(100, ErrorMessage = "Opis lokalizacji nie może przekraczać 100 znaków.")]
    public string? Location { get; set; }
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool? Active { get; set; }
}