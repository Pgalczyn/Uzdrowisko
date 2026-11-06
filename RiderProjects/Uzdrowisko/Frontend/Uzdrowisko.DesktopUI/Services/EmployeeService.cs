using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using SharedStructures.Dtos.Employee;
using Uzdrowisko.DesktopUI.Models;
using Uzdrowisko.DesktopUI.Services.Abstractions;
using Uzdrowisko.DesktopUI.Services.Common;

namespace Uzdrowisko.DesktopUI.Services;

public class EmployeeService : IEmployeeService
{
    private readonly HttpClient _httpClient;

    public EmployeeService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    public async Task<IReadOnlyList<EmployeeItem>> GetAllEmployeesAsync(CancellationToken ct)
    {
        _httpClient.EnsureAuthHeader();
        
        try
        {
            var dtos = await _httpClient.GetFromJsonAsync<List<EmployeeResponseDto>>($"api/employee", ct);
            if (dtos == null) return Array.Empty<EmployeeItem>();
            
            return dtos.Select(EmployeeItem.FromDto).ToList();
        }
        catch (HttpRequestException)
        {
            return Array.Empty<EmployeeItem>();
        }

    }

    public async Task<EmployeeItem?> GetEmployeeItemAsync(int id, CancellationToken ct)
    {
        _httpClient.EnsureAuthHeader();
        
        try
        {
            var dto = await _httpClient.GetFromJsonAsync<EmployeeResponseDto>($"api/employee/{id}", ct);
            
            return dto is not null ? EmployeeItem.FromDto(dto) : null;

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

    public async Task<EmployeeItem?> CreateEmployeeItemAsync(RegisterEmployeeDto dto, CancellationToken ct)
    {
        _httpClient.EnsureAuthHeader();
        
        try
        {
            var response =await _httpClient.PostAsJsonAsync("api/employee",dto, ct);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var createdDto = await response.Content.ReadFromJsonAsync<EmployeeResponseDto>(cancellationToken: ct);

            return createdDto is not null ? EmployeeItem.FromDto(createdDto) : null;
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

    public  async Task<bool>  UpdateEmployeeItemAsync(int id,UpdateEmployeeDto dto, CancellationToken ct)
    {
        _httpClient.EnsureAuthHeader();
        
        try
        {
            var response = await _httpClient.PatchAsJsonAsync($"api/employee/{id}", dto, ct);
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

    public async Task<bool> DeleteEmployeeItemAsync(int id, CancellationToken ct)
    {
        _httpClient.EnsureAuthHeader();
        try
        {
            var response = await _httpClient.DeleteAsync($"api/employee/{id}", ct);
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