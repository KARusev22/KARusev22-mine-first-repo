using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models;
using CanteenReservationSystem.Models.ViewModels;
using CanteenReservationSystem.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CanteenReservationSystem.Tests;

public class OrderServiceTests
{
    private static ApplicationDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ApplicationDbContext(options);
    }

    private static Dish CreateDish(string name, string categoryName, decimal price = 5.00m)
    {
        return new Dish
        {
            DishName = name,
            Price = price,
            Category = new Category { CategoryName = categoryName }
        };
    }

    [Fact]
    public async Task GetOrdersByUserAsync_ReturnsOrdersForUserOrderedByCreatedAtDesc()
    {
        using var context = CreateContext("GetOrdersByUserAsync_ReturnsOrdersForUserOrderedByCreatedAtDesc");

        var dish = CreateDish("Dish A", "Main");
        context.Dishes.Add(dish);

        var olderOrder = new Orders
        {
            UserId = "user1",
            TotalPrice = 10m,
            CreatedAt = new DateTime(2026, 5, 20),
            TargetDate = new DateTime(2026, 5, 21),
            Status = "Pending",
            UniqueCode = "A1",
            OrderDetails = new List<OrderDetails>
            {
                new OrderDetails { Dish = dish, DishId = dish.Id, Quantity = 1 }
            }
        };

        var newerOrder = new Orders
        {
            UserId = "user1",
            TotalPrice = 15m,
            CreatedAt = new DateTime(2026, 5, 21),
            TargetDate = new DateTime(2026, 5, 22),
            Status = "Completed",
            UniqueCode = "B2",
            OrderDetails = new List<OrderDetails>
            {
                new OrderDetails { Dish = dish, DishId = dish.Id, Quantity = 1 }
            }
        };

        var otherUserOrder = new Orders
        {
            UserId = "user2",
            TotalPrice = 8m,
            CreatedAt = new DateTime(2026, 5, 19),
            TargetDate = new DateTime(2026, 5, 20),
            Status = "Pending",
            UniqueCode = "C3",
            OrderDetails = new List<OrderDetails>
            {
                new OrderDetails { Dish = dish, DishId = dish.Id, Quantity = 1 }
            }
        };

        context.Orders.AddRange(olderOrder, newerOrder, otherUserOrder);
        await context.SaveChangesAsync();

        var service = new OrderService(context);
        var result = await service.GetOrdersByUserAsync("user1");

        Assert.Equal(2, result.Count);
        Assert.Equal(newerOrder.CreatedAt, result[0].CreatedAt);
        Assert.Equal(olderOrder.CreatedAt, result[1].CreatedAt);
    }

    [Fact]
    public async Task GetUserStatsAsync_ReturnsCorrectAggregatedUserStats()
    {
        using var context = CreateContext("GetUserStatsAsync_ReturnsCorrectAggregatedUserStats");

        var firstDish = CreateDish("Dish A", "Main", 10m);
        var secondDish = CreateDish("Dish B", "Side", 5m);
        context.Dishes.AddRange(firstDish, secondDish);

        var order1 = new Orders
        {
            UserId = "user1",
            TotalPrice = 35m,
            CreatedAt = new DateTime(2026, 5, 20),
            TargetDate = new DateTime(2026, 5, 22),
            Status = "Completed",
            UniqueCode = "A1",
            OrderDetails = new List<OrderDetails>
            {
                new OrderDetails { Dish = firstDish, DishId = firstDish.Id, Quantity = 3 },
                new OrderDetails { Dish = secondDish, DishId = secondDish.Id, Quantity = 1 }
            }
        };

        var order2 = new Orders
        {
            UserId = "user1",
            TotalPrice = 20m,
            CreatedAt = new DateTime(2026, 5, 21),
            TargetDate = new DateTime(2026, 5, 23),
            Status = "Pending",
            UniqueCode = "B2",
            OrderDetails = new List<OrderDetails>
            {
                new OrderDetails { Dish = secondDish, DishId = secondDish.Id, Quantity = 2 }
            }
        };

        context.Orders.AddRange(order1, order2);
        await context.SaveChangesAsync();

        var service = new OrderService(context);
        var stats = await service.GetUserStatsAsync("user1");

        Assert.Equal(2, stats.TotalOrders);
        Assert.Equal(55m, stats.TotalSpent);
        Assert.Equal("Dish A", stats.MostOrderedDish);
        Assert.Equal(1, stats.TakenOrders);
        Assert.Equal(1, stats.NotTakenOrders);
        Assert.Equal(2, stats.Top3Days.Count);
        Assert.Contains("Main", stats.Top3Categories.Keys);
        Assert.Contains("Side", stats.Top3Categories.Keys);
        Assert.Equal(3, stats.Top3Categories["Main"]);
        Assert.Equal(3, stats.Top3Categories["Side"]);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsInvalidOperationException_WhenTargetDateIsPast()
    {
        using var context = CreateContext("UpdateAsync_ThrowsInvalidOperationException_WhenTargetDateIsPast");

        var dish = CreateDish("Dish A", "Main");
        context.Dishes.Add(dish);

        var order = new Orders
        {
            UserId = "user1",
            TotalPrice = 10m,
            CreatedAt = DateTime.Today,
            TargetDate = DateTime.Today.AddDays(1),
            Status = "Pending",
            UniqueCode = "A1",
            OrderDetails = new List<OrderDetails>
            {
                new OrderDetails { Dish = dish, DishId = dish.Id, Quantity = 1 }
            }
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = new OrderService(context);
        var model = new EditOrderViewModel
        {
            OrderId = order.Id,
            TargetDate = DateTime.Today.AddDays(-1),
            Items = new List<EditOrderItemViewModel>
            {
                new EditOrderItemViewModel { DishId = dish.Id, Quantity = 1 }
            }
        };

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.UpdateAsync(order, model));
    }
}
