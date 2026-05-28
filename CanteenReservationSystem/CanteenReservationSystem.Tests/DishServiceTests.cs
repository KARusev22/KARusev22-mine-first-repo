using System.Collections.Generic;
using System.Threading.Tasks;
using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models;
using CanteenReservationSystem.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CanteenReservationSystem.Tests;

public class DishServiceTests
{
    private static ApplicationDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllDishes()
    {
        using var context = CreateContext("GetAllAsync_ReturnsAllDishes");
        var category = new Category { CategoryName = "Main" };
        var dish = new Dish
        {
            DishName = "Test Dish",
            Price = 9.99m,
            ImageUrl = "image.png",
            Characteristics = "Nice",
            Description = "Tasty",
            Category = category,
            Nutrition = new Nutrition
            {
                Calories = 100,
                Protein = 10,
                Fats = 5,
                Carbohydrates = 20,
                Fiber = 2,
                WeightGrams = 200
            },
            DishIngredients = new List<DishIngredient>(),
            DishAllergens = new List<DishAllergen>()
        };

        context.Dishes.Add(dish);
        await context.SaveChangesAsync();

        var service = new DishService(context);
        var result = await service.GetAllAsync();

        Assert.Single(result);
        Assert.Equal("Test Dish", Assert.Single(result).DishName);
        Assert.Equal("Main", Assert.Single(result).Category.CategoryName);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsMatchingDish()
    {
        using var context = CreateContext("GetByIdAsync_ReturnsMatchingDish");
        var category = new Category { CategoryName = "Main" };
        var dish = new Dish
        {
            DishName = "Dish 1",
            Price = 7.50m,
            ImageUrl = "dish1.png",
            Characteristics = "Fresh",
            Description = "Delicious",
            Category = category,
            Nutrition = new Nutrition
            {
                Calories = 150,
                Protein = 8,
                Fats = 6,
                Carbohydrates = 18,
                Fiber = 3,
                WeightGrams = 180
            },
            DishIngredients = new List<DishIngredient>(),
            DishAllergens = new List<DishAllergen>()
        };

        context.Dishes.Add(dish);
        await context.SaveChangesAsync();

        var service = new DishService(context);
        var result = await service.GetByIdAsync(dish.Id);

        Assert.NotNull(result);
        Assert.Equal("Dish 1", result!.DishName);
    }

    [Fact]
    public async Task CreateAsync_AddsDishToDatabase()
    {
        using var context = CreateContext("CreateAsync_AddsDishToDatabase");
        var category = new Category { CategoryName = "Sides" };
        var dish = new Dish
        {
            DishName = "New Dish",
            Price = 12.00m,
            ImageUrl = "newdish.png",
            Characteristics = "Hot",
            Description = "Fresh and hot",
            Category = category,
            Nutrition = new Nutrition
            {
                Calories = 200,
                Protein = 12,
                Fats = 7,
                Carbohydrates = 25,
                Fiber = 4,
                WeightGrams = 250
            },
            DishIngredients = new List<DishIngredient>(),
            DishAllergens = new List<DishAllergen>()
        };

        var service = new DishService(context);
        await service.CreateAsync(dish);

        var savedDish = await context.Dishes.FirstOrDefaultAsync(d => d.DishName == "New Dish");
        Assert.NotNull(savedDish);
        Assert.Equal(12.00m, savedDish!.Price);
    }

    [Fact]
    public async Task DeleteAsync_RemovesDishFromDatabase()
    {
        using var context = CreateContext("DeleteAsync_RemovesDishFromDatabase");
        var category = new Category { CategoryName = "Main" };
        var dish = new Dish
        {
            DishName = "Delete Dish",
            Price = 8.00m,
            ImageUrl = "delete.png",
            Characteristics = "Simple",
            Description = "Remove me",
            Category = category,
            Nutrition = new Nutrition
            {
                Calories = 110,
                Protein = 9,
                Fats = 4,
                Carbohydrates = 16,
                Fiber = 1,
                WeightGrams = 190
            },
            DishIngredients = new List<DishIngredient>(),
            DishAllergens = new List<DishAllergen>()
        };

        context.Dishes.Add(dish);
        await context.SaveChangesAsync();

        var service = new DishService(context);
        await service.DeleteAsync(dish.Id);

        var savedDish = await context.Dishes.FindAsync(dish.Id);
        Assert.Null(savedDish);
    }

    [Fact]
    public async Task FilterByCategoryAsync_ReturnsOnlyMatchingDishes()
    {
        using var context = CreateContext("FilterByCategoryAsync_ReturnsOnlyMatchingDishes");
        var category1 = new Category { CategoryName = "Main" };
        var category2 = new Category { CategoryName = "Dessert" };

        var dish1 = new Dish
        {
            DishName = "Main Dish",
            Price = 10.00m,
            ImageUrl = "main.png",
            Characteristics = "Hearty",
            Description = "Main course",
            Category = category1,
            Nutrition = new Nutrition
            {
                Calories = 220,
                Protein = 15,
                Fats = 8,
                Carbohydrates = 24,
                Fiber = 2,
                WeightGrams = 260
            },
            DishIngredients = new List<DishIngredient>(),
            DishAllergens = new List<DishAllergen>()
        };

        var dish2 = new Dish
        {
            DishName = "Dessert Dish",
            Price = 6.00m,
            ImageUrl = "dessert.png",
            Characteristics = "Sweet",
            Description = "Dessert course",
            Category = category2,
            Nutrition = new Nutrition
            {
                Calories = 180,
                Protein = 4,
                Fats = 6,
                Carbohydrates = 30,
                Fiber = 1,
                WeightGrams = 150
            },
            DishIngredients = new List<DishIngredient>(),
            DishAllergens = new List<DishAllergen>()
        };

        context.Dishes.AddRange(dish1, dish2);
        await context.SaveChangesAsync();

        var service = new DishService(context);
        var result = await service.FilterByCategoryAsync(category1.Id);

        Assert.Single(result);
        Assert.Equal("Main Dish", Assert.Single(result).DishName);
    }
}
