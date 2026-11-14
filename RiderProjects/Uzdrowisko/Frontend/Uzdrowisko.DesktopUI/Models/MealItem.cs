using Avalonia.Media.Imaging;
using SharedStructures.Dtos.Menu;

public class MealItem
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int Calories { get; set; }

    public string CaloriesDisplay => $"{Calories} kcal";

    public Bitmap? Image { get; set; } 
    public static MealItem FromDto(MealDto dto)
    {
    
        return new MealItem
        {
            Name = dto.Name,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            Calories = dto.Calories
        };
    }
}