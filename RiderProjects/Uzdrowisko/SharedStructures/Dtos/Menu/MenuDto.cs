namespace SharedStructures.Dtos.Menu;

public class MenuDto
{
    public DateOnly Date { get; set; }
    public MealDto Breakfast { get; set; } = new();
    public MealDto Lunch { get; set; } = new();
    public MealDto Dinner { get; set; } = new();
}