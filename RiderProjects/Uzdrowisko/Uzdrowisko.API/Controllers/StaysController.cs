using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedStructures.Dtos.Stay;
using Uzdrowisko.Infrastructure.DataBase.Context;
using Uzdrowisko.Infrastructure.DataBase.Entities;

namespace Uzdrowisko.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class StaysController : ControllerBase
{
    private readonly UzdrowiskoDbContext _dbContext;
    
    public StaysController(UzdrowiskoDbContext dbContext)
        {
        _dbContext = dbContext;
        }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<StayResponseDto>> GetStayById(int id)
    {
        var stay = await _dbContext.Stays
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new StayResponseDto
            {
                Id = t.Id,
                PatientId = t.PatientId,
                PatientFullName = t.Patient.Name +  " " + t.Patient.Surname,
                PatientPesel = t.Patient.Pesel,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                Status = t.Status,
                Active = t.Active,
                
            })
            .FirstOrDefaultAsync();
        if (stay == null)
        {
            return NotFound($"Pobyt o takim id: {id} nie istnieje");
        }
        return Ok(stay);

    }
    
    
    [HttpGet]
    public async Task<ActionResult<List<StayResponseDto>>> GetAllStays()
    {
        var stay = await _dbContext.Stays
            .AsNoTracking()
            .Select(t => new StayResponseDto
            {
                Id = t.Id,
                PatientId = t.PatientId,
                PatientFullName = t.Patient.Name + " " + t.Patient.Surname,
                PatientPesel = t.Patient.Pesel,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                Status = t.Status,
                Active = t.Active,

            })
            .ToListAsync();

        return Ok(stay);

    }
    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchStay(int id, [FromBody] UpdateStayDto stayDto)
    {
        var stay = await _dbContext.Stays.FirstOrDefaultAsync(t => t.Id == id);
        if (stay == null)
        {
            return NotFound($"Pobyt o podanym id {id} nie istnieje");
        }

        if (stayDto.DateFrom != null)
        {
            stay.DateFrom = stayDto.DateFrom.Value;
        }

        if (stayDto.DateTo != null)
        {
            stay.DateTo = stayDto.DateTo.Value;
        }

        if (stayDto.Active != null)
        {
            stay.Active = stayDto.Active.Value;
        }

        if (stayDto.Status != null)
        {
            stay.Status = stayDto.Status;
        }
        
        await  _dbContext.SaveChangesAsync();
        return NoContent();
    }
    [HttpPost]
    public async Task<IActionResult> CreateStay([FromBody] CreateStayDto stayDto)
    {
        if (stayDto.DateTo < stayDto.DateFrom)
            return BadRequest("Data zakończenia nie może być wcześniejsza niż rozpoczęcia.");

        var patientExists = await _dbContext.Patients.AnyAsync(p => p.Id == stayDto.PatientId);
        if (!patientExists)
            return NotFound($"Pacjent o id {stayDto.PatientId} nie istnieje.");

        var stay = new Stay
        {
            PatientId = stayDto.PatientId,
            DateFrom = stayDto.DateFrom,
            DateTo = stayDto.DateTo,
            Status = "Aktywny",
            Active = true
        };

        _dbContext.Stays.Add(stay);
        await _dbContext.SaveChangesAsync();

        var schedule = new Schedule
        {
            StayId = stay.Id,
            Active = true
        };

        _dbContext.Schedules.Add(schedule);
        await _dbContext.SaveChangesAsync();

        return Ok(new { stay.Id, stay.PatientId, stay.DateFrom, stay.DateTo, ScheduleId = schedule.Id });
    }
    
}