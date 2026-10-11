using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedStructures.Dtos.Patient;
using Uzdrowisko.Infrastructure.DataBase.Context;
using Uzdrowisko.Infrastructure.DataBase.Entities;

namespace Uzdrowisko.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly UzdrowiskoDbContext _dbContext;
    
    public PatientsController(UzdrowiskoDbContext dbContext)
        {
        _dbContext = dbContext;
        }

    [HttpPost("register")]
    public async Task<ActionResult<PatientResponseDto>> Register([FromBody] CreatePatientDto dtoPatient)
    {
        bool peselExists = await _dbContext.Patients.AnyAsync(p => p.Pesel == dtoPatient.Pesel);
        if (peselExists)
        {
            return Conflict($"Kuracjusz o numerze PESEL {dtoPatient.Pesel} jest już zarejestrowany w systemie.");
        }

        var newPatient = new Patient
        {
            Pesel = dtoPatient.Pesel,
            Name = dtoPatient.Name,
            Surname = dtoPatient.Surname,
            DateOfBirth = dtoPatient.DateOfBirth,
            PhoneNumber = dtoPatient.PhoneNumber,
            Email = dtoPatient.Email,
            Address = dtoPatient.Address,
            City = dtoPatient.City,
            PostalCode = dtoPatient.PostalCode,
            CreatedAt = DateTime.UtcNow
        };
            
        _dbContext.Patients.Add(newPatient);
        await _dbContext.SaveChangesAsync();
            
        var responseDto = new PatientResponseDto
            {
                Id = newPatient.Id,
                Pesel = newPatient.Pesel,
                Name = newPatient.Name,
                Surname = newPatient.Surname,
                PhoneNumber = newPatient.PhoneNumber,
                CreatedAt = newPatient.CreatedAt
            };
        return CreatedAtAction(nameof(GetPatientById), new { id = newPatient.Id }, responseDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PatientResponseDto>> GetPatientById(int id)
    {
        var patient = await _dbContext.Patients
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PatientResponseDto
            {
                Id = p.Id,
                Pesel = p.Pesel,
                Name = p.Name,
                Surname = p.Surname,
                PhoneNumber = p.PhoneNumber,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync();
        
        if (patient == null)
        {
            return NotFound($"Nie znaleziono kuracjusza o ID {id}.");
        }
        return Ok(patient);
    }
    
}