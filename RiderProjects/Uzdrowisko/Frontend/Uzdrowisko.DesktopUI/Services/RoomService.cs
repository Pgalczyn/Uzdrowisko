using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using SharedStructures.Dtos.Room;
using Uzdrowisko.DesktopUI.Models;
using Uzdrowisko.DesktopUI.Services.Abstractions;
using Uzdrowisko.DesktopUI.Services.Common;

namespace Uzdrowisko.DesktopUI.Services;

public class RoomService : IRoomService
{
    private readonly HttpClient _httpClient;
    public RoomService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<RoomItem>> GetAllRoomItemsAsync(CancellationToken ct)
    {
        _httpClient.EnsureAuthHeader();
        try
        {
            var dtos = await _httpClient.GetFromJsonAsync<List<RoomResponseDto>>($"api/Room", ct);
            if (dtos == null) return Array.Empty<RoomItem>();
            
            return dtos.Select(RoomItem.FromDto).ToList();
        }
        catch (HttpRequestException)
        {
            return Array.Empty<RoomItem>();
        }
        
    }

    public async Task<RoomItem?> GetRoomItemAsync(int id, CancellationToken ct)
    {
        _httpClient.EnsureAuthHeader();
        try
        {
            var dto = await _httpClient.GetFromJsonAsync<RoomResponseDto>($"api/Room{id}", ct);
            
            return dto is not null ? RoomItem.FromDto(dto) : null;

        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<RoomItem?> CreateRoomItemAsync(CreateRoomDto dto, CancellationToken ct)
    {
        _httpClient.EnsureAuthHeader();
        try
        {
            var response =await _httpClient.PostAsJsonAsync("api/room",dto, ct);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var createdDto = await response.Content.ReadFromJsonAsync<RoomResponseDto>(cancellationToken: ct);

            return createdDto is not null ? RoomItem.FromDto(createdDto) : null;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<bool> UpdateRoomItemAsync(int id, UpdateRoomDto dto, CancellationToken ct)
    {
        _httpClient.EnsureAuthHeader();

        try
        {
            var response = await _httpClient.PatchAsJsonAsync($"api/room/{id}", dto, ct);
            return response.IsSuccessStatusCode;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    public async Task<bool> DeleteRoomItemAsync(int id, CancellationToken ct)
    {
        _httpClient.EnsureAuthHeader();

        try
        {
            var response = await _httpClient.DeleteAsync($"api/room/{id}", ct);
            return response.IsSuccessStatusCode;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }
}