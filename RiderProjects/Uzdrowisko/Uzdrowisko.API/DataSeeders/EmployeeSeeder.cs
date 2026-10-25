using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Uzdrowisko.Infrastructure.DataBase.Context;
using Uzdrowisko.Infrastructure.DataBase.Entities;
using Uzdrowisko.Infrastructure.DataBase.Entities.Abstractions;

namespace Uzdrowisko.API.DataSeeders;

public static class EmployeeSeeder
{
    public static async Task SeedAsync(UzdrowiskoDbContext dbContext, ILogger logger,
        IPasswordService passwordService)
    {
        await SeedDefaultAdmin(dbContext, logger, passwordService);
        
        //await SeedDefaultEmployees(dbContext, logger, passwordService);
        await SeedEmployeesFromCsv(dbContext, logger, passwordService);
    }

    private static async Task SeedDefaultAdmin(UzdrowiskoDbContext dbContext, ILogger logger,
        IPasswordService passwordService)
    {
        const string emailDefaultAdmin = "admin@agh.edu.pl";
        if (await dbContext.Employees.AnyAsync(e => e.Email == emailDefaultAdmin))
        {
            return;
        }
        
        var defaultAdmin = new Employee
        {
            Name = "Anna",
            Surname = "Nowak",
            Email = emailDefaultAdmin,
            Role = "Admin",
            DateOfBirth = new DateOnly(2000,10,1),
            PhoneNumber = "123456789",
            Active = true,
            PasswordHash = string.Empty,
            CreatedAt = DateTime.UtcNow
        };
        
        defaultAdmin.PasswordHash = passwordService.HashPassword(defaultAdmin, "Sekretarka123!");
        
        dbContext.Employees.Add(defaultAdmin);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Konto początkowe {Email} zostało utworzone.", emailDefaultAdmin);
    }

    private static async Task SeedDefaultEmployees(UzdrowiskoDbContext dbContext, ILogger logger,
        IPasswordService passwordService)
    {
        var staffEmails = new[]
        {
            "lekarz@uzdrowisko.pl",
            "fizjoterapeuta@uzdrowisko.pl",
            "recepcja@uzdrowisko.pl"
        };

        var existingEmails = dbContext.Employees.Where(e => staffEmails.Contains(e.Email)).Select(e => e.Email).ToList();

        var employeesToAdd = new List<Employee>();
        
        
         if (!existingEmails.Contains("lekarz@uzdrowisko.pl"))
        {
            var doctor = new Employee
            {
                Name = "Marek",
                Surname = "Zieliński",
                Email = "lekarz@uzdrowisko.pl",
                Role = "Lekarz",
                PasswordHash = string.Empty,
                DateOfBirth = new DateOnly(1985, 3, 15),
                PhoneNumber = "500600701",
                Active = true,
                CreatedAt = DateTime.UtcNow
            };
            doctor.PasswordHash = passwordService.HashPassword(doctor, "Lekarz123!");
            employeesToAdd.Add(doctor);
        }

        if (!existingEmails.Contains("fizjoterapeuta@uzdrowisko.pl"))
        {
            var physio = new Employee
            {
                Name = "Tomasz",
                Surname = "Kowalski",
                Email = "fizjoterapeuta@uzdrowisko.pl",
                Role = "Fizjoterapeuta",
                PasswordHash = string.Empty,
                DateOfBirth = new DateOnly(1992, 7, 22),
                PhoneNumber = "500600702",
                Active = true,
                CreatedAt = DateTime.UtcNow
            };
            physio.PasswordHash = passwordService.HashPassword(physio, "Fizjo123!");
            employeesToAdd.Add(physio);
        }

        if (!existingEmails.Contains("recepcja@uzdrowisko.pl"))
        {
            var receptionist = new Employee
            {
                Name = "Katarzyna",
                Surname = "Wiśniewska",
                Email = "recepcja@uzdrowisko.pl",
                Role = "Sekretarka",
                PasswordHash = string.Empty,
                DateOfBirth = new DateOnly(1995, 11, 8),
                PhoneNumber = "500600703",
                Active = true,
                CreatedAt = DateTime.UtcNow
            };
            receptionist.PasswordHash = passwordService.HashPassword(receptionist, "Recepcja123!");
            employeesToAdd.Add(receptionist);
        }

        if (employeesToAdd.Count > 0)
        {
            await dbContext.Employees.AddRangeAsync(employeesToAdd);
            
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Transakcja 2 zatwierdzona: Dodano {Count} nowych pracowników.", employeesToAdd.Count);
        }
    }

    private static async Task SeedEmployeesFromCsv(UzdrowiskoDbContext dbContext, ILogger logger,
        IPasswordService passwordService)
    {
        var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataSeeders", "DataCSV", "Employees.csv");
        if (!File.Exists(csvPath))
        {
            logger.LogWarning("Plik CSV z danymi pracowników nie został odnaleziony pod ścieżką: {Path}", csvPath);
            return;
        }
        
        var lines = File.ReadAllLines(csvPath);
        if (lines.Length <= 1)
        {
            logger.LogInformation("Plik CSV jest pusty lub zawiera wyłącznie nagłówek.");
            return;
        }
        
        var parsedRecords = lines
            .Skip(1)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(l => l.Split(';'))
            .Where(columns => columns.Length > 2)
            .Select(columns => new
            {
                Name = columns[0].Trim(),
                Surname = columns[1].Trim(),
                Email = columns[2].Trim(),
                Role = columns[3].Trim(),
                DateOfBirth = DateOnly.ParseExact(columns[4].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                PhoneNumber = columns[5].Trim(),
                Password = columns[6].Trim()
            })
            .ToList();
        var emailsInFile = parsedRecords.Select(r => r.Email).ToList();
        var existingEmails = await dbContext.Employees
            .Where(e => emailsInFile.Contains(e.Email))
            .Select(e => e.Email)
            .ToListAsync();

        var employeesToAdd = new List<Employee>();
        foreach (var record in parsedRecords)
        {
            if (existingEmails.Contains(record.Email))
            {
                continue;
            }

            var newEmployee = new Employee
            {
                Name = record.Name,
                Surname = record.Surname,
                Email = record.Email,
                Role = record.Role,
                DateOfBirth = record.DateOfBirth,
                PhoneNumber = record.PhoneNumber,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = string.Empty
            };

            newEmployee.PasswordHash = passwordService.HashPassword(newEmployee, record.Password);
            employeesToAdd.Add(newEmployee);
        }

        if (employeesToAdd.Count > 0)
        {
            await dbContext.Employees.AddRangeAsync(employeesToAdd);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Zasilono bazę danych o {Count} pracowników z pliku CSV.", employeesToAdd.Count);
        }
    }
    
}