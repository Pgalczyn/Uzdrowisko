using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedStructures.Dtos.Appointment;
using SharedStructures.Dtos.Treatment;
using Uzdrowisko.Infrastructure.DataBase.Context;
using Uzdrowisko.Infrastructure.DataBase.Entities;

namespace Uzdrowisko.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentController: ControllerBase
{
    private readonly UzdrowiskoDbContext _dbContext;
    
    public AppointmentController(UzdrowiskoDbContext dbContext)
        {
        _dbContext = dbContext;
        }

    [HttpGet]
    public async Task<ActionResult<List<AppointmentResponseDto>>> GetAllAppointments()
    {
        var appointments = await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.Status != "Usuniety")
            .Select(a => new AppointmentResponseDto()
            {
                Id = a.Id,
                ScheduleId = a.ScheduleId,
                TreatmentId = a.TreatmentId,
                TreatmentName = a.Treatment.Name,
                TreatmentDurationMinutes = a.Treatment.DurationMinutes,
                PatientId = a.Schedule.Stay.PatientId,
                PatientFullName = a.Schedule.Stay.Patient.Name + " " + a.Schedule.Stay.Patient.Surname,
                EmployeeId = a.EmployeeId,
                EmployeeFullName = a.Employee.Name + " " + a.Employee.Surname,
                RoomId = a.RoomId,
                RoomName = a.Room.Name,
                RoomNumber = a.Room.Number,
                Date = a.Date,
                TimeFrom = a.TimeFrom,
                TimeTo = a.TimeTo,
                Status = a.Status,
                Notes = a.Notes,
            }).ToListAsync();
            return Ok(appointments);
    }
    
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppointmentResponseDto>> GetppointmentById(int id)
    {
        var appointment = await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AppointmentResponseDto()
            {
                Id = a.Id,
                ScheduleId = a.ScheduleId,
                TreatmentId = a.TreatmentId,
                TreatmentName = a.Treatment.Name,
                TreatmentDurationMinutes = a.Treatment.DurationMinutes,
                PatientId = a.Schedule.Stay.PatientId,
                PatientFullName = a.Schedule.Stay.Patient.Name + " " + a.Schedule.Stay.Patient.Surname,
                EmployeeId = a.EmployeeId,
                EmployeeFullName = a.Employee.Name + " " + a.Employee.Surname,
                RoomId = a.RoomId,
                RoomName = a.Room.Name,
                RoomNumber = a.Room.Number,
                Date = a.Date,
                TimeFrom = a.TimeFrom,
                TimeTo = a.TimeTo,
                Status = a.Status,
                Notes = a.Notes,
            }).FirstOrDefaultAsync();
        
        if (appointment== null)
        {
            return NotFound($"Nie znaleziono terminu o ID {id}.");
        }
        return Ok(appointment);
    }
    [HttpPost]
    public async Task<ActionResult<AppointmentResponseDto>> CreateTreatment(
        [FromBody] CreateAppointmentDto createAppointmentDto)
    {
        var newAppointment = new Appointment
        {
            ScheduleId = createAppointmentDto.ScheduleId,
            TreatmentId = createAppointmentDto.TreatmentId,
            EmployeeId = createAppointmentDto.EmployeeId,
            RoomId = createAppointmentDto.RoomId,
            Date = createAppointmentDto.Date,
            TimeFrom = createAppointmentDto.TimeFrom,
            TimeTo = createAppointmentDto.TimeTo,
            Status = createAppointmentDto.Status ?? "Zaplanowano",
            Notes = createAppointmentDto.Notes
        };
        
        _dbContext.Appointments.Add(newAppointment);
        await _dbContext.SaveChangesAsync();

        var response = await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.Id == newAppointment.Id)
            .Select(a => new AppointmentResponseDto()
            {
                Id = a.Id,
                ScheduleId = a.ScheduleId,
                TreatmentId = a.TreatmentId,
                TreatmentName = a.Treatment.Name,
                TreatmentDurationMinutes = a.Treatment.DurationMinutes,
                PatientId = a.Schedule.Stay.PatientId,
                PatientFullName = a.Schedule.Stay.Patient.Name + " " + a.Schedule.Stay.Patient.Surname,
                EmployeeId = a.EmployeeId,
                EmployeeFullName = a.Employee != null ? (a.Employee.Name + " " + a.Employee.Surname) : string.Empty,
                RoomId = a.RoomId,
                RoomName = a.Room != null ? a.Room.Name : string.Empty,
                RoomNumber = a.Room != null ? a.Room.Number : string.Empty,
                Date = a.Date,
                TimeFrom = a.TimeFrom,
                TimeTo = a.TimeTo,
                Status = a.Status,
                Notes = a.Notes,
            }).FirstOrDefaultAsync();
        return CreatedAtAction(nameof(GetppointmentById), new { id = newAppointment.Id }, response);
}
    [HttpGet("{id}")]
    public async Task<ActionResult<AppointmentResponseDto>> GetAppointmentById(int id)
    {
        var appointment = await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AppointmentResponseDto
            {
                Id = a.Id,
                ScheduleId = a.ScheduleId,
                TreatmentId = a.TreatmentId,
                TreatmentName = a.Treatment.Name,
                TreatmentDurationMinutes = a.Treatment.DurationMinutes,
                PatientId = a.Schedule.Stay.PatientId,
                PatientFullName = a.Schedule.Stay.Patient.Name + " " + a.Schedule.Stay.Patient.Surname,
                EmployeeId = a.EmployeeId,
                EmployeeFullName = a.Employee != null ? (a.Employee.Name + " " + a.Employee.Surname) : string.Empty,
                RoomId = a.RoomId,
                RoomName = a.Room != null ? a.Room.Name : string.Empty,
                RoomNumber = a.Room != null ? a.Room.Number : string.Empty,
                Date = a.Date,
                TimeFrom = a.TimeFrom,
                TimeTo = a.TimeTo,
                Status = a.Status,
                Notes = a.Notes
            })
            .FirstOrDefaultAsync();
        if (appointment == null)
        {
            return NotFound($"Termin o takim id: {id} nie istnieje");
        }
        return Ok(appointment);

    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAppointmentById(int id)
    {
        var appointment = await _dbContext.Appointments.FindAsync(id);
        if (appointment == null)
        {
            return NotFound($"Nie ma takiego terminu o ID {id}.");
        }
        
        appointment.Status = "Usuniety";
        
        await _dbContext.SaveChangesAsync();
        
        return NoContent();
    }


}