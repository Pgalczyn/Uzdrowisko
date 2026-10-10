using System.ComponentModel.DataAnnotations;
namespace Uzdrowisko.Infrastructure.DataBase.Entities;

public class Appointment
{
    public int Id { get; set; }
    
    public int ScheduleId { get; set; }
    public Schedule Schedule { get; set; } = null!; 

    public int TreatmentId { get; set; }
    public Treatment Treatment { get; set; } = null!;

    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public int? RoomId { get; set; }
    public Room? Room { get; set; }

    public DateOnly Date { get; set; }
    public TimeOnly TimeFrom { get; set; }
    public TimeOnly TimeTo { get; set; }

    [Required]
    [MaxLength(30)]
    public required string Status { get; set; } = "Zaplanowana";

    // Typ 'text' w PostgreSQL -> string bez atrybutu MaxLength
    public string? Notes { get; set; }
}