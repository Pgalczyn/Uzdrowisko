using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using SharedStructures.Dtos.Treatment;
using Uzdrowisko.DesktopUI.Models;
using Uzdrowisko.DesktopUI.Services;
using Uzdrowisko.DesktopUI.Services.Abstractions;
using Uzdrowisko.DesktopUI.Services.Common;

namespace Uzdrowisko.Services;

public class TreatmentService : ITreatmentService
{
    private readonly HttpClient _httpClient;

    public TreatmentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }



    public async Task<IReadOnlyList<TreatmentItem>> GetAllTreatmentsItemsAsync(CancellationToken ct = default)
    {
        _httpClient.EnsureAuthHeader();
        try
        {
            var dtos = await _httpClient.GetFromJsonAsync<List<TreatmentResponseDto>>("treatment", ct);
            if (dtos == null) return Array.Empty<TreatmentItem>();
            
            return dtos.Select(TreatmentItem.FromDto).ToList();
        }
        catch (HttpRequestException)
        {
            return Array.Empty<TreatmentItem>();
        }
    }

    public async Task<TreatmentItem?> GetTreatmentItemAsync(int id, CancellationToken ct = default)
    {
        _httpClient.EnsureAuthHeader();
        
        try{       
            var dto = await _httpClient.GetFromJsonAsync<TreatmentResponseDto>($"api/Treatment/{id}", ct);
            return dto is not null ? TreatmentItem.FromDto(dto) : null;
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

    public async Task<TreatmentItem?> CreateTreatmentItemAsync(CreateTreatmentDto dto, CancellationToken ct = default)
    {
        _httpClient.EnsureAuthHeader();
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/treatment", dto, ct);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var createdDto = await response.Content.ReadFromJsonAsync<TreatmentResponseDto>(cancellationToken: ct);

            return createdDto is not null ? TreatmentItem.FromDto(createdDto) : null;
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
    public async Task<bool> UpdateTreatmentItemAsync(int id, UpdateTreatmentDto dto, CancellationToken ct = default)
    {
        _httpClient.EnsureAuthHeader();

        try
        {
            var response = await _httpClient.PatchAsJsonAsync($"api/Treatment/{id}", dto, ct);
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

    public async Task<bool> DeleteTreatmentItemAsync(int id, CancellationToken ct = default)
    {
        _httpClient.EnsureAuthHeader();
        
        try
        {
            var response = await _httpClient.DeleteAsync($"api/treatment/{id}", ct);

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