using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using SharedStructures.Dtos.Auth;
using Uzdrowisko.DesktopUI.Services.Abstractions;

namespace Uzdrowisko.DesktopUI.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;

    public AuthService(HttpClient httpClient)
    {
        _httpClient =  httpClient;
    }

    public async Task<(bool Success, string? ErrorMessage)> LoginAsync(string email, string password)
    {
        try
        {
            var request = new LoginRequestDto
            {
                Email = email,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync("api/auth", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                if (result != null)
                {
                    UserSession.Token = result.Token;
                    return (true, null);
                }
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return (false, "Nieprawidłowy e-mail lub hasło.");
            }

            return (false, "Błąd serwera. Spróbuj ponownie później.");
        }
        catch (HttpRequestException)
        {
            return (false, "Nie można połączyć się z backendem. Upewnij się, że API jest uruchomione.");
        }
        catch (Exception ex)
        {
            return (false, $"Błąd: {ex.Message}");
        }
    }
}
