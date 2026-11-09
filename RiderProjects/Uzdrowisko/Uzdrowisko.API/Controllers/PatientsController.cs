using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedStructures.Dtos.Patient;
using Uzdrowisko.Infrastructure.DataBase.Context;
using Uzdrowisko.Infrastructure.DataBase.Entities;
using Uzdrowisko.Infrastructure.DataBase.Entities.Abstractions;

namespace Uzdrowisko.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly UzdrowiskoDbContext _dbContext;
    private readonly IPasswordService _passwordService;
    
    public PatientsController(UzdrowiskoDbContext dbContext, IPasswordService passwordService)
        {
        _dbContext = dbContext;
        _passwordService = passwordService;
        }

    [HttpPost("register")]
    [Authorize(Roles = "Sekretarka,Admin")]
    public async Task<ActionResult<PatientResponseDto>> Register([FromBody] CreatePatientDto dtoPatient)
    {
        var peselExists = await _dbContext.Patients.AnyAsync(p => p.Pesel == dtoPatient.Pesel);
        if (peselExists)
        {
            return Conflict($"Kuracjusz o numerze PESEL {dtoPatient.Pesel} jest już zarejestrowany w systemie.");
        }
        
        if (!string.IsNullOrWhiteSpace(dtoPatient.Email))
        {
            var emailTakenByPatient = await _dbContext.Patients.AnyAsync(p => p.Email == dtoPatient.Email);
            var emailTakenByEmployee = await _dbContext.Employees.AnyAsync(e => e.Email == dtoPatient.Email);

            if (emailTakenByPatient || emailTakenByEmployee)
            {
                return Conflict($"Adres e-mail '{dtoPatient.Email}' jest już zajęty w systemie.");
            }
        }

        var newPatient = new Patient
        {
            Pesel = dtoPatient.Pesel,
            Name = dtoPatient.Name,
            Surname = dtoPatient.Surname,
            DateOfBirth = dtoPatient.DateOfBirth,
            PasswordHash = string.Empty,
            PhoneNumber = dtoPatient.PhoneNumber,
            Email = dtoPatient.Email,
            Address = dtoPatient.Address,
            City = dtoPatient.City,
            PostalCode = dtoPatient.PostalCode,
            CreatedAt = DateTime.UtcNow
        };
        
        newPatient.PasswordHash =  _passwordService.HashPassword(newPatient,dtoPatient.PasswordRaw);
            
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
    [HttpGet]
    public async Task<ActionResult<List<PatientResponseDto>>> GetAllPatients()
    {
        var patient = await _dbContext.Patients
            .AsNoTracking()
            .Select(p => new PatientResponseDto
            {
                Id = p.Id,
                Pesel = p.Pesel,
                Name = p.Name,
                Surname = p.Surname,
                PhoneNumber = p.PhoneNumber,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
        
        return Ok(patient);
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
    
    
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Sekretarka,Admin")]
    public async Task<IActionResult> DeletePatientById(int id)
    {
        var patient = await _dbContext.Patients.FindAsync(id);
        if (patient == null)
        {
            return NotFound($"Pacjent o podanym id {id} nie istnieje");
        }

        patient.Active = false;

        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchPatient(int id,[FromBody] UpdatePatientDto patchDto)
    {
        var patient = await _dbContext.Patients.FirstOrDefaultAsync(p => p.Id == id); 
        if (patient == null)
        {
            return NotFound($"Pacjent o takim id {id} nie istnieje");
        }
        

        
        if (patchDto.Name != null)
            patient.Name = patchDto.Name;

        if (patchDto.Surname != null)
            patient.Surname = patchDto.Surname;

        if (patchDto.PhoneNumber != null)
            patient.PhoneNumber = patchDto.PhoneNumber;

        if (patchDto.Email != null)
            patient.Email = patchDto.Email;

        if (patchDto.Address != null)
            patient.Address = patchDto.Address;

        if (patchDto.City != null)
            patient.City = patchDto.City;

        if (patchDto.PostalCode != null)
            patient.PostalCode = patchDto.PostalCode;

        if (patchDto.DateOfBirth.HasValue)
            patient.DateOfBirth = patchDto.DateOfBirth.Value;

        if (patchDto.Active.HasValue)
            patient.Active = patchDto.Active.Value;

        patient.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
    
}