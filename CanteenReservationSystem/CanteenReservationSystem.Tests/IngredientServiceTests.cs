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

public class IngredientServiceTests
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
            Nutrition = new Nutrition
            {
                Calories = 100,
                Protein = 5,
                Fats = 3,
                Carbohydrates = 12,
                Fiber = 1,
                WeightGrams = 150
            },
            DishIngredients = new List<DishIngredient>(),
            DishAllergens = new List<DishAllergen>()
        };
    }

    [Fact]
    public async Task GetAllAsync_ReturnsIngredients()
    {
        using var context = CreateContext("GetAllAsync_ReturnsIngredients");
        context.Ingredients.Add(new Ingredient { IngredientName = "Salt" });
        context.Ingredients.Add(new Ingredient { IngredientName = "Pepper" });
        await context.SaveChangesAsync();

        var service = new IngredientService(context);
        var result = (await service.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, i => i.IngredientName == "Salt");
        Assert.Contains(result, i => i.IngredientName == "Pepper");
    }

    [Fact]
    public async Task CreateAsync_AddsIngredient()
    {
        using var context = CreateContext("CreateAsync_AddsIngredient");
        var service = new IngredientService(context);

        var ingredient = new Ingredient { IngredientName = "Garlic" };
        await service.CreateAsync(ingredient);

        var stored = await context.Ingredients.FirstOrDefaultAsync(i => i.IngredientName == "Garlic");
        Assert.NotNull(stored);
    }

    [Fact]
    public async Task UpdateAsync_ChangesIngredient()
    {
        using var context = CreateContext("UpdateAsync_ChangesIngredient");
        var ingredient = new Ingredient { IngredientName = "Tomato" };
        context.Ingredients.Add(ingredient);
        await context.SaveChangesAsync();

        var service = new IngredientService(context);
        ingredient.IngredientName = "Tomatoes";
        await service.UpdateAsync(ingredient);

        var stored = await context.Ingredients.FindAsync(ingredient.Id);
        Assert.Equal("Tomatoes", stored!.IngredientName);
    }

    [Fact]
    public async Task DeleteAsync_RemovesIngredient()
    {
        using var context = CreateContext("DeleteAsync_RemovesIngredient");
        var ingredient = new Ingredient { IngredientName = "Onion" };
        context.Ingredients.Add(ingredient);
        await context.SaveChangesAsync();

        var service = new IngredientService(context);
        await service.DeleteAsync(ingredient.Id);

        var stored = await context.Ingredients.FindAsync(ingredient.Id);
        Assert.Null(stored);
    }

    [Fact]
    public async Task GetDishesByIngredientAsync_ReturnsLinkedDishes()
    {
        using var context = CreateContext("GetDishesByIngredientAsync_ReturnsLinkedDishes");
        var ingredient = new Ingredient { IngredientName = "Cheese" };
        var dish = CreateDish("Pasta");

        context.Ingredients.Add(ingredient);
        context.Dishes.Add(dish);
        await context.SaveChangesAsync();

        var di = new DishIngredient { DishId = dish.Id, IngredientId = ingredient.Id };
        context.DishIngredients.Add(di);
        await context.SaveChangesAsync();

        var service = new IngredientService(context);
        var dishes = (await service.GetDishesByIngredientAsync(ingredient.Id)).ToList();

        Assert.Single(dishes);
        Assert.Equal(dish.Id, dishes.First().Id);
    }

    [Fact]
    public async Task FindOrCreateByNameAsync_FindsExistingOrCreates()
    {
        using var context = CreateContext("FindOrCreateByNameAsync_FindsExistingOrCreates");
        var service = new IngredientService(context);

        // create new
        var created = await service.FindOrCreateByNameAsync("Basil");
        Assert.NotNull(created);
        Assert.Equal("Basil", created.IngredientName);

        // find existing (case-insensitive)
        var found = await service.FindOrCreateByNameAsync("basil");
        Assert.Equal(created.Id, found.Id);
    }
}
