using System.ComponentModel.DataAnnotations;
namespace Uzdrowisko.Infrastructure.DataBase.Entities;

public class Stay
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public Patient Patient { get; set; } = null!;

    public DateOnly DateFrom { get; set; }

    public DateOnly DateTo { get; set; }

    [MaxLength(20)]
    public string? Status { get; set; }

    public bool Active { get; set; } = true;
}