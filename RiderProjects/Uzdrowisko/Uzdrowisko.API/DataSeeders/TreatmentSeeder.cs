using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uzdrowisko.Infrastructure.DataBase.Context;
using Uzdrowisko.Infrastructure.DataBase.Entities;

namespace Uzdrowisko.API.DataSeeders;

public static class TreatmentSeeder
{
    public static async Task SeedAsync(UzdrowiskoDbContext dbContext, ILogger logger)
    {
        await SeedTreatmentsFromCsv(dbContext, logger);
    }
    
    private static async Task SeedTreatmentsFromCsv(UzdrowiskoDbContext dbContext, ILogger logger)
    {
        var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataSeeders", "DataCSV", "Treatments.csv");
        if (!File.Exists(csvPath))
        {
            logger.LogWarning("Plik CSV z danymi zabiegów nie został odnaleziony pod ścieżką: {Path}", csvPath);
            return;
        }
        
        var lines = await File.ReadAllLinesAsync(csvPath);
        if (lines.Length <= 1)
        {
            logger.LogInformation("Plik CSV z zabiegami jest pusty lub zawiera wyłącznie nagłówek.");
            return;
        }
        
        var parsedRecords = lines
            .Skip(1)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(l => l.Split(';'))
            .Where(columns => columns.Length >= 5)
            .Select(columns => new
            {
                Name = columns[0].Trim(),
                Description = columns[1].Trim(),
                DurationMinutes = int.TryParse(columns[2].Trim(), out var duration) ? duration : 30,
                Type = columns[3].Trim(),
                Flag = columns[4].Trim()
            })
            .ToList();

        var treatmentNamesInCsv = parsedRecords.Select(r => r.Name).ToList();
        var existingTreatmentNames = await dbContext.Treatments
            .Where(t => treatmentNamesInCsv.Contains(t.Name))
            .Select(t => t.Name)
            .ToListAsync();
        
        var treatmentsToAdd = new List<Treatment>();

        foreach (var record in parsedRecords)
        {
            if (existingTreatmentNames.Contains(record.Name))
            {
                continue;
            }

            var newTreatment = new Treatment
            {
                Name = record.Name,
                Description = record.Description,
                DurationMinutes = record.DurationMinutes,
                Type = record.Type,
                Flag = record.Flag
            };

            treatmentsToAdd.Add(newTreatment);
        }
        
        if (treatmentsToAdd.Count > 0)
        {
            await dbContext.Treatments.AddRangeAsync(treatmentsToAdd);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Zasilono bazę danych o {Count} zabiegów z pliku CSV.", treatmentsToAdd.Count);
        }
        else
        {
            logger.LogInformation("Wszystkie zabiegi z pliku CSV znajdują się już w bazie danych.");
        }
    }
}
