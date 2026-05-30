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
        private readonly IDishService _dishService;
        public OrdersController(IOrderService orderService, IDishService dishService)
        {
            _orderService = orderService;
            _dishService = dishService;
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
                TargetDate = o.TargetDate,
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
public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _orderService.GetByIdForUserAsync(id, userId);

            if (order == null)
                return NotFound();

            var model = new OrderViewModel
            {
                Id = order.Id,
                Code = order.UniqueCode,
                CreatedOn = order.CreatedAt,
                TargetDate = order.TargetDate,
                Status = order.Status,
                Notes = order.OrderDetails.Any(d => d.Note != null)
                    ? string.Join("; ", order.OrderDetails.Where(d => d.Note != null).Select(d => d.Note))
                    : null,
                TotalPrice = order.TotalPrice,
                Items = order.OrderDetails.Select(d => new OrderItemViewModel
                {
                    Name = d.Dish.DishName,
                    Quantity = d.Quantity
                }).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _orderService.GetByIdForUserAsync(id, userId);

            if (order == null)
                return NotFound();

            if (order.TargetDate.Date <= DateTime.Today)
                return Forbid();

            var dishes = await _dishService.GetAllAsync();

            var model = new EditOrderViewModel
            {
                OrderId = order.Id,
                TargetDate = order.TargetDate,
                Items = order.OrderDetails.Select(d => new EditOrderItemViewModel
                {
                    OrderDetailId = d.Id,
                    DishId = d.DishId,
                    DishName = d.Dish.DishName,
                    Price = d.Dish.Price,
                    Quantity = d.Quantity,
                    Note = d.Note
                }).ToList(),
                AllDishes = dishes.Select(d => new DishOption
                {
                    Id = d.Id,
                    Name = d.DishName,
                    Price = d.Price
                }).ToList()
            };

            return View(model);
        }
        
        [HttpPost]
        public async Task<IActionResult> Edit(EditOrderViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _orderService.GetByIdForUserAsync(model.OrderId, userId);

            if (order == null)
                return NotFound();

            if (order.TargetDate.Date <= DateTime.Today)
                return Forbid();
            
            if (model.TargetDate.Date <= DateTime.Today)
            {
                ModelState.AddModelError("TargetDate", "You cannot set a past date.");
                return View(model);
            }

            await _orderService.UpdateAsync(order, model);

            return RedirectToAction("MyOrders");
        }
        
        public async Task<IActionResult> Stats()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var stats = await _orderService.GetUserStatsAsync(userId);
            return View(stats);
        }
        
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _orderService.GetByIdForUserAsync(id, userId);

            if (order == null)
                return NotFound();

            if (order.TargetDate.Date <= DateTime.Today)
                return Forbid();

            await _orderService.DeleteAsync(order);

            return Ok();
        }
    }
}