using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SharedStructures.Dtos.Room;
using Uzdrowisko.DesktopUI.Models;

namespace Uzdrowisko.DesktopUI.Services.Abstractions;

public interface IRoomService
{
    Task<IReadOnlyList<RoomItem>> GetAllRoomItemsAsync(CancellationToken ct);
    Task<RoomItem?> GetRoomItemAsync(int id, CancellationToken ct);
    Task<RoomItem?> CreateRoomItemAsync(CreateRoomDto  dto, CancellationToken ct);
    Task<bool> UpdateRoomItemAsync(int id, UpdateRoomDto dto, CancellationToken ct);
    Task<bool> DeleteRoomItemAsync(int id, CancellationToken ct);
}