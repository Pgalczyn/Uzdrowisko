using SharedStructures.Dtos.Room;

namespace Uzdrowisko.DesktopUI.Models;

public class RoomItem
{
    public int Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public string? Name { get; set; }

    public string? Location { get; set; }

    public bool Active { get; set; }
    
    public string LocationDisplay => string.IsNullOrWhiteSpace(Location)
        ? "Brak przypisanej lokalizacji"
        : Location;


    public string StatusDisplay => Active ? "Dostępny" : "Wyłączony z użytku";
    
    public static RoomItem FromDto(RoomResponseDto dto)
    {
        return new RoomItem
        {
            Id = dto.Id,
            Number = dto.Number,
            Name = dto.Name,
            Location = dto.Location,
            Active = dto.Active
        };
    }
}