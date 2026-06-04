using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models;
using CanteenReservationSystem.Services;
using CanteenReservationSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CanteenReservationSystem.Tests;

public class ReservationServiceTests
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

    private class FakeCartService : ICartService
    {
        private readonly List<CartItems> _items;
        public bool RemoveItemsByIdsCalled { get; private set; }
        public List<int> RemovedIds { get; private set; } = new();

        public FakeCartService(List<CartItems> items)
        {
            _items = items;
        }

        public Task AddToCartAsync(CartItems item) => throw new NotImplementedException();
        public Task ClearCartAsync(string userId) => throw new NotImplementedException();
        public Task<IEnumerable<CartItems>> GetUserCartAsync(string userId) => throw new NotImplementedException();
        public Task<List<CartItems>> GetItemsByIdsAsync(List<int> ids)
        {
            return Task.FromResult(_items.Where(c => ids.Contains(c.Id)).ToList());
        }

        public Task RemoveItemAsync(string userId, int cartItemId) => throw new NotImplementedException();
        public Task RemoveItemsByIdsAsync(List<int> ids)
        {
            RemovedIds.AddRange(ids);
            RemoveItemsByIdsCalled = true;
            return Task.CompletedTask;
        }

        public Task UpdateItemAsync(string userId, int cartItemId, int quantity, string? note) => throw new NotImplementedException();
    }

    [Fact]
    public async Task CreateReservationAsync_ThrowsWhenNoItemsSelected()
    {
        using var context = CreateContext("CreateReservationAsync_ThrowsWhenNoItemsSelected");
        var service = new ReservationService(context, new FakeCartService(new List<CartItems>()));

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.CreateReservationAsync("user1", DateTime.Today.AddDays(2), new List<int> { 1 }));
    }

    [Fact]
    public async Task CreateReservationAsync_ThrowsWhenTargetDateBeforeTomorrow()
    {
        using var context = CreateContext("CreateReservationAsync_ThrowsWhenTargetDateBeforeTomorrow");
        var dish = CreateDish("Dish");
        var item = new CartItems { Id = 1, Dish = dish, DishId = 1, Quantity = 1, UserId = "user2", TargetDate = DateTime.Today.AddDays(2) };
        var service = new ReservationService(context, new FakeCartService(new List<CartItems> { item }));

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.CreateReservationAsync("user2", DateTime.Today, new List<int> { 1 }));
    }

    [Fact]
    public async Task CreateReservationAsync_CreatesOrderAndCallsRemoveItems()
    {
        using var context = CreateContext("CreateReservationAsync_CreatesOrderAndCallsRemoveItems");
        var dish = CreateDish("Test Dish");
        dish.Price = 10.00m;
        var item = new CartItems { Id = 1, Dish = dish, DishId = 1, Quantity = 2, UserId = "user3", Note = "No onions", TargetDate = DateTime.Today.AddDays(2) };
        var cartService = new FakeCartService(new List<CartItems> { item });
        var service = new ReservationService(context, cartService);

        var order = await service.CreateReservationAsync("user3", DateTime.Today.AddDays(2), new List<int> { 1 });

        Assert.Equal("Pending", order.Status);
        Assert.Equal(20.00m, order.TotalPrice);
        Assert.Single(order.OrderDetails);
        Assert.Equal("No onions", order.OrderDetails.First().Note);
        Assert.True(cartService.RemoveItemsByIdsCalled);
        Assert.Contains(1, cartService.RemovedIds);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsOrderWithDetails()
    {
        using var context = CreateContext("GetByIdAsync_ReturnsOrderWithDetails");
        var dish = CreateDish("Dish");
        context.Dishes.Add(dish);
        await context.SaveChangesAsync();

        var user = new ApplicationUser
        {
            Id = "user4",
            UserName = "user4@example.com",
            NormalizedUserName = "USER4@EXAMPLE.COM",
            Email = "user4@example.com",
            NormalizedEmail = "USER4@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            FullName = "User Four",
            Role = "Client"
        };
        context.Users.Add(user);

        var order = new Orders
        {
            UserId = "user4",
            UniqueCode = "ABC12345",
            Status = "Pending",
            CreatedAt = DateTime.Now,
            TargetDate = DateTime.Today.AddDays(2),
            TotalPrice = 12.00m,
            OrderDetails = new List<OrderDetails>
            {
                new OrderDetails { Dish = dish, DishId = dish.Id, Quantity = 1 }
            }
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        Assert.True(order.Id > 0, "Order id should be generated after saving.");

        var direct = await context.Orders.FindAsync(order.Id);
        Assert.NotNull(direct);

        var directIncluded = await context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Dish)
            .FirstOrDefaultAsync(o => o.Id == order.Id);
        Assert.NotNull(directIncluded);

        var service = new ReservationService(context, new FakeCartService(new List<CartItems>()));
        var result = await service.GetByIdAsync(order.Id);

        Assert.NotNull(result);
        Assert.Equal("ABC12345", result!.UniqueCode);
        Assert.Single(result.OrderDetails);
    }

    [Fact]
    public async Task UpdateStatusAsync_ChangesOrderStatus()
    {
        using var context = CreateContext("UpdateStatusAsync_ChangesOrderStatus");
        var order = new Orders
        {
            UserId = "user5",
            UniqueCode = "XYZ12345",
            Status = "Pending",
            CreatedAt = DateTime.Now,
            TargetDate = DateTime.Today.AddDays(3),
            TotalPrice = 15.00m,
            OrderDetails = new List<OrderDetails>()
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = new ReservationService(context, new FakeCartService(new List<CartItems>()));
        await service.UpdateStatusAsync(order.Id, "Confirmed");

        var updated = await context.Orders.FindAsync(order.Id);
        Assert.Equal("Confirmed", updated!.Status);
    }
}
