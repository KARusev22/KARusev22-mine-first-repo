using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CanteenReservationSystem.Controllers;
using CanteenReservationSystem.Models;
using CanteenReservationSystem.Models.ViewModels;
using CanteenReservationSystem.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CanteenReservationSystem.Tests;

public class OrdersControllerTests
{
    private ControllerContext CreateControllerContext(string userId)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }));
        var httpContext = new DefaultHttpContext { User = user };
        return new ControllerContext { HttpContext = httpContext };
    }

    private class FakeOrderService : IOrderService
    {
        public Func<string, Task<List<Orders>>> GetOrdersByUserAsyncImpl { get; set; } = _ => Task.FromResult(new List<Orders>());
        public Func<int, string, Task<Orders>> GetByIdForUserAsyncImpl { get; set; } = (_, __) => Task.FromResult<Orders>(null!);
        public Func<string, Task<UserStatsViewModel>> GetUserStatsAsyncImpl { get; set; } = _ => Task.FromResult(new UserStatsViewModel());
        public Func<Orders, Task>? DeleteAsyncImpl { get; set; }
        public Func<Orders, EditOrderViewModel, Task>? UpdateAsyncImpl { get; set; }

        public Task<List<Orders>> GetOrdersByUserAsync(string userId) => GetOrdersByUserAsyncImpl(userId);
        public Task<Orders> GetByIdForUserAsync(int id, string userId) => GetByIdForUserAsyncImpl(id, userId);
        public Task<UserStatsViewModel> GetUserStatsAsync(string userId) => GetUserStatsAsyncImpl(userId);
        public Task DeleteAsync(Orders order) => DeleteAsyncImpl?.Invoke(order) ?? Task.CompletedTask;
        public Task UpdateAsync(Orders order, EditOrderViewModel model) => UpdateAsyncImpl?.Invoke(order, model) ?? Task.CompletedTask;
    }

    private class FakeDishService : IDishService
    {
        private readonly IEnumerable<Dish> _dishes;

        public FakeDishService(IEnumerable<Dish> dishes) => _dishes = dishes;
        public Task CreateAsync(Dish dish) => Task.CompletedTask;
        public Task DeleteAsync(int id) => Task.CompletedTask;
        public Task<Dish?> GetByIdAsync(int id) => Task.FromResult(_dishes.FirstOrDefault(d => d.Id == id));
        public Task<IEnumerable<Dish>> GetAllAsync() => Task.FromResult(_dishes);
        public Task<IEnumerable<Dish>> FilterByCategoryAsync(int categoryId) => Task.FromResult(_dishes);
        public Task UpdateAsync(Dish dish) => Task.CompletedTask;
    }

    [Fact]
    public async Task MyOrders_ReturnsViewWithMappedOrders()
    {
        var order = new Orders
        {
            Id = 1,
            UserId = "user1",
            UniqueCode = "ABC",
            TotalPrice = 20m,
            CreatedAt = new DateTime(2026, 5, 20),
            TargetDate = new DateTime(2026, 5, 21),
            Status = "Pending",
            OrderDetails = new List<OrderDetails>
            {
                new OrderDetails { Dish = new Dish { DishName = "Dish" }, Quantity = 2, Note = "note" }
            }
        };

        var fakeOrderService = new FakeOrderService
        {
            GetOrdersByUserAsyncImpl = userId => Task.FromResult(new List<Orders> { order })
        };
        var controller = new OrdersController(fakeOrderService, new FakeDishService(new List<Dish>()))
        {
            ControllerContext = CreateControllerContext("user1")
        };

        var result = await controller.MyOrders();
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<List<OrderViewModel>>(view.Model);
        Assert.Single(model);
        Assert.Equal("ABC", model[0].Code);
        Assert.Equal("note", model[0].Notes);
        Assert.Equal(2, model[0].Items[0].Quantity);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenOrderIsNull()
    {
        var fakeOrderService = new FakeOrderService
        {
            GetByIdForUserAsyncImpl = (id, userId) => Task.FromResult<Orders>(null!)
        };

        var controller = new OrdersController(fakeOrderService, new FakeDishService(new List<Dish>()))
        {
            ControllerContext = CreateControllerContext("user1")
        };

        var result = await controller.Details(1);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsViewWithModel_WhenOrderExists()
    {
        var order = new Orders
        {
            Id = 2,
            UserId = "user1",
            UniqueCode = "XYZ",
            TotalPrice = 30m,
            CreatedAt = new DateTime(2026, 5, 22),
            TargetDate = new DateTime(2026, 5, 23),
            Status = "Completed",
            OrderDetails = new List<OrderDetails>
            {
                new OrderDetails { Dish = new Dish { DishName = "Meal" }, Quantity = 1 }
            }
        };

        var fakeOrderService = new FakeOrderService
        {
            GetByIdForUserAsyncImpl = (id, userId) => Task.FromResult(order)
        };

        var controller = new OrdersController(fakeOrderService, new FakeDishService(new List<Dish>()))
        {
            ControllerContext = CreateControllerContext("user1")
        };

        var result = await controller.Details(2);
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<OrderViewModel>(view.Model);
        Assert.Equal("XYZ", model.Code);
        Assert.Equal("Meal", model.Items[0].Name);
    }

    [Fact]
    public async Task Edit_Get_ReturnsForbid_WhenTargetDateIsTodayOrPast()
    {
        var order = new Orders
        {
            Id = 3,
            UserId = "user1",
            UniqueCode = "FORBID",
            TotalPrice = 15m,
            CreatedAt = DateTime.Today.AddDays(-2),
            TargetDate = DateTime.Today,
            Status = "Pending",
            OrderDetails = new List<OrderDetails>()
        };

        var fakeOrderService = new FakeOrderService
        {
            GetByIdForUserAsyncImpl = (id, userId) => Task.FromResult(order)
        };

        var controller = new OrdersController(fakeOrderService, new FakeDishService(new List<Dish>()))
        {
            ControllerContext = CreateControllerContext("user1")
        };

        var result = await controller.Edit(3);
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_ReturnsOk_WhenOrderExistsAndInFuture()
    {
        var order = new Orders
        {
            Id = 4,
            UserId = "user1",
            UniqueCode = "DEL",
            TotalPrice = 10m,
            CreatedAt = DateTime.Today.AddDays(-1),
            TargetDate = DateTime.Today.AddDays(1),
            Status = "Pending",
            OrderDetails = new List<OrderDetails>()
        };

        var deletedOrders = new List<Orders>();
        var fakeOrderService = new FakeOrderService
        {
            GetByIdForUserAsyncImpl = (id, userId) => Task.FromResult(order),
            DeleteAsyncImpl = o =>
            {
                deletedOrders.Add(o);
                return Task.CompletedTask;
            }
        };

        var controller = new OrdersController(fakeOrderService, new FakeDishService(new List<Dish>()))
        {
            ControllerContext = CreateControllerContext("user1")
        };

        var result = await controller.DeleteConfirmed(4);
        Assert.IsType<OkResult>(result);
        Assert.Single(deletedOrders);
        Assert.Equal(4, deletedOrders[0].Id);
    }

    [Fact]
    public async Task Edit_Post_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        var fakeOrderService = new FakeOrderService
        {
            GetByIdForUserAsyncImpl = (id, userId) => Task.FromResult<Orders>(null!)
        };

        var controller = new OrdersController(fakeOrderService, new FakeDishService(new List<Dish>()))
        {
            ControllerContext = CreateControllerContext("user1")
        };

        var model = new EditOrderViewModel
        {
            OrderId = 5,
            TargetDate = DateTime.Today.AddDays(2),
            Items = new List<EditOrderItemViewModel>(),
            AllDishes = new List<DishOption>()
        };

        var result = await controller.Edit(model);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_ReturnsView_WhenTargetDateIsInPast()
    {
        var order = new Orders
        {
            Id = 6,
            UserId = "user1",
            TargetDate = DateTime.Today.AddDays(3),
            OrderDetails = new List<OrderDetails>()
        };

        var fakeOrderService = new FakeOrderService
        {
            GetByIdForUserAsyncImpl = (id, userId) => Task.FromResult(order)
        };

        var controller = new OrdersController(fakeOrderService, new FakeDishService(new List<Dish>()))
        {
            ControllerContext = CreateControllerContext("user1")
        };

        var model = new EditOrderViewModel
        {
            OrderId = 6,
            TargetDate = DateTime.Today.AddDays(-1),
            Items = new List<EditOrderItemViewModel>(),
            AllDishes = new List<DishOption>()
        };

        var result = await controller.Edit(model);
        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(model, view.Model);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey("TargetDate"));
    }

    [Fact]
    public async Task Edit_Post_RedirectsToMyOrders_WhenModelIsValid()
    {
        var order = new Orders
        {
            Id = 7,
            UserId = "user1",
            TargetDate = DateTime.Today.AddDays(3),
            OrderDetails = new List<OrderDetails>()
        };

        var updateCalled = false;
        var fakeOrderService = new FakeOrderService
        {
            GetByIdForUserAsyncImpl = (id, userId) => Task.FromResult(order),
            UpdateAsyncImpl = (o, model) =>
            {
                updateCalled = true;
                return Task.CompletedTask;
            }
        };

        var controller = new OrdersController(fakeOrderService, new FakeDishService(new List<Dish>()))
        {
            ControllerContext = CreateControllerContext("user1")
        };

        var model = new EditOrderViewModel
        {
            OrderId = 7,
            TargetDate = DateTime.Today.AddDays(4),
            Items = new List<EditOrderItemViewModel>(),
            AllDishes = new List<DishOption>()
        };

        var result = await controller.Edit(model);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("MyOrders", redirect.ActionName);
        Assert.True(updateCalled);
    }

    [Fact]
    public async Task Stats_ReturnsViewWithUserStats()
    {
        var expectedStats = new UserStatsViewModel { TotalOrders = 2, TotalSpent = 45m };
        var fakeOrderService = new FakeOrderService
        {
            GetUserStatsAsyncImpl = userId => Task.FromResult(expectedStats)
        };

        var controller = new OrdersController(fakeOrderService, new FakeDishService(new List<Dish>()))
        {
            ControllerContext = CreateControllerContext("user1")
        };

        var result = await controller.Stats();
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UserStatsViewModel>(view.Model);
        Assert.Equal(2, model.TotalOrders);
        Assert.Equal(45m, model.TotalSpent);
    }
}
