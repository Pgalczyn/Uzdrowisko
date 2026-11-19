using System;
using System.ComponentModel.DataAnnotations;

namespace SharedStructures.Dtos.Transaction;

public class CreateTransactionDto
{
    [Required(ErrorMessage = "Wskazanie zabiegu jest wymagane.")]
    [Range(1, int.MaxValue, ErrorMessage = "Nieprawidłowy identyfikator zabiegu.")]
    public int TreatmentId { get; set; }
    [Required]
    public int PatientId { get; set; }
    
    [Required(ErrorMessage = "Podanie ceny jest wymagane.")]
    [Range(0, int.MaxValue, ErrorMessage = "Cena transakcji nie może być ujemna.")]
    public int Price { get; set; }
    
    [Required]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "Data rozpoczęcia zabiegu jest wymagana.")]
    public DateTime StartOfTreatment { get; set; }

    [Required(ErrorMessage = "Data zakończenia zabiegu jest wymagana.")]
    public DateTime EndOfTreatment { get; set; }

    public bool IsPaid { get; set; }
}