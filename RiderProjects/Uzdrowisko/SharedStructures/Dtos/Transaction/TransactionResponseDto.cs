
namespace SharedStructures.Dtos.Transaction;

public class TransactionResponseDto
{
    public int Id { get; set; }
    public int TreatmentId { get; set; }
    
    public string TreatmentName { get; set; } = String.Empty;
    public int PatientId { get; set; }
    public int Price { get; set; }
    public DateTime Date { get; set; }
    public DateTime StartOfTreatment { get; set; }
    public DateTime EndOfTreatment { get; set; }
    public bool IsPaid { get; set; }
    public bool IsActive { get; set; }
}