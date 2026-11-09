using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Uzdrowisko.Infrastructure.DataBase.Context;
using Uzdrowisko.Infrastructure.DataBase.Entities;

namespace Uzdrowisko.API.DataSeeders;

public static class StaysSeeder
{
    public static async Task SeedAsync(UzdrowiskoDbContext dbContext, ILogger logger)
    {
        var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataSeeders", "DataCSV", "Stays.csv");
        if (!File.Exists(csvPath))
        {
            logger.LogWarning("Plik CSV z pobytami nie został odnaleziony pod ścieżką: {Path}", csvPath);
            return;
        }

        var lines = await File.ReadAllLinesAsync(csvPath);
        if (lines.Length <= 1)
        {
            logger.LogInformation("Plik Stays.csv jest pusty lub zawiera wyłącznie nagłówek.");
            return;
        }

        var records = new List<CsvRecord>();
        foreach (var line in lines.Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)))
        {
            var c = line.Split(';');
            if (c.Length < 3
                || !DateOnly.TryParseExact(c[1].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateFrom)
                || !DateOnly.TryParseExact(c[2].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTo))
            {
                logger.LogWarning("Pominięto nieprawidłowy wiersz w Stays.csv: {Line}", line);
                continue;
            }

            records.Add(new CsvRecord(
                c[0].Trim(),
                dateFrom,
                dateTo,
                c.Length > 3 && !string.IsNullOrWhiteSpace(c[3]) ? c[3].Trim() : null));
        }

        if (records.Count == 0)
        {
            logger.LogInformation("Brak poprawnych rekordów w Stays.csv.");
            return;
        }

        var pesels = records.Select(r => r.PatientPesel).Distinct().ToList();
        var patients = await dbContext.Patients
            .Where(p => pesels.Contains(p.Pesel))
            .ToDictionaryAsync(p => p.Pesel);

        var existingStays = await dbContext.Stays
            .Where(s => s.Active)
            .Select(s => new { s.PatientId, s.DateFrom, s.DateTo })
            .ToListAsync();

        var existingSet = existingStays.Select(s => (s.PatientId, s.DateFrom, s.DateTo)).ToHashSet();

        var toAdd = new List<Stay>();

        foreach (var r in records)
        {
            if (!patients.TryGetValue(r.PatientPesel, out var patient))
            {
                logger.LogWarning("Nie znaleziono kuracjusza o PESEL {Pesel}. Pominięto wpis.", r.PatientPesel);
                continue;
            }

            if (!existingSet.Add((patient.Id, r.DateFrom, r.DateTo)))
            {
                continue;
            }

            toAdd.Add(new Stay
            {
                PatientId = patient.Id,
                DateFrom = r.DateFrom,
                DateTo = r.DateTo,
                Status = r.Status,
                Active = true
            });
        }

        if (toAdd.Count > 0)
        {
            await dbContext.Stays.AddRangeAsync(toAdd);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Pomyślnie zasilono bazę danych o {Count} pobytów z pliku CSV.", toAdd.Count);
        }
        else
        {
            logger.LogInformation("Wszystkie pobyty z pliku CSV znajdują się już w bazie danych.");
        }
    }

    private sealed record CsvRecord(
        string PatientPesel,
        DateOnly DateFrom,
        DateOnly DateTo,
        string? Status);
}