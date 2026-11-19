using System.Threading;
using System.Threading.Tasks;
using Uzdrowisko.DesktopUI.Models;

namespace Uzdrowisko.DesktopUI.Services.Abstractions;

public interface IMenuService
{
    Task<MenuItem?> GetMenu(CancellationToken ct = default);
}