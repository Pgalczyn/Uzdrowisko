using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedStructures.Dtos.Stay;
using SharedStructures.Dtos.Treatment;
using Uzdrowisko.Infrastructure.DataBase.Context;

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
    
}