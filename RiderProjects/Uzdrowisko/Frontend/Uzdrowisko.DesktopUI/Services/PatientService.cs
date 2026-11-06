using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using SharedStructures.Dtos.Patient;
using Uzdrowisko.DesktopUI.Models;
using Uzdrowisko.DesktopUI.Services.Abstractions;
using Uzdrowisko.DesktopUI.Services.Common;

namespace Uzdrowisko.DesktopUI.Services;

public class PatientService : IPatientService
{
    private readonly HttpClient _httpClient;
    
    public PatientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<IReadOnlyList<PatientItem>> GetAllPatientsAsync(CancellationToken ct)
    {
        _httpClient.EnsureAuthHeader();
        
        try
        {
            var dtos = await _httpClient.GetFromJsonAsync<List<PatientResponseDto>>($"api/patients", ct);
            if (dtos == null) return Array.Empty<PatientItem>();
            
            return dtos.Select(PatientItem.FromDto).ToList();
        }
        catch (HttpRequestException)
        {
            return Array.Empty<PatientItem>();
        }

    }

    public async Task<PatientItem?> GetPatientItemAsync(int id, CancellationToken ct)
    {
        _httpClient.EnsureAuthHeader();
        
        try
        {
            var dto = await _httpClient.GetFromJsonAsync<PatientResponseDto>($"api/patients/register{id}", ct);
            
            return dto is not null ? PatientItem.FromDto(dto) : null;

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

    public async Task<PatientItem?> CreatePatientItemAsync(CreatePatientDto dto, CancellationToken ct)
    {
        _httpClient.EnsureAuthHeader();
        
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/patients/register", dto, ct);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var createdDto = await response.Content.ReadFromJsonAsync<PatientResponseDto>(cancellationToken: ct);

            return createdDto is not null ? PatientItem.FromDto(createdDto) : null;
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

    public async Task<bool> UpdatePatientItemAsync(int id, UpdatePatientDto dto, CancellationToken ct)
    {
        _httpClient.EnsureAuthHeader();
        try
        {
            var response = await _httpClient.PatchAsJsonAsync($"api/patients/{id}", dto, ct);
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

    public async Task<bool> DeletePatientItemAsync(int id, CancellationToken ct)
    {
        _httpClient.EnsureAuthHeader();
        try
        {
            var response = await _httpClient.DeleteAsync($"api/patients/{id}", ct);
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