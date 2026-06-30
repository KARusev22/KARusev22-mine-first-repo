using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CanteenReservationSystem.Controllers;
using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models;
using CanteenReservationSystem.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CanteenReservationSystem.Tests;

public class AdminOrdersControllerTests
{
    private static ApplicationDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ApplicationUser CreateUser(string id, string fullName)
    {
        return new ApplicationUser
        {
            Id = id,
            UserName = id,
            Email = id + "@example.com",
            FullName = fullName,
            Role = "Admin"
        };
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
    public async Task Index_ReturnsOrdersAndUserNames_WhenNoFiltersApplied()
    {
        using var context = CreateContext("AdminOrdersController_Index_ReturnsOrders");

        var user = CreateUser("user1", "Alice");
        var dish = CreateDish("Dish A", "Main");

        context.Users.Add(user);
        context.Dishes.Add(dish);

        var order1 = new Orders
        {
            Id = 1,
            UserId = "user1",
            User = user,
            UniqueCode = "A1",
            TotalPrice = 10m,
            Status = "Pending",
            CreatedAt = new DateTime(2026, 5, 20),
            TargetDate = new DateTime(2026, 5, 21),
            OrderDetails = new List<OrderDetails>
            {
                new OrderDetails { Dish = dish, DishId = dish.Id, Quantity = 1 }
            }
        };

        var order2 = new Orders
        {
            Id = 2,
            UserId = "user1",
            User = user,
            UniqueCode = "B2",
            TotalPrice = 15m,
            Status = "Completed",
            CreatedAt = new DateTime(2026, 5, 21),
            TargetDate = new DateTime(2026, 5, 22),
            OrderDetails = new List<OrderDetails>
            {
                new OrderDetails { Dish = dish, DishId = dish.Id, Quantity = 2, Note = "Extra spice" }
            }
        };

        context.Orders.AddRange(order1, order2);
        await context.SaveChangesAsync();

        var controller = new AdminOrdersController(context);
        var result = await controller.Index(null, null, null) as ViewResult;

        Assert.NotNull(result);
        var model = Assert.IsType<List<OrderViewModel>>(result.Model);
        Assert.Equal(2, model.Count);
        Assert.Equal("B2", model[0].Code);
        Assert.Equal("A1", model[1].Code);
        Assert.Equal("Extra spice", model[0].Notes);

        var viewBagUserNames = Assert.IsType<Dictionary<int, string>>(controller.ViewBag.UserNames);
        Assert.Equal("Alice", viewBagUserNames[1]);
        Assert.Equal("Alice", viewBagUserNames[2]);
    }

    [Fact]
    public async Task Index_FiltersByStatusDateAndCode()
    {
        using var context = CreateContext("AdminOrdersController_Index_Filters");

        var user = CreateUser("user1", "Bob");
        var dish = CreateDish("Dish B", "Side");

        context.Users.Add(user);
        context.Dishes.Add(dish);

        var validOrder = new Orders
        {
            Id = 1,
            UserId = "user1",
            User = user,
            UniqueCode = "B2",
            TotalPrice = 20m,
            Status = "Completed",
            CreatedAt = new DateTime(2026, 5, 22),
            TargetDate = new DateTime(2026, 5, 23),
            OrderDetails = new List<OrderDetails>
            {
                new OrderDetails { Dish = dish, DishId = dish.Id, Quantity = 1 }
            }
        };

        var otherOrder = new Orders
        {
            Id = 2,
            UserId = "user1",
            User = user,
            UniqueCode = "C3",
            TotalPrice = 8m,
            Status = "Pending",
            CreatedAt = new DateTime(2026, 5, 21),
            TargetDate = new DateTime(2026, 5, 24),
            OrderDetails = new List<OrderDetails>
            {
                new OrderDetails { Dish = dish, DishId = dish.Id, Quantity = 1 }
            }
        };

        context.Orders.AddRange(validOrder, otherOrder);
        await context.SaveChangesAsync();

        var controller = new AdminOrdersController(context);
        var result = await controller.Index("Completed", new DateTime(2026, 5, 23), "B") as ViewResult;

        Assert.NotNull(result);
        var model = Assert.IsType<List<OrderViewModel>>(result.Model);
        Assert.Single(model);
        Assert.Equal("B2", model[0].Code);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        using var context = CreateContext("AdminOrdersController_Details_ReturnsNotFound");
        var controller = new AdminOrdersController(context);

        var result = await controller.Details(123);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsOrderViewModelAndUserFullName_WhenOrderExists()
    {
        using var context = CreateContext("AdminOrdersController_Details_ReturnsOrder");

        var user = CreateUser("user1", "Charlie");
        var dish = CreateDish("Dish C", "Dessert");

        context.Users.Add(user);
        context.Dishes.Add(dish);

        var order = new Orders
        {
            Id = 1,
            UserId = "user1",
            User = user,
            UniqueCode = "C3",
            TotalPrice = 12m,
            Status = "Delivered",
            CreatedAt = new DateTime(2026, 5, 24),
            TargetDate = new DateTime(2026, 5, 25),
            OrderDetails = new List<OrderDetails>
            {
                new OrderDetails { Dish = dish, DishId = dish.Id, Quantity = 2 }
            }
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var controller = new AdminOrdersController(context);
        var result = await controller.Details(1) as ViewResult;

        Assert.NotNull(result);
        var model = Assert.IsType<OrderViewModel>(result.Model);
        Assert.Equal(1, model.Id);
        Assert.Equal("C3", model.Code);
        Assert.Equal("Charlie", controller.ViewBag.UserFullName);
    }
}
