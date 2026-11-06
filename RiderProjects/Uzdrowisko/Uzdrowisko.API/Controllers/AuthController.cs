using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SharedStructures.Dtos.Auth;
using Uzdrowisko.Infrastructure.DataBase.Context;
using Uzdrowisko.Infrastructure.DataBase.Entities.Abstractions;
namespace Uzdrowisko.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UzdrowiskoDbContext _dbContext;
    private readonly IConfiguration _config;
    private readonly IPasswordService _passwordService;
    
    public AuthController(UzdrowiskoDbContext dbContext,IConfiguration config,IPasswordService passwordService)
    {
        _dbContext = dbContext;
        _config = config;
        _passwordService = passwordService;
    }

    [HttpPost]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        IHasPassword? user = null;
        var userId = 0;
        var userRole = string.Empty;
        var userFullName = string.Empty;
        
        var employee = await _dbContext.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Email == request.Email);
        if (employee != null)
        {
            user = employee;
            userId = employee.Id;
            userRole = employee.Role;
            userFullName = $"{employee.Name} {employee.Surname}";
                
        }
        else
        {
            var patient = await _dbContext.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Email == request.Email);
            if (patient != null)
            { 
                user = patient;
                userId = patient.Id;
                userRole = "Kuracjusz";
                userFullName = $"{patient.Name} {patient.Surname}";
                
            }
        }
        if (user == null)
        {
            return Unauthorized("Nieprawidłowy email lub hasło.");
        }

        var isPasswordValid = _passwordService.VerifyHashedPassword(user, request.Password);
        if (!isPasswordValid)
        {
            return Unauthorized("Nieprawidłowy email lub hasło.");
        }
        var (token, expirationDate) = GenerateJwtToken(userId, request.Email, userRole, userFullName);
        return Ok(new LoginResponseDto
        {
            Token = token,
            Expiration = expirationDate
        });

    }

    private (string Token, DateTime Expiration) GenerateJwtToken(int userId, string email, string role, string fullName)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim("FullName", fullName),
            new Claim(ClaimTypes.Role, role)
        };
    
        var secretKey = _config["Jwt:Key"]
                        ?? throw new InvalidOperationException("Brak klucza 'Jwt:Key' w pliku konfiguracyjnym.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    
        var expiresInMinutes = double.Parse(_config["Jwt:ExpiresInMinutes"] ?? "60");
        var expirationDate = DateTime.UtcNow.AddMinutes(expiresInMinutes);
    
        var tokenDescriptor = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expirationDate, 
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        return (tokenString, expirationDate);
    }
    
}