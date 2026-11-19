using Algorithm;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uzdrowisko.Infrastructure.DataBase.Context;
using Uzdrowisko.Infrastructure.DataBase.Entities;

namespace Uzdrowisko.API.Controllers;

public class GenerateScheduleDto
{
    public int PatientId { get; set; }
    public List<int> TreatmentIds { get; set; } = new();
}

[ApiController]
[Route("api/[controller]")]
public class GenScheduleController : ControllerBase
{
    private readonly UzdrowiskoDbContext _dbContext;
    public GenScheduleController(UzdrowiskoDbContext dbContext) => _dbContext = dbContext;

    [HttpPost]
    public async Task<IActionResult> GenerateSchedule([FromBody] GenerateScheduleDto dto)
    {
        var patient = await _dbContext.Patients.FindAsync(dto.PatientId);
        if (patient == null)
        {
            return NotFound("Pacjent o takim id nie istnieje");
        }

        var stay = await _dbContext.Stays.FirstOrDefaultAsync(s => s.PatientId == dto.PatientId);
        if (stay == null)
        {
            return NotFound("Nie ma pobytu dla danego pacjenta");
        }

        var fromDate = stay.DateFrom;
        var toDate = stay.DateTo;

        var appointments = await _dbContext.Appointments
            .Include(a => a.Schedule)
            .ThenInclude(s => s!.Stay)
            .Include(a => a.Treatment)
            .Include(a => a.Employee)
            .Include(a => a.Room)
            .Where(a => a.Date >= fromDate
                        && a.Date <= toDate
                        && a.Schedule != null
                        && a.Schedule.Stay != null
                        && a.Room != null
                        && a.Employee != null
                        && a.Treatment != null)
            .OrderBy(a => a.Date)
            .ThenBy(a => a.TimeFrom)
            .ToListAsync();
        
        var scheduler = new Scheduler(15, 20, 480, 960);

        List<AppointmentLocal> localAppointments = appointments
            .Select(a => new AppointmentLocal(scheduler, a))
            .ToList();

        var employees = await _dbContext.Employees.AsNoTracking().ToListAsync();
        var rooms = await _dbContext.Rooms
            .Include(r => r.Treatment)
            .Where(r => r.Treatment != null)
            .AsNoTracking()
            .ToListAsync();
        var selectedTreatmentsDb = await _dbContext.Treatments
            .AsNoTracking()
            .Where(t => dto.TreatmentIds.Contains(t.Id) && !string.IsNullOrEmpty(t.Flag))
            .ToListAsync();

        var patientSchedule = await _dbContext.Schedules
            .Include(sch => sch.Stay)
            .Where(sch => sch.Stay != null && sch.Stay.PatientId == dto.PatientId && sch.Stay.Active == true)
            .FirstOrDefaultAsync();

        if (patientSchedule == null)
        {
            return NotFound("Brak schedule");
        }

        if (!employees.Any() || !rooms.Any() || !selectedTreatmentsDb.Any())
        {
            return BadRequest("Brak wystarczających danych w bazie (pracowników, gabinetów lub zabiegów) do wygenerowania planu.");
        }

        var blocksTaken = new List<BlockedTime>();

        var newAppointments = scheduler.schedule(
            localAppointments,
            rooms,
            employees,
            selectedTreatmentsDb,
            patientSchedule,
            blocksTaken,
            null);

        if (newAppointments == null)
        {
            return BadRequest($"Nie udało się wygenerować planu: {scheduler.failureReason}");
        }

        List<Appointment> resultAppointments = newAppointments
            .Select(app => app.ConvertToAppointment())
            .ToList();

        await _dbContext.Appointments.AddRangeAsync(resultAppointments);
        await _dbContext.SaveChangesAsync();

        var response = resultAppointments.Select(a => new
        {
            a.Id,
            a.ScheduleId,
            a.TreatmentId,
            a.EmployeeId,
            a.RoomId,
            a.Date,
            a.TimeFrom,
            a.TimeTo,
            a.Status,
            a.Notes
        }).ToList();

        return Ok(response);
    }
}