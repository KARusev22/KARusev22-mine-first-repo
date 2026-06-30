using Microsoft.AspNetCore.Mvc;
using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models;
using CanteenReservationSystem.Services;
using CanteenReservationSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CanteenReservationSystem.Controllers;

public class CartController : Controller
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    public async Task<IActionResult> Index()
    {
        //Retrieve the logged-in user's ID from authentication claims
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var cart = await _cartService.GetUserCartAsync(userId);
        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> Add(int dishId, int quantity)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //Build cart item model
        var item = new CartItems
        {
            UserId = userId,
            DishId = dishId,
            Quantity = quantity,
        };

        //Delegate logic to the cart service
        await _cartService.AddToCartAsync(item);

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Update(int cartItemId, int quantity, string? note)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //Service handles validation and update logic
        await _cartService.UpdateItemAsync(userId, cartItemId, quantity, note);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Remove(int cartItemId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        await _cartService.RemoveItemAsync(userId, cartItemId);

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Clear()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        await _cartService.ClearCartAsync(userId);

        return RedirectToAction("Index");
    }
}