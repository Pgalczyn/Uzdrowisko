namespace Uzdrowisko.DesktopUI.Services;

public static class UserSession
{
    public static string? Token { get; set; }
    public static bool IsLoggedIn => !string.IsNullOrEmpty(Token);

    public static void Clear()
    {
        Token = null;
    }
}
