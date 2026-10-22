using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedStructures.Dtos.Treatment;
using Uzdrowisko.Infrastructure.DataBase.Context;
using Uzdrowisko.Infrastructure.DataBase.Entities;

namespace Uzdrowisko.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class TreatmentController: ControllerBase
{
    private readonly UzdrowiskoDbContext _dbContext;

    public TreatmentController(UzdrowiskoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    [Authorize(Roles = "Sekretarka,Admin")]
    public async Task<ActionResult<TreatmentResponseDto>> CreateTreatment(
        [FromBody] CreateTreatmentDto createTreatmentDto)
    {
        var treatmentExists =await _dbContext.Treatments.AnyAsync(t => t.Name == createTreatmentDto.Name);
        if (treatmentExists)
        {
            return Conflict($"Zabieg z nazwie {createTreatmentDto.Name} już istnieje");
        }


        var newTreatment = new Treatment
        {
            Name = createTreatmentDto.Name,
            Description = createTreatmentDto.Description,
            DurationMinutes = createTreatmentDto.DurationMinutes,
            Type = createTreatmentDto.Type
        };
        
        _dbContext.Treatments.Add(newTreatment);
        await _dbContext.SaveChangesAsync();

        var response = new TreatmentResponseDto
        {
            Id = newTreatment.Id,
            Name = newTreatment.Name,
            Description = newTreatment.Description,
            Type = newTreatment.Type
        };
        return CreatedAtAction(nameof(GetTreatmentById), new { id = newTreatment.Id }, response);

    }


    [HttpGet("{id}")]
    public async Task<ActionResult<TreatmentResponseDto>> GetTreatmentById(int id)
    {
        var treatment = await _dbContext.Treatments
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TreatmentResponseDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Type = t.Type
            })
            .FirstOrDefaultAsync();
        if (treatment == null)
        {
            return NotFound($"Zabieg o takim id: {id} nie istnieje");
        }
        return Ok(treatment);

    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchTreatment(int id, [FromBody] UpdateTreatmentDto treatmentDto)
    {
        var treatment = await _dbContext.Treatments.FirstOrDefaultAsync(t => t.Id == id);
        if (treatment == null)
        {
            return NotFound($"Zabieg o podanym id {id} nie istnieje");
        }

        if (treatmentDto.Type != null)
        {
            treatment.Type = treatmentDto.Type;
        }

        if (treatmentDto.Description != null)
        {
            treatment.Description = treatmentDto.Description;
        }

        if (treatmentDto.DurationMinutes != null)
        {
            treatment.DurationMinutes = treatmentDto.DurationMinutes.Value;
        }

        if (treatmentDto.Name != null)
        {
            treatment.Name = treatmentDto.Name;
        }
        
        await  _dbContext.SaveChangesAsync();
        return NoContent();
    }
}