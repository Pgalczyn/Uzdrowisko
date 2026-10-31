using System.Net.Http;
using System.Net.Http.Headers;
using Uzdrowisko.DesktopUI.Services;

namespace Uzdrowisko.DesktopUI.Services.Common;

public static class HttpClientExtensions
{
    public static void EnsureAuthHeader(this HttpClient httpClient)
    {
        if (!string.IsNullOrWhiteSpace(UserSession.Token))
        {
            httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", UserSession.Token);
        }
    }
}