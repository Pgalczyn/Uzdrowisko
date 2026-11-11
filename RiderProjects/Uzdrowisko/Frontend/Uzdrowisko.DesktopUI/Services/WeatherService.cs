using System.Net.Http;
using System;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SharedStructures.Dtos.Weather;
using Uzdrowisko.DesktopUI.Services.Abstractions;

namespace Uzdrowisko.DesktopUI.Services;

public class WeatherService: IWeatherService

{
    private readonly HttpClient  _httpClient;
    private readonly string _apiKey;
    
    public WeatherService(IHttpClientFactory httpClientFactory,IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient("WeatherApi");
        _apiKey = configuration["WeatherApi:ApiKey"] 
                  ?? throw new InvalidOperationException("Brak klucza WeatherApi:ApiKey w appsettings.json");
    }
    
    public async Task<WeatherRecord?> GetWeather(string location = "solec-zdroj", CancellationToken ct = default)
    {
        try
        {
            Console.WriteLine($"[DEBUG] BaseAddress: {_httpClient.BaseAddress} | Key: {_apiKey}");

            var relativeUrl = $"forecast.json?key={_apiKey}&q={location}&days=3&aqi=no&alerts=no&lang=pl";
            var weatherData = await _httpClient.GetFromJsonAsync<WeatherRecord>(relativeUrl, ct);

            return weatherData;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[WeatherService Error]: {ex}");
            return null;
        }

    }
}