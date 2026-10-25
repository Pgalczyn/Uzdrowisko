using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Uzdrowisko.DesktopUI.Models;

namespace Uzdrowisko.DesktopUI.Services.Abstractions;

public interface IAppointmentService
{
    Task<IReadOnlyList<AppointmentItem>> GetAllAppointments();
    Task<AppointmentItem?> GetAppointmentAsync(int id, CancellationToken ct);
}