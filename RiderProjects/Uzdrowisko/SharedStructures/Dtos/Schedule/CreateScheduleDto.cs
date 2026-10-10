using System.ComponentModel.DataAnnotations;
namespace SharedStructures.Dtos.Schedule;

public class CreateScheduleDto
{
    [Required(ErrorMessage = "Wskazanie pobytu jest wymagane.")]
    [Range(1, int.MaxValue, ErrorMessage = "Nieprawidłowy identyfikator pobytu.")]
    public int StayId { get; set; }
}