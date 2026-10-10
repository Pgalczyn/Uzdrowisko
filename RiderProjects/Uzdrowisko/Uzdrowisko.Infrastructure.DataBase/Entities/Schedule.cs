using Microsoft.EntityFrameworkCore;
namespace Uzdrowisko.Infrastructure.DataBase.Entities;

[Index(nameof(StayId), IsUnique = true)]
public class Schedule
{
    public int Id { get; set; }
    public int StayId { get; set; }

    public Stay Stay { get; set; } = null!;

    public bool Active { get; set; } = true;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}