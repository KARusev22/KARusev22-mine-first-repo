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

public class CategoryServiceTests
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
    public async Task GetAllAsync_ReturnsCategories()
    {
        using var context = CreateContext("GetAllAsync_ReturnsCategories");
        context.Categories.Add(new Category { CategoryName = "Main" });
        context.Categories.Add(new Category { CategoryName = "Dessert" });
        await context.SaveChangesAsync();

        var service = new CategoryService(context);
        var result = (await service.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.CategoryName == "Main");
        Assert.Contains(result, c => c.CategoryName == "Dessert");
    }

    [Fact]
    public async Task CreateAsync_AddsCategory()
    {
        using var context = CreateContext("CreateAsync_AddsCategory");
        var service = new CategoryService(context);

        var category = new Category { CategoryName = "Drinks" };
        await service.CreateAsync(category);

        var stored = await context.Categories.FirstOrDefaultAsync(c => c.CategoryName == "Drinks");
        Assert.NotNull(stored);
    }

    [Fact]
    public async Task UpdateAsync_ChangesCategory()
    {
        using var context = CreateContext("UpdateAsync_ChangesCategory");
        var category = new Category { CategoryName = "Sides" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var service = new CategoryService(context);
        category.CategoryName = "Side Dishes";
        await service.UpdateAsync(category);

        var stored = await context.Categories.FindAsync(category.Id);
        Assert.Equal("Side Dishes", stored!.CategoryName);
    }

    [Fact]
    public async Task DeleteAsync_RemovesCategory()
    {
        using var context = CreateContext("DeleteAsync_RemovesCategory");
        var category = new Category { CategoryName = "Temp" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var service = new CategoryService(context);
        await service.DeleteAsync(category.Id);

        var stored = await context.Categories.FindAsync(category.Id);
        Assert.Null(stored);
    }

    [Fact]
    public async Task GetDishesByCategoryAsync_ReturnsLinkedDishes()
    {
        using var context = CreateContext("GetDishesByCategoryAsync_ReturnsLinkedDishes");
        var category = new Category { CategoryName = "Mains" };
        var dish = CreateDish("Steak");

        context.Categories.Add(category);
        context.Dishes.Add(dish);
        await context.SaveChangesAsync();

        // ensure dish links to category
        dish.CategoryId = category.Id;
        context.Dishes.Update(dish);
        await context.SaveChangesAsync();

        var service = new CategoryService(context);
        var dishes = (await service.GetDishesByCategoryAsync(category.Id)).ToList();

        Assert.Single(dishes);
        Assert.Equal(dish.Id, dishes.First().Id);
    }
}
