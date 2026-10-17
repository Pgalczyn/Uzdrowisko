using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Uzdrowisko.DesktopUI.Models;

namespace Uzdrowisko.Services;

public interface ITreatmentService
{
    Task<IReadOnlyList<TreatmentItem>> GetForDayAsync(DateTime day, CancellationToken ct = default);
}