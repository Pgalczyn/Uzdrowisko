using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Uzdrowisko.DesktopUI.Services.Abstractions;

public interface IGenSchedule
{
    Task<(bool IsSuccess, string? ErrorMessage)> GenScheduleResultAsync(int patientId, List<int> treatmentIds, CancellationToken ct = default);
}