using System;
using System.Text;
using System.Text.Json;

namespace Uzdrowisko.DesktopUI.Services;

public static class UserSession
{
    public static string? Token { get; set; }
    public static bool IsLoggedIn => !string.IsNullOrEmpty(Token);

    private static string GetPayload()
    {
        if (string.IsNullOrWhiteSpace(Token)) return string.Empty;
        var parts = Token.Split('.');
        if (parts.Length < 2) return string.Empty;

        var base64 = parts[1].Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        }
        catch
        {
            return string.Empty;
        }
    }

    public static bool IsEmployee => IsLoggedIn && !GetPayload().Contains("Kuracjusz", StringComparison.OrdinalIgnoreCase);
    public static bool IsAdmin => IsLoggedIn && GetPayload().Contains("Admin", StringComparison.OrdinalIgnoreCase);

    public static int UserId
    {
        get
        {
            var idStr = TryGetClaim("sub") 
                        ?? TryGetClaim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            return int.TryParse(idStr, out var id) ? id : 0;
        }
    }

    public static string FullName => TryGetClaim("FullName") ?? string.Empty;

    private static string? TryGetClaim(string key)
    {
        var payload = GetPayload();
        if (string.IsNullOrEmpty(payload)) return null;
        try
        {
            using var doc = JsonDocument.Parse(payload);
            return doc.RootElement.TryGetProperty(key, out var prop) ? prop.ToString() : null;
        }
        catch
        {
            return null;
        }
    }

    public static void Clear() => Token = null;
}