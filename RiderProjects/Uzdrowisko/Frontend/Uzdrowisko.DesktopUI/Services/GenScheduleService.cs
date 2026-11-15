using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Uzdrowisko.DesktopUI.Services.Abstractions;
using Uzdrowisko.DesktopUI.Services.Common;

namespace Uzdrowisko.DesktopUI.Services;

public class GenScheduleService : IGenSchedule
{
    private readonly HttpClient _httpClient;
    
    public GenScheduleService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    
    public async Task<(bool IsSuccess, string? ErrorMessage)> GenScheduleResultAsync(int patientId, CancellationToken ct = default)
    {
        _httpClient.EnsureAuthHeader();

        try
        {
            var response = await _httpClient.GetAsync($"api/GenSchedule/{patientId}", ct);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            
            var rawError = await response.Content.ReadAsStringAsync(ct);
            var cleanError = rawError.Trim('"');
            
            if (!string.IsNullOrWhiteSpace(cleanError))
            {
                return (false, cleanError);
            }

            return (false, $"Błąd serwera (kod HTTP: {(int)response.StatusCode}).");

        }
        catch (HttpRequestException)
        {
            return (false, "Nie można połączyć się z serwerem. Sprawdź połączenie lub upewnij się, że API działa.");
        }
        catch (OperationCanceledException)
        {
            return (false, "Operacja została anulowana lub przekroczono czas oczekiwania.");
        }
        catch (Exception ex)
        {
            return (false, $"Wystąpił nieoczekiwany błąd po stronie aplikacji: {ex.Message}");
        }
    }
}