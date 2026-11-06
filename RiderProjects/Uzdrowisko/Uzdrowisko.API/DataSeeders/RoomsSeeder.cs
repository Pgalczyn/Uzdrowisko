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

        var parsedRecords = lines
            .Skip(1)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Split(';'))
            .Where(columns => columns.Length >= 4)
            .Select(columns => new
            {
                Number = columns[0].Trim(),
                Name = string.IsNullOrWhiteSpace(columns[1]) ? null : columns[1].Trim(),
                Location = string.IsNullOrWhiteSpace(columns[2]) ? null : columns[2].Trim(),
                Capacity = int.TryParse(columns[3].Trim(), out var cap) ? cap : 1

            })
            .Where(r => !string.IsNullOrWhiteSpace(r.Number))
            .ToList();

        var numbersInCsv = parsedRecords.Select(r => r.Number).ToList();
        var existingNumbers = await dbContext.Rooms
            .Where(r => numbersInCsv.Contains(r.Number))
            .Select(r => r.Number)
            .ToListAsync();

        var roomsToAdd = new List<Room>();

        foreach (var record in parsedRecords)
        {
            if (existingNumbers.Contains(record.Number))
            {
                continue;
            }

            var room = new Room
            {
                Number = record.Number,
                Name = record.Name,
                Location = record.Location,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                Capacity = record.Capacity
            };

            roomsToAdd.Add(room);
        }

        if (roomsToAdd.Count > 0)
        {
            await dbContext.Rooms.AddRangeAsync(roomsToAdd);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Pomyślnie zasilono bazę danych o {Count} sal/gabinetów z pliku CSV.", roomsToAdd.Count);
        }
        else
        {
            logger.LogInformation("Wszystkie sale z pliku CSV znajdują się już w bazie danych.");
        }
    }
}
