using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Uzdrowisko.Infrastructure.DataBase.Context;
using Uzdrowisko.Infrastructure.DataBase.Entities;

namespace Uzdrowisko.API.DataSeeders;

public static class RoomsSeeder
{
    public static async Task SeedAsync(UzdrowiskoDbContext dbContext, ILogger logger)
    {
        await SeedRoomsFromCsv(dbContext, logger);
    }

    private static async Task SeedRoomsFromCsv(UzdrowiskoDbContext dbContext, ILogger logger)
    {
        var csvPath = Path.Combine(AppContext.BaseDirectory, "DataSeeders", "DataCSV", "Rooms.csv");
        if (!File.Exists(csvPath))
        {
            logger.LogWarning("Plik CSV z danymi sal/gabinetów nie został odnaleziony pod ścieżką: {Path}", csvPath);
            return;
        }

        var lines = await File.ReadAllLinesAsync(csvPath);
        if (lines.Length <= 1)
        {
            logger.LogInformation("Plik Rooms.csv jest pusty lub zawiera wyłącznie nagłówek.");
            return;
        }

        var validTreatmentIds = await dbContext.Treatments
            .AsNoTracking()
            .OrderBy(t => t.Id)
            .Select(t => t.Id)
            .ToListAsync();

        if (validTreatmentIds.Count == 0)
        {
            logger.LogWarning("Brak zabiegów w bazie danych — nie można przypisać TreatmentId do sal.");
            return;
        }

        var validTreatmentIdSet = validTreatmentIds.ToHashSet();

        var parsedRecords = lines
            .Skip(1)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Split(';'))
            .Where(columns => columns.Length >= 5)
            .Select((columns, index) =>
            {
                int treatmentId = int.TryParse(columns[4].Trim(), out var parsedId) && validTreatmentIdSet.Contains(parsedId)
                    ? parsedId
                    : validTreatmentIds[index % validTreatmentIds.Count];

                return new
                {
                    Number = columns[0].Trim(),
                    Name = string.IsNullOrWhiteSpace(columns[1]) ? null : columns[1].Trim(),
                    Location = string.IsNullOrWhiteSpace(columns[2]) ? null : columns[2].Trim(),
                    Capacity = int.TryParse(columns[3].Trim(), out var cap) ? cap : 1,
                    TreatmentId = treatmentId
                };
            })
            .Where(r => !string.IsNullOrWhiteSpace(r.Number))
            .ToList();

        var parsedByNumber = parsedRecords
            .GroupBy(r => r.Number)
            .ToDictionary(g => g.Key, g => g.First());

        var existingRooms = await dbContext.Rooms
            .Where(r => parsedByNumber.Keys.Contains(r.Number))
            .ToListAsync();

        var existingNumbers = existingRooms.Select(r => r.Number).ToHashSet();

        int updatedCount = 0;
        foreach (var existingRoom in existingRooms)
        {
            if (existingRoom.TreatmentId == null && parsedByNumber.TryGetValue(existingRoom.Number, out var csvRecord))
            {
                existingRoom.TreatmentId = csvRecord.TreatmentId;
                existingRoom.UpdatedAt = DateTime.UtcNow;
                updatedCount++;
            }
        }

        var roomsToAdd = new List<Room>();

        foreach (var record in parsedRecords)
        {
            if (existingNumbers.Contains(record.Number))
            {
                continue;
            }

            roomsToAdd.Add(new Room
            {
                Number = record.Number,
                Name = record.Name,
                Location = record.Location,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                Capacity = record.Capacity,
                TreatmentId = record.TreatmentId
            });
        }

        if (roomsToAdd.Count > 0)
        {
            await dbContext.Rooms.AddRangeAsync(roomsToAdd);
        }

        if (roomsToAdd.Count > 0 || updatedCount > 0)
        {
            await dbContext.SaveChangesAsync();
        }

        if (roomsToAdd.Count > 0)
        {
            logger.LogInformation("Pomyślnie zasilono bazę danych o {Count} sal/gabinetów z pliku CSV.", roomsToAdd.Count);
        }

        if (updatedCount > 0)
        {
            logger.LogInformation("Zaktualizowano TreatmentId dla {Count} istniejących sal/gabinetów.", updatedCount);
        }

        if (roomsToAdd.Count == 0 && updatedCount == 0)
        {
            logger.LogInformation("Wszystkie sale z pliku CSV znajdują się już w bazie danych i mają przypisane zabiegi.");
        }
    }
}