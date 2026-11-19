using System.Text.Json.Serialization;

namespace SharedStructures.Dtos.Weather;

public record WeatherRecord(
    [property: JsonPropertyName("location")] WeatherLocation Location,
    [property: JsonPropertyName("current")] CurrentWeather Current,
    [property: JsonPropertyName("forecast")] ForecastContainer Forecast
);

public record WeatherLocation(
    [property: JsonPropertyName("name")] string Name
);

public record CurrentWeather(
    [property: JsonPropertyName("temp_c")] double TempC,
    [property: JsonPropertyName("condition")] WeatherCondition Condition
);

public record ForecastContainer(
    [property: JsonPropertyName("forecastday")] IReadOnlyList<ForecastDayItem> Days
);

public record ForecastDayItem(
    [property: JsonPropertyName("date")] string Date,
    [property: JsonPropertyName("day")] DaySummary Day
);

public record DaySummary(
    [property: JsonPropertyName("maxtemp_c")] double MaxTempC,
    [property: JsonPropertyName("mintemp_c")] double MinTempC,
    [property: JsonPropertyName("daily_chance_of_rain")] int DailyChanceOfRain,
    [property: JsonPropertyName("condition")] WeatherCondition Condition
);

public record WeatherCondition(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("icon")] string IconUrl
);
