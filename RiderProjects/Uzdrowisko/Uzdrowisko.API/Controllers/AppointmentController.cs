using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedStructures.Dtos.Appointment;
using Uzdrowisko.Infrastructure.DataBase.Context;

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


}