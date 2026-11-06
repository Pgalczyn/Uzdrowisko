using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SharedStructures.Dtos.Stay;
using Uzdrowisko.DesktopUI.Models;

namespace Uzdrowisko.DesktopUI.Services.Abstractions;

public interface IStayService
{
    Task<IReadOnlyList<StayItem>> GetAllStays();
    Task<StayItem?> GetStay(int id, CancellationToken ct);
    Task<bool> UpdateStay(int id, UpdateStayDto dto, CancellationToken ct);
}