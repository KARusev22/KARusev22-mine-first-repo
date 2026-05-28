using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CanteenReservationSystem.Services.Interfaces;
using CanteenReservationSystem.Models.ViewModels;

namespace CanteenReservationSystem.Controllers
{
    [Authorize(Roles = "User")]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> MyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await _orderService.GetOrdersByUserAsync(userId);

            var model = orders.Select(o => new OrderViewModel
            {
                Id = o.Id,
                Code = o.UniqueCode,
                CreatedOn = o.CreatedAt,
                Status = o.Status,
                Notes = o.OrderDetails.Any(d => d.Note != null)
                    ? string.Join("; ", o.OrderDetails.Where(d => d.Note != null).Select(d => d.Note))
                    : null,
                TotalPrice = o.TotalPrice,
                Items = o.OrderDetails.Select(d => new OrderItemViewModel
                {
                    Name = d.Dish.DishName,
                    Quantity = d.Quantity
                }).ToList()
            }).ToList();

            return View(model);
        }

        public async Task<IActionResult> Stats()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var stats = await _orderService.GetUserStatsAsync(userId);
            return View(stats);
        }
    }
}