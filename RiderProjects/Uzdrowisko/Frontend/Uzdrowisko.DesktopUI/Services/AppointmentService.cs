using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using SharedStructures.Dtos.Appointment;
using SharedStructures.Dtos.Patient;
using Uzdrowisko.DesktopUI.Models;
using Uzdrowisko.DesktopUI.Services.Abstractions;
using Uzdrowisko.DesktopUI.Services.Common;

namespace Uzdrowisko.DesktopUI.Services;

public class AppointmentService : IAppointmentService
{
    private readonly HttpClient _httpClient;

    public AppointmentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<AppointmentItem>> GetAllAppointments()
    {
        _httpClient.EnsureAuthHeader();
        try
        {
            var dtos = await _httpClient.GetFromJsonAsync<List<AppointmentResponseDto>>("api/appointment");
            if (dtos == null) return Array.Empty<AppointmentItem>();
            
            return dtos.Select(AppointmentItem.FromDto).ToList();
        }
        catch (HttpRequestException)
        {
            return Array.Empty<AppointmentItem>();
        }
    }

    public async Task<AppointmentItem?> GetAppointmentAsync(int id, CancellationToken ct = default)
    {
        _httpClient.EnsureAuthHeader();

        try
        {
            var dto = await _httpClient.GetFromJsonAsync<AppointmentResponseDto>($"api/appointment/{id}", ct);
            return dto is not null ? AppointmentItem.FromDto(dto) : null;
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

    public async Task<IReadOnlyList<AppointmentItem>> GetForDayAsync(DateTime day, CancellationToken ct = default)
    {
        var all = await GetAllAppointments();
        var targetDate = DateOnly.FromDateTime(day);
        return all.Where(a => a.Date == targetDate).ToList();
    }
}
