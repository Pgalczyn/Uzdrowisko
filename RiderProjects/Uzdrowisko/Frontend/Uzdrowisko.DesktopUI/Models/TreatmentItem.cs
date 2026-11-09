using SharedStructures.Dtos.Treatment;

namespace Uzdrowisko.DesktopUI.Models;

public class TreatmentItem
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DurationMinutes { get; set; }

    public string? Type { get; set; }

    public bool Active { get; set; }


    // Sformatowana nazwa do list rozwijanych (ComboBox)
    public string DisplayName => !string.IsNullOrWhiteSpace(Type)
        ? $"{Name} ({DurationMinutes} min) – [{Type}]"
        : $"{Name} ({DurationMinutes} min)";

    public string DurationDisplay => $"{DurationMinutes} min";

    public string TypeDisplay => string.IsNullOrWhiteSpace(Type) ? "Ogólny" : Type;

    public string DescriptionDisplay => string.IsNullOrWhiteSpace(Description)
        ? "Brak opisu zabiegu."
        : Description;

    public string StatusDisplay => Active ? "Dostępny" : "Niedostępny";

    public static TreatmentItem FromDto(TreatmentResponseDto dto)
    {
        return new TreatmentItem
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            DurationMinutes = dto.DurationMinutes,
            Type = dto.Type,
            Active = dto.Active
        };
    }
}