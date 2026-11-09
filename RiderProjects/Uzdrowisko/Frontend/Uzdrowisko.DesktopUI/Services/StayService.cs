using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using SharedStructures.Dtos.Stay;
using Uzdrowisko.DesktopUI.Models;
using Uzdrowisko.DesktopUI.Services.Abstractions;
using Uzdrowisko.DesktopUI.Services.Common;

namespace Uzdrowisko.DesktopUI.Services;

public class StayService : IStayService
{
    private readonly HttpClient _httpClient;
    public StayService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<IReadOnlyList<StayItem>> GetAllStays()
    {
        _httpClient.EnsureAuthHeader();
        
        try
        {
            var dtos = await _httpClient.GetFromJsonAsync<List<StayResponseDto>>("stay");
            
            if (dtos == null) return Array.Empty<StayItem>();
            
            return dtos.Select(StayItem.FromDto).ToList();
        }
        catch (HttpRequestException)
        {
           return Array.Empty<StayItem>();
        }
    }

    public async Task<StayItem?> GetStay(int id, CancellationToken ct)
    {
        _httpClient.EnsureAuthHeader();
        
        try{       
            var dto = await _httpClient.GetFromJsonAsync<StayResponseDto>($"api/stay/{id}", ct);
            return dto is not null ? StayItem.FromDto(dto) : null;
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

    public async Task<bool> UpdateStay(int id, UpdateStayDto dto, CancellationToken ct)
    {
        _httpClient.EnsureAuthHeader();
        
        try
        {
            var response = await _httpClient.PatchAsJsonAsync($"api/stay/{id}", dto, ct);
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