using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models;
using CanteenReservationSystem.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CanteenReservationSystem.Tests;

public class NutritionServiceTests
{
    private static ApplicationDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ApplicationDbContext(options);
    }

    private static Dish CreateDish(string name)
    {
        return new Dish
        {
            DishName = name,
            Price = 5.00m,
            ImageUrl = "image.png",
            Characteristics = "Test",
            Description = "Test description",
            Category = new Category { CategoryName = "Main" },
            Nutrition = null,
            DishIngredients = new List<DishIngredient>(),
            DishAllergens = new List<DishAllergen>()
        };
    }

    [Fact]
    public async Task GetByDishIdAsync_ReturnsNutrition()
    {
        using var context = CreateContext("GetByDishIdAsync_ReturnsNutrition");
        var dish = CreateDish("DishA");
        context.Dishes.Add(dish);
        await context.SaveChangesAsync();

        var nutrition = new Nutrition { DishId = dish.Id, Calories = 200, Protein = 10, Fats = 5, Carbohydrates = 30, Fiber = 2, WeightGrams = 250 };
        context.Nutritions.Add(nutrition);
        await context.SaveChangesAsync();

        var service = new NutritionService(context);
        var result = await service.GetByDishIdAsync(dish.Id);

        Assert.NotNull(result);
        Assert.Equal(200, result!.Calories);
    }

    [Fact]
    public async Task CreateAsync_AddsNutrition()
    {
        using var context = CreateContext("CreateAsync_AddsNutrition");
        var dish = CreateDish("DishB");
        context.Dishes.Add(dish);
        await context.SaveChangesAsync();

        var service = new NutritionService(context);
        var nutrition = new Nutrition { DishId = dish.Id, Calories = 150, Protein = 8, Fats = 4, Carbohydrates = 20, Fiber = 1, WeightGrams = 200 };
        await service.CreateAsync(nutrition);

        var stored = await context.Nutritions.FirstOrDefaultAsync(n => n.DishId == dish.Id);
        Assert.NotNull(stored);
        Assert.Equal(150, stored!.Calories);
    }

    [Fact]
    public async Task UpdateAsync_ChangesNutrition()
    {
        using var context = CreateContext("UpdateAsync_ChangesNutrition");
        var dish = CreateDish("DishC");
        context.Dishes.Add(dish);
        await context.SaveChangesAsync();

        var nutrition = new Nutrition { DishId = dish.Id, Calories = 100, Protein = 5, Fats = 2, Carbohydrates = 10, Fiber = 1, WeightGrams = 100 };
        context.Nutritions.Add(nutrition);
        await context.SaveChangesAsync();

        var service = new NutritionService(context);
        nutrition.Calories = 120;
        await service.UpdateAsync(nutrition);

        var stored = await context.Nutritions.FindAsync(nutrition.Id);
        Assert.Equal(120, stored!.Calories);
    }

    [Fact]
    public async Task DeleteByDishIdAsync_RemovesNutrition()
    {
        using var context = CreateContext("DeleteByDishIdAsync_RemovesNutrition");
        var dish = CreateDish("DishD");
        context.Dishes.Add(dish);
        await context.SaveChangesAsync();

        var nutrition = new Nutrition { DishId = dish.Id, Calories = 180, Protein = 9, Fats = 3, Carbohydrates = 25, Fiber = 2, WeightGrams = 220 };
        context.Nutritions.Add(nutrition);
        await context.SaveChangesAsync();

        var service = new NutritionService(context);
        await service.DeleteByDishIdAsync(dish.Id);

        var stored = await context.Nutritions.FirstOrDefaultAsync(n => n.DishId == dish.Id);
        Assert.Null(stored);
    }
}
