using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uzdrowisko.Infrastructure.DataBase.Context;
using Uzdrowisko.Infrastructure.DataBase.Entities;
using Uzdrowisko.Infrastructure.DataBase.Entities.Abstractions;

namespace Uzdrowisko.API.DataSeeders;

public static class PatientsSeeder
{
    public static async Task SeedAsync(UzdrowiskoDbContext dbContext, ILogger logger,IPasswordService passwordHasher)
    {
        await SeedPatientsFromCsv(dbContext, logger, passwordHasher);
    }

    private static async Task SeedPatientsFromCsv(UzdrowiskoDbContext dbContext, ILogger logger,IPasswordService passwordHasher)
    {
        var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataCSV", "Patients.csv");
        if (!File.Exists(csvPath))
        {
            logger.LogWarning("Plik CSV z danymi kuracjuszy nie został odnaleziony pod ścieżką: {Path}", csvPath);
            return;
        }

        var lines = await File.ReadAllLinesAsync(csvPath);
        if (lines.Length <= 1)
        {
            logger.LogInformation("Plik Patients.csv jest pusty lub zawiera wyłącznie nagłówek.");
            return;
        }

        var parsedRecords = lines
            .Skip(1)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Split(';'))
            .Where(columns => columns.Length >= 7) 
            .Select(columns => new
            {
                Name = columns[0].Trim(),
                Surname = columns[1].Trim(),
                Pesel = columns[2].Trim(),
                DateOfBirth = DateOnly.TryParseExact(columns[3].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dob)
                    ? dob
                    : new DateOnly(1970, 1, 1),
                PhoneNumber = columns[4].Trim(),
                Email = columns[5].Trim(),
                Password = columns[6].Trim(),
            })
            .ToList();

        var peselsInCsv = parsedRecords.Select(r => r.Pesel).ToList();
        var existingPesels = await dbContext.Patients
            .Where(p => peselsInCsv.Contains(p.Pesel))
            .Select(p => p.Pesel)
            .ToListAsync();

        var patientsToAdd = new List<Patient>();

        foreach (var record in parsedRecords)
        {
            if (existingPesels.Contains(record.Pesel))
            {
                continue;
            }

            var patient = new Patient
            {
                Name = record.Name,
                Surname = record.Surname,
                Pesel = record.Pesel,
                PasswordHash = string.Empty,
                DateOfBirth = record.DateOfBirth,
                PhoneNumber = record.PhoneNumber,
                Email = record.Email,
                Active = true,
                CreatedAt = DateTime.UtcNow
            };
            
            patient.PasswordHash = passwordHasher.HashPassword(patient, record.Password);

            patientsToAdd.Add(patient);
        }
        
        if (patientsToAdd.Count > 0)
        {
            await dbContext.Patients.AddRangeAsync(patientsToAdd);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Pomyślnie zasilono bazę danych o {Count} kuracjuszy z pliku CSV.", patientsToAdd.Count);
        }
        else
        {
            logger.LogInformation("Wszyscy pacjenci z pliku CSV znajdują się już w bazie danych.");
        }
    }
}
