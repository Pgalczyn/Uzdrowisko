using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SharedStructures.Dtos.Menu;

namespace Uzdrowisko.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly IHttpClientFactory  _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly IConfiguration _configuration;

    public MenuController(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _configuration = configuration;
    }
    [HttpGet]
    public async Task<ActionResult<MenuDto>> GetMenu()
    {
        Console.WriteLine("\n---> [BACKEND] Ktoś właśnie zapytał o menu! <---");

        
        var today = DateOnly.FromDateTime(DateTime.Now);
        var cacheKey = $"Menu_{today:yyyy-MM-dd}";

        if (_memoryCache.TryGetValue(cacheKey, out MenuDto? cachedMenu) && cachedMenu is not null)
        {
            return Ok(cachedMenu);
        }
        
        try
        {
            var apiKey = _configuration["SpoonacularApiKey"];
            if (string.IsNullOrEmpty(apiKey)) return StatusCode(500, "Brak klucza Spoonacular w konfiguracji.");
            
            var spoonacularClient = _httpClientFactory.CreateClient("Spoonacular");
            var translatorClient = _httpClientFactory.CreateClient("Translator");
            
            
            var breakfastTask = GetTranslatedMeal(spoonacularClient, translatorClient, "breakfast", apiKey);
            var lunchTask = GetTranslatedMeal(spoonacularClient, translatorClient, "soup", apiKey);
            var dinnerTask = GetTranslatedMeal(spoonacularClient, translatorClient, "main course", apiKey);

            await Task.WhenAll(breakfastTask, lunchTask, dinnerTask);

            var menu = new MenuDto
            {
                Date = today,
                Breakfast = await breakfastTask,
                Lunch = await lunchTask,
                Dinner = await dinnerTask
            };

            var midnight = new DateTimeOffset(DateTime.Today.AddDays(1));
            _memoryCache.Set(cacheKey, menu, midnight);

            return Ok(menu);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n\n[KRYTYCZNY BŁĄD BACKENDU]: {ex.Message}\n{ex.StackTrace}\n\n");

            return StatusCode(500, "Wystąpił problem podczas łączenia z zewnętrznym API kulinarnym.");
        }
    }

    private static async Task<MealDto> GetTranslatedMeal(HttpClient spoonacularClient, HttpClient translatorClient,
        string mealType, 
        string apiKey)
    {
        var url = $"complexSearch?type={mealType}&number=1&addRecipeInformation=true&addRecipeNutrition=true&sort=random&apiKey={apiKey}";
        
        var jsonResponse = await spoonacularClient.GetStringAsync(url);
        using var doc = JsonDocument.Parse(jsonResponse);
        
        var resultsArray = doc.RootElement.GetProperty("results");
        
        var meal = resultsArray[0];

        var englishName = meal.GetProperty("title").GetString() ?? "Unknown";
        var imageUrl = meal.GetProperty("image").GetString() ?? string.Empty;
        var rawSummary = meal.GetProperty("summary").GetString() ?? string.Empty;
        
        var cleanSummary = Regex.Replace(rawSummary, "<.*?>", string.Empty);
        
        var shortSummary = cleanSummary.Length > 180 
            ? cleanSummary[..180].Trim() + "..." 
            : cleanSummary;
        
        int calories = 0;
        var nutrientsArray = meal.GetProperty("nutrition").GetProperty("nutrients");
        foreach (var nutrient in nutrientsArray.EnumerateArray())
        {
            if (nutrient.GetProperty("name").GetString() == "Calories")
            {
                calories = (int)nutrient.GetProperty("amount").GetDouble();
                break;
            }
        }
        
        var polishName = await TranslateToPolish(translatorClient, englishName);
        var polishDescription = await TranslateToPolish(translatorClient, shortSummary);

        return new MealDto
        {
            Name = polishName,
            Description = polishDescription,
            Calories = calories,
            ImageUrl = imageUrl
        };
    }
    
    private static async Task<string> TranslateToPolish(HttpClient client, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var encodedText = Uri.EscapeDataString(text);

        var response = await client.GetStringAsync($"get?q={encodedText}&langpair=en|pl");
        using var doc =  JsonDocument.Parse(response);

        return doc.RootElement
            .GetProperty("responseData")
            .GetProperty("translatedText")
            .GetString() ?? text;
    }
}