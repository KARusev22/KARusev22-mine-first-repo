using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CanteenReservationSystem.Controllers;
using CanteenReservationSystem.Models;
using CanteenReservationSystem.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CanteenReservationSystem.Tests;

public class CartControllerTests
{
    private ControllerContext CreateControllerContext(string userId)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }));
        var httpContext = new DefaultHttpContext { User = user };
        return new ControllerContext { HttpContext = httpContext };
    }

    private class TrackingCartService : ICartService
    {
        public bool AddCalled { get; private set; }
        public bool UpdateCalled { get; private set; }
        public bool RemoveCalled { get; private set; }
        public bool ClearCalled { get; private set; }
        public string? LastUserId { get; private set; }

        private readonly IEnumerable<CartItems> _cart;

        public TrackingCartService(IEnumerable<CartItems> cart = null)
        {
            _cart = cart ?? new List<CartItems>();
        }

        public Task AddToCartAsync(CartItems item)
        {
            AddCalled = true;
            LastUserId = item.UserId;
            return Task.CompletedTask;
        }

        public Task ClearCartAsync(string userId)
        {
            ClearCalled = true;
            LastUserId = userId;
            return Task.CompletedTask;
        }

        public Task<IEnumerable<CartItems>> GetUserCartAsync(string userId)
        {
            LastUserId = userId;
            return Task.FromResult(_cart);
        }

        public Task<List<CartItems>> GetItemsByIdsAsync(List<int> ids) => Task.FromResult(new List<CartItems>());

        public Task RemoveItemAsync(string userId, int cartItemId)
        {
            RemoveCalled = true;
            LastUserId = userId;
            return Task.CompletedTask;
        }

        public Task RemoveItemsByIdsAsync(List<int> ids) => Task.CompletedTask;

        public Task UpdateItemAsync(string userId, int cartItemId, int quantity, string? note)
        {
            UpdateCalled = true;
            LastUserId = userId;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Index_ReturnsViewWithCart()
    {
        var cart = new List<CartItems> { new CartItems { Id = 1 } };
        var svc = new TrackingCartService(cart);
        var controller = new CartController(svc) { ControllerContext = CreateControllerContext("u1") };

        var result = await controller.Index();
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(cart, view.Model);
        Assert.Equal("u1", svc.LastUserId);
    }

    [Fact]
    public async Task Add_Post_RedirectsToIndex_AndCallsAdd()
    {
        var svc = new TrackingCartService();
        var controller = new CartController(svc) { ControllerContext = CreateControllerContext("u2") };

        var result = await controller.Add(5, 2, DateTime.Today.AddDays(1));
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.True(svc.AddCalled);
    }

    [Fact]
    public async Task Update_Post_ReturnsOk_AndCallsUpdate()
    {
        var svc = new TrackingCartService();
        var controller = new CartController(svc) { ControllerContext = CreateControllerContext("u3") };

        var result = await controller.Update(10, 3, "note");
        Assert.IsType<OkResult>(result);
        Assert.True(svc.UpdateCalled);
    }

    [Fact]
    public async Task Remove_Post_RedirectsToIndex_AndCallsRemove()
    {
        var svc = new TrackingCartService();
        var controller = new CartController(svc) { ControllerContext = CreateControllerContext("u4") };

        var result = await controller.Remove(7);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.True(svc.RemoveCalled);
    }

    [Fact]
    public async Task Clear_Post_RedirectsToIndex_AndCallsClear()
    {
        var svc = new TrackingCartService();
        var controller = new CartController(svc) { ControllerContext = CreateControllerContext("u5") };

        var result = await controller.Clear();
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.True(svc.ClearCalled);
    }
}
