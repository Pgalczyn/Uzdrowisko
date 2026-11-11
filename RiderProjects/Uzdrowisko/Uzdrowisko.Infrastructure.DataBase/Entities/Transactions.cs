using System.ComponentModel.DataAnnotations;

namespace Uzdrowisko.Infrastructure.DataBase.Entities;

public class Transactions
{
    public int Id { get; set; }
    
    public int TreatmentId { get; set; }
    
    public Treatment Treatment { get; set; } = null!;
    
    public int PatientId { get; set; }
    
    public Patient Patient { get; set; } = null!;
    
    public int Price { get; set; }
    
    public DateTime Date { get; set; }
    
    public DateTime StartOfTreatment { get; set; }
    
    public DateTime EndOfTreatment { get; set; }
    
    public bool IsPaid { get; set; }

    public bool IsActive { get; set; } = true;
}