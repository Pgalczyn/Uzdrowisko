using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Uzdrowisko.DesktopUI.Models;

namespace Uzdrowisko.Services;

public class FakeTreatmentService : ITreatmentService
{
    public async Task<IReadOnlyList<TreatmentItem>> GetForDayAsync(DateTime day, CancellationToken ct = default)
    {
        await Task.Delay(300, ct);

        if (day.Day % 3 == 0) return Array.Empty<TreatmentItem>();

        var d = day.Date;
        return new[]
        {
            new TreatmentItem(1, "Masaż klasyczny", "Jan Kowalski",  "Sala 3", d.AddHours(9),  TimeSpan.FromMinutes(45)),
            new TreatmentItem(2, "Kąpiel borowinowa","Anna Nowak",   "Sala 1", d.AddHours(10.5), TimeSpan.FromMinutes(30)),
            new TreatmentItem(3, "Krioterapia",      "Piotr Zieliński","Sala 5", d.AddHours(13), TimeSpan.FromMinutes(20)),
            
        };
    }
}