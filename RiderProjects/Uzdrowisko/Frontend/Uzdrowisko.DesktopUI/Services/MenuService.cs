using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using SharedStructures.Dtos.Menu;
using Uzdrowisko.DesktopUI.Models;
using Uzdrowisko.DesktopUI.Services.Abstractions;

namespace Uzdrowisko.DesktopUI.Services;

public class MenuService : IMenuService
{
    private readonly HttpClient _httpClient;

    public MenuService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<MenuItem?> GetMenu(CancellationToken ct = default)
    {
        try
        {

            var dto = await _httpClient.GetFromJsonAsync<MenuDto>("api/menu/", ct);
            
            if (dto == null) 
            {
                return null;
            }
            
            return MenuItem.FromDto(dto);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}