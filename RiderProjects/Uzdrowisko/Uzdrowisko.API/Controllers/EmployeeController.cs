using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedStructures.Dtos.Employee;
using SharedStructures.Dtos.Role;
using Uzdrowisko.Infrastructure.DataBase.Context;
using Uzdrowisko.Infrastructure.DataBase.Entities;
using Uzdrowisko.Infrastructure.DataBase.Entities.Abstractions;

namespace Uzdrowisko.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class EmployeeController: ControllerBase
{
    
    private readonly UzdrowiskoDbContext _dbContext;
    private readonly IPasswordService _passwordService;
    
    public EmployeeController(UzdrowiskoDbContext dbContext, IPasswordService passwordService)
        {
        _dbContext = dbContext;
        _passwordService = passwordService;
        }

    [HttpPost("register")]
    [Authorize(Roles = "Sekretarka,Admin")]
    public async Task<ActionResult<EmployeeResponseDto>> RegisterEmployee([FromBody] RegisterEmployeeDto  registerDto)
    {
        if (!string.IsNullOrWhiteSpace(registerDto.Email))
        {
            var emailTakenByPatient = await _dbContext.Patients.AnyAsync(p => p.Email == registerDto.Email);
            var emailTakenByEmployee = await _dbContext.Employees.AnyAsync(e => e.Email == registerDto.Email);

            if (emailTakenByPatient || emailTakenByEmployee)
            {
                return Conflict($"Adres e-mail '{registerDto.Email}' jest już zajęty w systemie.");
            }
        }
        
        var distinctRoleIds = registerDto.EmployeeRoleIds.Distinct().ToList();
        var roles = distinctRoleIds.Count > 0
            ? await _dbContext.Roles
                .Where(r => distinctRoleIds.Contains(r.Id) && r.IsActive)
                .ToListAsync()
            : new List<Roles>();

        if (roles.Count != distinctRoleIds.Count)
        {
            return BadRequest("Jedna lub więcej wybranych kwalifikacji nie istnieje lub jest nieaktywna.");
        }


        var newEmployee = new Employee
        {
            Name = registerDto.FirstName,
            Surname = registerDto.LastName,
            Email = registerDto.Email,
            PhoneNumber = registerDto.PhoneNumber,
            DateOfBirth = registerDto.DateOfBirth,
            PasswordHash = string.Empty,
            Role = registerDto.Role,
            Active = true,
            RolesCollection = roles

        };
        newEmployee.PasswordHash= _passwordService.HashPassword(newEmployee,registerDto.Password);

        _dbContext.Employees.Add(newEmployee);
        await _dbContext.SaveChangesAsync();

        var responseDto = new EmployeeResponseDto
        {
            Id = newEmployee.Id,
            Email = newEmployee.Email,
            Name = newEmployee.Name,
            Surname = newEmployee.Surname,
            PhoneNumber = newEmployee.PhoneNumber,
            CreatedAt = newEmployee.CreatedAt,
            Role = newEmployee.Role,
            Qualifications = roles.Select(r => new RoleDto
            {
                Id = r.Id,
                RoleName = r.RoleName
            }).ToList()
            
        };
        return CreatedAtAction(nameof(GetEmployeeById), new { id = responseDto.Id }, responseDto);
    }
    
    [HttpGet]
    public async Task<ActionResult<List<EmployeeResponseDto>>> GetAllEmployees()
    {
        var employee = await _dbContext.Employees
            .AsNoTracking()
            .Select(p => new EmployeeResponseDto()
            {
                Id = p.Id,
                Name = p.Name,
                Email = p.Email,
                Surname = p.Surname,
                PhoneNumber = p.PhoneNumber,
                CreatedAt = p.CreatedAt,
                Role = p.Role,
                Qualifications = p.RolesCollection
                    .Select(r => new RoleDto
                    {
                        Id = r.Id,
                        RoleName = r.RoleName
                    })
                    .ToList()
            })
            .ToListAsync();
        
        return Ok(employee);
    }
    
    
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeResponseDto>> GetEmployeeById(int id)
    {
        var employee = await _dbContext.Employees
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new EmployeeResponseDto()
            {
                Id = p.Id,
                Name = p.Name,
                Email = p.Email,
                Surname = p.Surname,
                PhoneNumber = p.PhoneNumber,
                CreatedAt = p.CreatedAt,
                Qualifications = p.RolesCollection
                .Select(r => new RoleDto
                {
                Id = r.Id,
                RoleName = r.RoleName
            })
            .ToList()
            })
            .FirstOrDefaultAsync();
        
        if (employee == null)
        {
            return NotFound($"Nie znaleziono pracownika o ID {id}.");
        }
        return Ok(employee);
    }
    
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Sekretarka,Admin")]
    public async Task<IActionResult> DeleteEmployeeById(int id)
    {
        var employee = await _dbContext.Employees.FindAsync(id);
        if (employee == null)
        {
            return NotFound($"Pracownik o podanym id {id} nie istnieje");
        }

        employee.Active = false;

        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchEmployee(int id, [FromBody] UpdateEmployeeDto patchDto)
    {
        var employee = await _dbContext.Employees.FirstOrDefaultAsync(p => p.Id == id); 
        if (employee == null)
        {
            return NotFound($"Pracownik o takim id {id} nie istnieje");
        }
        
        if (!string.IsNullOrWhiteSpace(patchDto.Email))
        {
            var emailTakenByPatient = await _dbContext.Patients.AnyAsync(p => p.Email == patchDto.Email && p.Id != id);
            var emailTakenByEmployee = await _dbContext.Employees.AnyAsync(e => e.Email == patchDto.Email);

            if (emailTakenByPatient || emailTakenByEmployee)
            {
                return Conflict($"Adres e-mail '{patchDto.Email}' jest już zajęty w systemie.");
            }
        }
        if (patchDto.Name != null)
            employee.Name = patchDto.Name;

        if (patchDto.Surname != null)
            employee.Surname = patchDto.Surname;

        if (patchDto.PhoneNumber != null)
            employee.PhoneNumber = patchDto.PhoneNumber;

        if (patchDto.Email != null)
            employee.Email = patchDto.Email;

        if (patchDto.Address != null)
            employee.Address = patchDto.Address;

        if (patchDto.City != null)
            employee.City = patchDto.City;

        if (patchDto.PostalCode != null)
            employee.PostalCode = patchDto.PostalCode;

        if (patchDto.DateOfBirth.HasValue)
            employee.DateOfBirth = patchDto.DateOfBirth.Value;

        if (patchDto.Active.HasValue)
            employee.Active = patchDto.Active.Value;
        
        if (patchDto.Role != null)
            employee.Role = patchDto.Role;

        if (patchDto.RoleIds != null)
        {
            var distinctRoleIds = patchDto.RoleIds.Distinct().ToList();
            var updatedRoles = distinctRoleIds.Count > 0
                ? await _dbContext.Roles
                    .Where(r => distinctRoleIds.Contains(r.Id) && r.IsActive)
                    .ToListAsync()
                : new List<Roles>();

            if (updatedRoles.Count != distinctRoleIds.Count)
            {
                return BadRequest("Jedna lub więcej wybranych kwalifikacji nie istnieje lub jest nieaktywna.");
            }
            
            employee.RolesCollection.Clear();
            foreach (var role in updatedRoles)
            {
                employee.RolesCollection.Add(role);
            }
        }

        employee.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("seed-initial-admin")]
    [AllowAnonymous]
    public async Task<IActionResult> SeedInitialAdmin()
    {
        const string adminEmail = "admin@agh.edu.pl";
        
        var exists = await _dbContext.Employees.AnyAsync(e => e.Email == adminEmail);
        if (exists)
        {
            return BadRequest("Konto początkowe już zostało wcześniej utworzone.");
        }
        
        var initialEmployee = new Employee
        {
            Name = "Anna",
            Surname = "Nowak",
            Email = adminEmail,
            Role = "Admin",
            DateOfBirth = new DateOnly(2000,10,1),
            PhoneNumber = "123456789",
            Active = true,
            PasswordHash = string.Empty,
            CreatedAt = DateTime.UtcNow
        };
        
        initialEmployee.PasswordHash = _passwordService.HashPassword(initialEmployee, "Sekretarka123!");
        
        _dbContext.Employees.Add(initialEmployee);
        await _dbContext.SaveChangesAsync();
        
        return Ok(new { message = $"Utworzono pracownika: {adminEmail}", role = initialEmployee.Role });
    }
    
}