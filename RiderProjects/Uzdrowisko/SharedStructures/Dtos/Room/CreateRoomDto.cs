using System.ComponentModel.DataAnnotations;
namespace SharedStructures.Dtos.Room;

public class CreateRoomDto
{
    [Required(ErrorMessage = "Numer gabinetu jest wymagany.")]
    [MaxLength(20, ErrorMessage = "Numer gabinetu nie może przekraczać 20 znaków.")]
    public required string Number { get; set; }

    [MaxLength(100, ErrorMessage = "Nazwa gabinetu nie może przekraczać 100 znaków.")]
    public string? Name { get; set; }

    [MaxLength(100, ErrorMessage = "Opis lokalizacji nie może przekraczać 100 znaków.")]
    public string? Location { get; set; }
    
    public int? Capacity { get; set; }
}