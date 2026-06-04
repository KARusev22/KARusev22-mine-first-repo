using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CanteenReservationSystem.Controllers;
using CanteenReservationSystem.Models;
using CanteenReservationSystem.Models.ViewModels;
using CanteenReservationSystem.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Xunit;

namespace CanteenReservationSystem.Tests;

public class ReservationControllerTests
{
    private ControllerContext CreateControllerContext(string userId)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }));
        var httpContext = new DefaultHttpContext { User = user };
        return new ControllerContext { HttpContext = httpContext };
    }

    private TempDataDictionary CreateTempData()
    {
        var httpContext = new DefaultHttpContext();
        return new TempDataDictionary(httpContext, new FakeTempDataProvider());
    }

    private class FakeTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context)
        {
            return new Dictionary<string, object>();
        }

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
            // no-op
        }
    }

    private class FakeCartService : ICartService
    {
        private readonly IEnumerable<CartItems> _cart;
        public FakeCartService(IEnumerable<CartItems> cart) => _cart = cart;
        public Task AddToCartAsync(CartItems item) => Task.CompletedTask;
        public Task ClearCartAsync(string userId) => Task.CompletedTask;
        public Task<IEnumerable<CartItems>> GetUserCartAsync(string userId) => Task.FromResult(_cart);
        public Task<List<CartItems>> GetItemsByIdsAsync(List<int> ids) => Task.FromResult(new List<CartItems>());
        public Task RemoveItemAsync(string userId, int cartItemId) => Task.CompletedTask;
        public Task RemoveItemsByIdsAsync(List<int> ids) => Task.CompletedTask;
        public Task UpdateItemAsync(string userId, int cartItemId, int quantity, string? note) => Task.CompletedTask;
    }

    private class FakeReservationService : IReservationService
    {
        public Orders CreatedOrder { get; set; }
        public Func<int, Task<Orders?>> GetByIdImpl { get; set; }
        public Func<string, Task<Orders?>> GetByCodeImpl { get; set; }
        public bool UpdateStatusCalled { get; private set; }

        public Task<Orders> CreateReservationAsync(string userId, DateTime targetDate, List<int> selectedItemIds)
        {
            CreatedOrder = new Orders { Id = 99, UserId = userId, TotalPrice = 0m, Status = "Pending", OrderDetails = new List<OrderDetails>() };
            return Task.FromResult(CreatedOrder);
        }

        public Task<Orders?> GetByIdAsync(int id) => GetByIdImpl != null ? GetByIdImpl(id) : Task.FromResult<Orders?>(CreatedOrder);
        public Task<Orders?> GetByCodeAsync(string code) => GetByCodeImpl != null ? GetByCodeImpl(code) : Task.FromResult<Orders?>(null);
        public Task UpdateStatusAsync(int id, string status) { UpdateStatusCalled = true; return Task.CompletedTask; }
    }

    [Fact]
    public async Task Checkout_ReturnsViewWithCart()
    {
        var cart = new List<CartItems> { new CartItems { Id = 1 } };
        var controller = new ReservationController(new FakeCartService(cart), new FakeReservationService())
        {
            ControllerContext = CreateControllerContext("u1"),
            TempData = CreateTempData()
        };

        var result = await controller.Checkout();
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(cart, view.Model);
    }

    [Fact]
    public async Task CreateReservation_Post_NoItemsSelected_RedirectsToCheckout()
    {
        var controller = new ReservationController(new FakeCartService(new List<CartItems>()), new FakeReservationService())
        {
            ControllerContext = CreateControllerContext("u2"),
            TempData = CreateTempData()
        };

        var model = new ReservationRequestModel { SelectedItemIds = new List<int>() , TargetDate = DateTime.Today.AddDays(2)};
        var result = await controller.CreateReservation(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Checkout", redirect.ActionName);
        Assert.NotNull(controller.TempData["Error"]);
    }

    [Fact]
    public async Task CreateReservation_Post_TargetDateBeforeTomorrow_RedirectsToCheckout()
    {
        var controller = new ReservationController(new FakeCartService(new List<CartItems>()), new FakeReservationService())
        {
            ControllerContext = CreateControllerContext("u3"),
            TempData = CreateTempData()
        };

        var model = new ReservationRequestModel { SelectedItemIds = new List<int> { 1 }, TargetDate = DateTime.Today };
        var result = await controller.CreateReservation(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Checkout", redirect.ActionName);
        Assert.NotNull(controller.TempData["Error"]);
    }

    [Fact]
    public async Task CreateReservation_Post_Valid_RedirectsToSuccess()
    {
        var fakeReservation = new FakeReservationService();
        var controller = new ReservationController(new FakeCartService(new List<CartItems>()), fakeReservation)
        {
            ControllerContext = CreateControllerContext("u4"),
            TempData = CreateTempData()
        };

        var model = new ReservationRequestModel { SelectedItemIds = new List<int> { 1 }, TargetDate = DateTime.Today.AddDays(2) };
        var result = await controller.CreateReservation(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Success", redirect.ActionName);
        Assert.True(redirect.RouteValues.ContainsKey("id"));
    }

    [Fact]
    public async Task Success_ReturnsViewWhenFound()
    {
        var fake = new FakeReservationService();
        fake.GetByIdImpl = id => Task.FromResult<Orders?>(new Orders { Id = id, UniqueCode = "X" });
        var controller = new ReservationController(new FakeCartService(new List<CartItems>()), fake)
        {
            ControllerContext = CreateControllerContext("u5"),
            TempData = CreateTempData()
        };

        var result = await controller.Success(5);
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Orders>(view.Model);
        Assert.Equal(5, model.Id);
    }

    [Fact]
    public async Task Success_ReturnsNotFoundWhenNull()
    {
        var fake = new FakeReservationService();
        fake.GetByIdImpl = id => Task.FromResult<Orders?>(null);
        var controller = new ReservationController(new FakeCartService(new List<CartItems>()), fake)
        {
            ControllerContext = CreateControllerContext("u6"),
            TempData = CreateTempData()
        };

        var result = await controller.Success(6);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ManagerLookup_Post_NoReservation_ShowsError()
    {
        var fake = new FakeReservationService();
        fake.GetByCodeImpl = code => Task.FromResult<Orders?>(null);
        var controller = new ReservationController(new FakeCartService(new List<CartItems>()), fake)
        {
            ControllerContext = CreateControllerContext("u7"),
            TempData = CreateTempData()
        };

        var result = await controller.ManagerLookup("NOPE");
        var view = Assert.IsType<ViewResult>(result);
        Assert.NotNull(controller.TempData["Error"]);
    }

    [Fact]
    public async Task UpdateStatus_Post_RedirectsToManagerLookup()
    {
        var fake = new FakeReservationService();
        var controller = new ReservationController(new FakeCartService(new List<CartItems>()), fake)
        {
            ControllerContext = CreateControllerContext("u8"),
            TempData = CreateTempData()
        };

        var result = await controller.UpdateStatus(1, "Confirmed");
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ManagerLookup", redirect.ActionName);
        Assert.True(fake.UpdateStatusCalled);
    }
}
