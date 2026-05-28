using System.Collections.Generic;
using System.Threading.Tasks;
using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models;
using CanteenReservationSystem.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CanteenReservationSystem.Tests;

public class CartServiceTests
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
    public async Task AddToCartAsync_InsertsNewCartItem_WhenNoExistingItem()
    {
        using var context = CreateContext("AddToCartAsync_InsertsNewCartItem");
        var dish = CreateDish("Dish A");
        context.Dishes.Add(dish);
        await context.SaveChangesAsync();

        var cartItem = new CartItems
        {
            UserId = "user1",
            DishId = dish.Id,
            Dish = dish,
            TargetDate = new DateTime(2026, 5, 28),
            Quantity = 2
        };

        var service = new CartService(context);
        await service.AddToCartAsync(cartItem);

        var savedItem = await context.CartItems.FirstOrDefaultAsync(c => c.UserId == "user1");
        Assert.NotNull(savedItem);
        Assert.Equal(2, savedItem!.Quantity);
    }

    [Fact]
    public async Task AddToCartAsync_IncrementsQuantity_WhenExistingItemHasSameDate()
    {
        using var context = CreateContext("AddToCartAsync_IncrementsQuantity");
        var dish = CreateDish("Dish B");
        context.Dishes.Add(dish);
        await context.SaveChangesAsync();

        var existing = new CartItems
        {
            UserId = "user2",
            DishId = dish.Id,
            Dish = dish,
            TargetDate = new DateTime(2026, 5, 29),
            Quantity = 1
        };

        context.CartItems.Add(existing);
        await context.SaveChangesAsync();

        var newItem = new CartItems
        {
            UserId = "user2",
            DishId = dish.Id,
            Dish = dish,
            TargetDate = new DateTime(2026, 5, 29),
            Quantity = 3
        };

        var service = new CartService(context);
        await service.AddToCartAsync(newItem);

        var savedItem = await context.CartItems.FirstOrDefaultAsync(c => c.UserId == "user2");
        Assert.NotNull(savedItem);
        Assert.Equal(4, savedItem!.Quantity);
    }

    [Fact]
    public async Task GetUserCartAsync_ReturnsItemsOrderedByTargetDate()
    {
        using var context = CreateContext("GetUserCartAsync_ReturnsItemsOrderedByTargetDate");
        var dish = CreateDish("Dish C");
        context.Dishes.Add(dish);
        await context.SaveChangesAsync();

        var item1 = new CartItems
        {
            UserId = "user3",
            DishId = dish.Id,
            Dish = dish,
            TargetDate = new DateTime(2026, 5, 30),
            Quantity = 1
        };

        var item2 = new CartItems
        {
            UserId = "user3",
            DishId = dish.Id,
            Dish = dish,
            TargetDate = new DateTime(2026, 5, 28),
            Quantity = 1
        };

        context.CartItems.AddRange(item1, item2);
        await context.SaveChangesAsync();

        var service = new CartService(context);
        var result = await service.GetUserCartAsync("user3");

        Assert.Equal(2, result.Count());
        Assert.Equal(new DateTime(2026, 5, 28), result.ElementAt(0).TargetDate);
    }

    [Fact]
    public async Task UpdateItemAsync_UpdatesQuantityAndNote_WhenItemExists()
    {
        using var context = CreateContext("UpdateItemAsync_UpdatesQuantityAndNote");
        var dish = CreateDish("Dish D");
        context.Dishes.Add(dish);
        await context.SaveChangesAsync();

        var item = new CartItems
        {
            UserId = "user4",
            DishId = dish.Id,
            Dish = dish,
            TargetDate = new DateTime(2026, 6, 1),
            Quantity = 1
        };

        context.CartItems.Add(item);
        await context.SaveChangesAsync();

        var service = new CartService(context);
        await service.UpdateItemAsync("user4", item.Id, 5, "More spicy");

        var updatedItem = await context.CartItems.FindAsync(item.Id);
        Assert.NotNull(updatedItem);
        Assert.Equal(5, updatedItem!.Quantity);
        Assert.Equal("More spicy", updatedItem.Note);
    }

    [Fact]
    public async Task RemoveItemAsync_RemovesItem_WhenItemExists()
    {
        using var context = CreateContext("RemoveItemAsync_RemovesItem");
        var dish = CreateDish("Dish E");
        context.Dishes.Add(dish);
        await context.SaveChangesAsync();

        var item = new CartItems
        {
            UserId = "user5",
            DishId = dish.Id,
            Dish = dish,
            TargetDate = new DateTime(2026, 6, 2),
            Quantity = 1
        };

        context.CartItems.Add(item);
        await context.SaveChangesAsync();

        var service = new CartService(context);
        await service.RemoveItemAsync("user5", item.Id);

        var removed = await context.CartItems.FindAsync(item.Id);
        Assert.Null(removed);
    }

    [Fact]
    public async Task ClearCartAsync_RemovesAllUserItems()
    {
        using var context = CreateContext("ClearCartAsync_RemovesAllUserItems");
        var dish = CreateDish("Dish F");
        context.Dishes.Add(dish);
        await context.SaveChangesAsync();

        var item1 = new CartItems
        {
            UserId = "user6",
            DishId = dish.Id,
            Dish = dish,
            TargetDate = new DateTime(2026, 6, 3),
            Quantity = 1
        };

        var item2 = new CartItems
        {
            UserId = "user6",
            DishId = dish.Id,
            Dish = dish,
            TargetDate = new DateTime(2026, 6, 4),
            Quantity = 2
        };

        context.CartItems.AddRange(item1, item2);
        await context.SaveChangesAsync();

        var service = new CartService(context);
        await service.ClearCartAsync("user6");

        var remaining = await context.CartItems.CountAsync(c => c.UserId == "user6");
        Assert.Equal(0, remaining);
    }
}
