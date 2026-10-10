namespace SharedStructures.Dtos.Treatment;

public class TreatmentResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public string? Type { get; set; }
    public bool Active { get; set; }

    // Właściwość ułatwiająca wyświetlanie w kontrolkach ComboBox w Avalonia:
    public string DisplayName => $"{Name} ({DurationMinutes} min) - [{Type ?? "Ogólny"}]";
}