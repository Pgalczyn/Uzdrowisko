using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Uzdrowisko.Infrastructure.DataBase.Context;
using Uzdrowisko.Infrastructure.DataBase.Entities;

namespace Uzdrowisko.API.DataSeeders;

public static class AppointmentsSeeder
{
    private const string AppointmentStatus = "Zaplanowana";

    private const string? StayStatus = null;

    public static async Task SeedAsync(UzdrowiskoDbContext dbContext, ILogger logger, int weeks = 1)
    {
        var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataSeeders", "DataCSV", "Appointments.csv");
        if (!File.Exists(csvPath))
        {
            logger.LogWarning("Plik CSV z zabiegami nie został odnaleziony pod ścieżką: {Path}", csvPath);
            return;
        }

        var lines = await File.ReadAllLinesAsync(csvPath);
        if (lines.Length <= 1)
        {
            logger.LogInformation("Plik Appointments.csv jest pusty lub zawiera wyłącznie nagłówek.");
            return;
        }

        var records = new List<CsvRecord>();
        foreach (var line in lines.Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)))
        {
            var c = line.Split(';');
            if (c.Length < 6
                || !int.TryParse(c[4].Trim(), out var dayOfWeek) || dayOfWeek is < 0 or > 6
                || !TimeOnly.TryParseExact(c[5].Trim(), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var timeFrom))
            {
                logger.LogWarning("Pominięto nieprawidłowy wiersz w Appointments.csv: {Line}", line);
                continue;
            }

            records.Add(new CsvRecord(
                c[0].Trim(),
                c[1].Trim(),
                c[2].Trim(),
                c[3].Trim(),
                (DayOfWeek)dayOfWeek,
                timeFrom,
                c.Length > 6 && !string.IsNullOrWhiteSpace(c[6]) ? c[6].Trim() : null));
        }

        if (records.Count == 0)
        {
            logger.LogInformation("Brak poprawnych rekordów w Appointments.csv.");
            return;
        }

        var pesels = records.Select(r => r.PatientPesel).Distinct().ToList();
        var patients = await dbContext.Patients
            .Where(p => pesels.Contains(p.Pesel))
            .ToDictionaryAsync(p => p.Pesel);

        var employees = (await dbContext.Employees.ToListAsync())
            .GroupBy(e => e.Email)
            .ToDictionary(g => g.Key, g => g.First());

        var treatments = (await dbContext.Treatments.ToListAsync())
            .GroupBy(t => t.Name)
            .ToDictionary(g => g.Key, g => g.First());

        var rooms = (await dbContext.Rooms.ToListAsync())
            .GroupBy(r => r.Number)
            .ToDictionary(g => g.Key, g => g.First());

        var startDate = DateOnly.FromDateTime(DateTime.Today);
        var endDate = startDate.AddDays(Math.Max(weeks, 1) * 7 - 1);

        var patientIds = patients.Values.Select(p => p.Id).ToList();

        var stays = (await dbContext.Stays
                .Where(s => patientIds.Contains(s.PatientId) && s.Active)
                .ToListAsync())
            .GroupBy(s => s.PatientId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.DateTo).First());

        var newStays = new List<Stay>();
        foreach (var patient in patients.Values)
        {
            if (stays.ContainsKey(patient.Id))
            {
                continue;
            }

            var stay = new Stay
            {
                PatientId = patient.Id,
                DateFrom = startDate,
                DateTo = endDate,
                Status = StayStatus,
                Active = true
            };
            stays[patient.Id] = stay;
            newStays.Add(stay);
        }

        if (newStays.Count > 0)
        {
            await dbContext.Stays.AddRangeAsync(newStays);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Utworzono {Count} pobytów dla kuracjuszy.", newStays.Count);
        }

        var stayIds = stays.Values.Select(s => s.Id).ToList();
        var schedules = await dbContext.Schedules
            .Where(s => stayIds.Contains(s.StayId))
            .ToDictionaryAsync(s => s.StayId);

        var newSchedules = new List<Schedule>();
        foreach (var stay in stays.Values)
        {
            if (schedules.ContainsKey(stay.Id))
            {
                continue;
            }

            var schedule = new Schedule { StayId = stay.Id, Active = true };
            schedules[stay.Id] = schedule;
            newSchedules.Add(schedule);
        }

        if (newSchedules.Count > 0)
        {
            await dbContext.Schedules.AddRangeAsync(newSchedules);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Utworzono {Count} planów (Schedule).", newSchedules.Count);
        }

        var scheduleIds = schedules.Values.Select(s => s.Id).ToList();
        var existing = (await dbContext.Appointments
                .Where(a => scheduleIds.Contains(a.ScheduleId))
                .Select(a => new { a.ScheduleId, a.Date, a.TimeFrom })
                .ToListAsync())
            .Select(a => (a.ScheduleId, a.Date, a.TimeFrom))
            .ToHashSet();

        var byDay = records.GroupBy(r => r.DayOfWeek).ToDictionary(g => g.Key, g => g.ToList());
        var toAdd = new List<Appointment>();

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            if (!byDay.TryGetValue(date.DayOfWeek, out var dayRecords))
            {
                continue;
            }

            foreach (var r in dayRecords)
            {
                if (!patients.TryGetValue(r.PatientPesel, out var patient))
                {
                    logger.LogWarning("Nie znaleziono kuracjusza o PESEL {Pesel}. Pominięto wpis.", r.PatientPesel);
                    continue;
                }
                if (!treatments.TryGetValue(r.TreatmentName, out var treatment))
                {
                    logger.LogWarning("Nie znaleziono zabiegu '{Name}'. Pominięto wpis.", r.TreatmentName);
                    continue;
                }
                if (!employees.TryGetValue(r.EmployeeEmail, out var employee))
                {
                    logger.LogWarning("Nie znaleziono pracownika {Email}. Pominięto wpis.", r.EmployeeEmail);
                    continue;
                }
                if (!rooms.TryGetValue(r.RoomNumber, out var room))
                {
                    logger.LogWarning("Nie znaleziono pokoju o numerze {Number}. Pominięto wpis.", r.RoomNumber);
                    continue;
                }

                var stay = stays[patient.Id];
                if (date < stay.DateFrom || date > stay.DateTo)
                {
                    logger.LogWarning(
                        "Data {Date} wypada poza pobytem kuracjusza {Pesel} ({From} - {To}). Pominięto wpis.",
                        date, r.PatientPesel, stay.DateFrom, stay.DateTo);
                    continue;
                }

                var schedule = schedules[stay.Id];
                if (!existing.Add((schedule.Id, date, r.TimeFrom)))
                {
                    continue;
                }

                toAdd.Add(new Appointment
                {
                    ScheduleId = schedule.Id,
                    TreatmentId = treatment.Id,
                    EmployeeId = employee.Id,
                    RoomId = room.Id,
                    Date = date,
                    TimeFrom = r.TimeFrom,
                    TimeTo = r.TimeFrom.AddMinutes(treatment.DurationMinutes),
                    Status = AppointmentStatus,
                    Notes = r.Notes
                });
            }
        }

        if (toAdd.Count > 0)
        {
            await dbContext.Appointments.AddRangeAsync(toAdd);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Pomyślnie zasilono bazę danych o {Count} zabiegów z pliku CSV.", toAdd.Count);
        }
        else
        {
            logger.LogInformation("Wszystkie zabiegi z pliku CSV znajdują się już w bazie danych.");
        }
    }

    private sealed record CsvRecord(
        string PatientPesel,
        string TreatmentName,
        string EmployeeEmail,
        string RoomNumber,
        DayOfWeek DayOfWeek,
        TimeOnly TimeFrom,
        string? Notes);
}