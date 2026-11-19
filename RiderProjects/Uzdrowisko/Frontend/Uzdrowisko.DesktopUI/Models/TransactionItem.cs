using System;
using SharedStructures.Dtos.Transaction;

namespace Uzdrowisko.DesktopUI.Models;

public class TransactionItem
{
    public int Id { get; set; }
    public int TreatmentId { get; set; }
    
    public string TreatmentName { get; set; } = string.Empty;
    
    public int PatientId { get; set; }
    
    public decimal Price { get; set; }
    
    public DateTime Date { get; set; }
    public DateTime StartOfTreatment { get; set; }
    public DateTime EndOfTreatment { get; set; }
    public bool IsPaid { get; set; }
    public bool IsActive { get; set; }

    
    public string DisplayName => $"Paragon nr {Id} – {TreatmentName}";

    public string PriceDisplay => $"{Price:F2} zł";

    public string DateDisplay => Date.ToString("dd.MM.yyyy HH:mm");

    public string TreatmentTimeDisplay => $"{StartOfTreatment:HH:mm} - {EndOfTreatment:HH:mm}";

    public string PaymentStatusDisplay => IsPaid ? "Opłacono" : "Do zapłaty";

    public string StatusDisplay => IsActive ? "Aktywna" : "Anulowana";
    
    

    public static TransactionItem FromDto(TransactionResponseDto dto)
    {
        return new TransactionItem
        {
            Id = dto.Id,
            TreatmentId = dto.TreatmentId,
            PatientId = dto.PatientId,
            TreatmentName = dto.TreatmentName,
            Price = dto.Price,
            Date = dto.Date,
            StartOfTreatment = dto.StartOfTreatment,
            EndOfTreatment = dto.EndOfTreatment,
            IsPaid = dto.IsPaid,
            IsActive = dto.IsActive
        };
    }
}