using System;
using System.Threading; 
using System.Threading.Tasks;
using SharedStructures.Dtos.Weather;

namespace Uzdrowisko.DesktopUI.Services.Abstractions;

public interface IWeatherService
{
    Task<WeatherRecord?> GetWeather(string location = "solec-zdroj",CancellationToken ct = default);
}