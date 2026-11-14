using System;
using SharedStructures.Dtos.Menu;

namespace Uzdrowisko.DesktopUI.Models;

public class MenuItem
{
    public DateOnly Date { get; set; }
    public MealItem Breakfast { get; set; } = new();
    public MealItem Lunch { get; set; } = new();
    public MealItem Dinner { get; set; } = new();

    public int TotalCalories => Breakfast.Calories + Lunch.Calories + Dinner.Calories;
    
    public string TotalCaloriesDisplay => $"Łącznie: {TotalCalories} kcal";

    public string DateDisplay => Date.ToString("dd MMMM yyyy");

    public static MenuItem FromDto(MenuDto dto)
    {
        return new MenuItem
        {
            Date = dto.Date,
            Breakfast = MealItem.FromDto(dto.Breakfast),
            Lunch = MealItem.FromDto(dto.Lunch),
            Dinner = MealItem.FromDto(dto.Dinner)
        };
    }
}

