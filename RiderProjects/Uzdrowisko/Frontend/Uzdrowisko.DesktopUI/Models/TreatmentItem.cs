using System;

namespace Uzdrowisko.DesktopUI.Models;

public record TreatmentItem(
    int Id,
    string Name,
    string Patient,
    string Room,
    DateTime Start,
    TimeSpan Duration)
{
    public DateTime End => Start + Duration;
    public string TimeRange => $"{Start:HH\\:mm} – {End:HH\\:mm}";
}