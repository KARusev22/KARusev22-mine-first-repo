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

public class AllergenServiceTests
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
    public async Task GetAllAsync_ReturnsAllergens()
    {
        using var context = CreateContext("GetAllAsync_ReturnsAllergens");
        context.Allergens.Add(new Allergen { AllergenName = "Gluten", IconUrl = "" });
        context.Allergens.Add(new Allergen { AllergenName = "Nuts", IconUrl = "" });
        await context.SaveChangesAsync();

        var service = new AllergenService(context);
        var result = (await service.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, a => a.AllergenName == "Gluten");
        Assert.Contains(result, a => a.AllergenName == "Nuts");
    }

    [Fact]
    public async Task CreateAsync_AddsAllergen()
    {
        using var context = CreateContext("CreateAsync_AddsAllergen");
        var service = new AllergenService(context);

        var allergen = new Allergen { AllergenName = "Soy", IconUrl = "" };
        await service.CreateAsync(allergen);

        var stored = await context.Allergens.FirstOrDefaultAsync(a => a.AllergenName == "Soy");
        Assert.NotNull(stored);
    }

    [Fact]
    public async Task UpdateAsync_ChangesAllergen()
    {
        using var context = CreateContext("UpdateAsync_ChangesAllergen");
        var allergen = new Allergen { AllergenName = "Fish", IconUrl = "" };
        context.Allergens.Add(allergen);
        await context.SaveChangesAsync();

        var service = new AllergenService(context);
        allergen.AllergenName = "Seafood";
        await service.UpdateAsync(allergen);

        var stored = await context.Allergens.FindAsync(allergen.Id);
        Assert.Equal("Seafood", stored!.AllergenName);
    }

    [Fact]
    public async Task DeleteAsync_RemovesAllergen()
    {
        using var context = CreateContext("DeleteAsync_RemovesAllergen");
        var allergen = new Allergen { AllergenName = "Eggs", IconUrl = "" };
        context.Allergens.Add(allergen);
        await context.SaveChangesAsync();

        var service = new AllergenService(context);
        await service.DeleteAsync(allergen.Id);

        var stored = await context.Allergens.FindAsync(allergen.Id);
        Assert.Null(stored);
    }

    [Fact]
    public async Task GetDishesByAllergenAsync_ReturnsLinkedDishes()
    {
        using var context = CreateContext("GetDishesByAllergenAsync_ReturnsLinkedDishes");
        var allergen = new Allergen { AllergenName = "Dairy", IconUrl = "" };
        var dish = CreateDish("Cheesy");

        context.Allergens.Add(allergen);
        context.Dishes.Add(dish);
        await context.SaveChangesAsync();

        var da = new DishAllergen { DishId = dish.Id, AllergenId = allergen.Id };
        context.DishAllergens.Add(da);
        await context.SaveChangesAsync();

        var service = new AllergenService(context);
        var dishes = (await service.GetDishesByAllergenAsync(allergen.Id)).ToList();

        Assert.Single(dishes);
        Assert.Equal(dish.Id, dishes.First().Id);
    }
}
