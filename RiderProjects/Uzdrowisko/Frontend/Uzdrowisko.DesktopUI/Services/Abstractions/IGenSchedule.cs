using System;
using System.Threading;
using System.Threading.Tasks;

namespace Uzdrowisko.DesktopUI.Services.Abstractions;

public interface IGenSchedule
{
    Task<(bool IsSuccess, string? ErrorMessage)> GenScheduleResultAsync(int patientId, CancellationToken ct = default);
}