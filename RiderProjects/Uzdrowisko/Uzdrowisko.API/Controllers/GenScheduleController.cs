using Algorithm;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uzdrowisko.Infrastructure.DataBase.Context;
using Uzdrowisko.Infrastructure.DataBase.Entities;

namespace Uzdrowisko.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class GenScheduleController: ControllerBase
{
    private readonly UzdrowiskoDbContext _dbContext;
    public GenScheduleController(UzdrowiskoDbContext dbContext) => _dbContext = dbContext;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetScheduleByEmployeeId(int id)
    {
        var patient =await _dbContext.Patients.FindAsync(id);
        if (patient == null)
        {
            return NotFound("Pacjent o takim id nie istnieje");
        }
        
        var stay =await _dbContext.Stays.FirstOrDefaultAsync(s =>  s.PatientId == id);

        if (stay == null)
        {
            return NotFound($"Nie ma pobytu dla danego pacjenta");
        }

        var fromDate = stay.DateFrom;
        var toDate = stay.DateTo;

        var appointments = await _dbContext.Appointments
            .Include(a => a.Schedule)
            .Include(a => a.Treatment)
            .Include(a => a.Employee)
            .Include(a => a.Room)
            .Where(a => a.Date >= fromDate && a.Date <= toDate)
            .OrderBy(a => a.Date)
            .ThenBy(a => a.TimeFrom)
            .ToListAsync();

        var scheduler = new Scheduler(15,20,480,960);
        
        var employees = await _dbContext.Employees.AsNoTracking().ToListAsync();
        var rooms = await _dbContext.Rooms.AsNoTracking().ToListAsync();
        var randomTreatmentsDb = await _dbContext.Treatments
            .AsNoTracking()
            .OrderBy(t => EF.Functions.Random())
            .Take(10)
            .ToListAsync();
        var patientSchedule = await _dbContext.Schedules
            .Where(sch => sch.Stay.PatientId == id && sch.Stay.Active == true)
            .FirstOrDefaultAsync();

        if (patientSchedule == null)
        {
            return NotFound("Brak schedule");
        }
        
        if (!employees.Any() || !rooms.Any() || !randomTreatmentsDb.Any())
        {
            return BadRequest("Brak wystarczających danych w bazie (pracowników, gabinetów lub zabiegów) do wygenerowania planu.");
        }
        
        var blocksTaken = new List<BlockedTime>();
        
        var newAppointments =scheduler.schedule(appointments,rooms,employees,randomTreatmentsDb,patientSchedule,blocksTaken,null);
        
        await _dbContext.Appointments.AddRangeAsync(newAppointments);
        await _dbContext.SaveChangesAsync();
        
        return Ok(newAppointments);
    }
    
}