using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uzdrowisko.Infrastructure.DataBase.Context;
using Uzdrowisko.Infrastructure.DataBase.Entities;

namespace Uzdrowisko.API.DataSeeders;

public static class TreatmentSeeder
{
    private static readonly Regex SingleFlagRegex = new(@"^[0-2]_(G|S)_\d+$", RegexOptions.Compiled);

    public static async Task SeedAsync(UzdrowiskoDbContext dbContext, ILogger logger)
    {
        await SeedTreatmentsFromCsv(dbContext, logger);
    }

    private static string NormalizeFlag(string? rawFlag, string fallbackFlag = "0_S_0")
    {
        if (string.IsNullOrWhiteSpace(rawFlag))
        {
            return fallbackFlag;
        }

        var parts = rawFlag
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            return fallbackFlag;
        }

        var validFlags = parts.Where(p => SingleFlagRegex.IsMatch(p)).ToList();

        return validFlags.Count > 0 ? string.Join(";", validFlags) : fallbackFlag;
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
            .Where(columns => columns.Length >= 7)
            .Select(columns => new
            {
                Name = columns[0].Trim(),
                Description = columns[1].Trim(),
                DurationMinutes = int.TryParse(columns[2].Trim(), out var duration) ? duration : 30,
                Type = columns[3].Trim(),
                Flag = NormalizeFlag(columns[4].Trim()),
                Price = decimal.TryParse(columns[5].Trim(), out var price) ? price : 100,
                RequiredRole = columns[6].Trim()
            })
            .ToList();

        var parsedByName = parsedRecords
            .GroupBy(r => r.Name)
            .ToDictionary(g => g.Key, g => g.First());

        var existingTreatments = await dbContext.Treatments
            .Where(t => parsedByName.Keys.Contains(t.Name))
            .ToListAsync();

        var existingTreatmentNames = existingTreatments.Select(t => t.Name).ToHashSet();

        var rolesByName = await dbContext.Roles
            .ToDictionaryAsync(r => r.RoleName, r => r.Id);

        int updatedCount = 0;
        foreach (var existing in existingTreatments)
        {
            if (parsedByName.TryGetValue(existing.Name, out var csvRecord))
            {
                if (existing.Flag != csvRecord.Flag)
                {
                    existing.Flag = csvRecord.Flag;
                    updatedCount++;
                }
            }
        }

        var treatmentsToAdd = new List<Treatment>();

        foreach (var record in parsedRecords)
        {
            if (existingTreatmentNames.Contains(record.Name) || !rolesByName.TryGetValue(record.RequiredRole, out var roleId))
            {
                continue;
            }

            var newTreatment = new Treatment
            {
                Name = record.Name,
                Description = record.Description,
                DurationMinutes = record.DurationMinutes,
                Type = record.Type,
                Flag = record.Flag,
                Price = record.Price,
                Active = true,
                RequiredRoleId = roleId
            };

            treatmentsToAdd.Add(newTreatment);
        }

        if (treatmentsToAdd.Count > 0)
        {
            await dbContext.Treatments.AddRangeAsync(treatmentsToAdd);
        }

        if (treatmentsToAdd.Count > 0 || updatedCount > 0)
        {
            await dbContext.SaveChangesAsync();
        }

        if (treatmentsToAdd.Count > 0)
        {
            logger.LogInformation("Zasilono bazę danych o {Count} zabiegów z pliku CSV.", treatmentsToAdd.Count);
        }

        if (updatedCount > 0)
        {
            logger.LogInformation("Zaktualizowano flagi zależności dla {Count} istniejących zabiegów.", updatedCount);
        }

        if (treatmentsToAdd.Count == 0 && updatedCount == 0)
        {
            logger.LogInformation("Wszystkie zabiegi z pliku CSV znajdują się już w bazie danych i mają poprawne flagi.");
        }
    }
}