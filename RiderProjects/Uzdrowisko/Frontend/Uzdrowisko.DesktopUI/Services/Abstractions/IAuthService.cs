using System.Threading.Tasks;

namespace Uzdrowisko.DesktopUI.Services.Abstractions;

public interface IAuthService
{
    Task<(bool Success, string? ErrorMessage)> LoginAsync(string email, string password);
}