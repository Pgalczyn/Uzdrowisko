namespace SharedStructures.Dtos.Room;

public class RoomResponseDto
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Location { get; set; }
    
    public int? Capacity { get; set; }
    public bool Active { get; set; }

    public string DisplayName => !string.IsNullOrWhiteSpace(Name)
        ? $"Gabinet {Number} – {Name}"
        : $"Gabinet {Number}";
}