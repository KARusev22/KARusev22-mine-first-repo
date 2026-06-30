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

            //Map database entities to a view model
            var model = orders.Select(o => new OrderViewModel
            {
                Id = o.Id,
                Code = o.UniqueCode,
                CreatedOn = o.CreatedAt,
                TargetDate = o.TargetDate,
                Status = o.Status,
                
                //Combine notes from all order items
                Notes = o.OrderDetails.Any(d => d.Note != null)
                    ? string.Join("; ", o.OrderDetails.Where(d => d.Note != null).Select(d => d.Note))
                    : null,
                TotalPrice = o.TotalPrice,
                
                //Map order items
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

            //Prevent access to orders belonging to other users
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

            //Prevent editing past or same-day orders
            if (order.TargetDate.Date <= DateTime.Today)
                return Forbid();

            var dishes = await _dishService.GetAllAsync();

            //Build view model for editing
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
                
                //Provide all dishes for selection
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

            //Prevent editing past or same-day orders
            if (order.TargetDate.Date <= DateTime.Today)
                return Forbid();
            
            //Validate new target date
            if (model.TargetDate.Date <= DateTime.Today)
            {
                ModelState.AddModelError("TargetDate", "You cannot set a past date.");
                
                //Reload dish list and update item display fields
                var dishes = await _dishService.GetAllAsync();
                model.AllDishes = dishes.Select(d => new DishOption
                {
                    Id = d.Id,
                    Name = d.DishName,
                    Price = d.Price
                }).ToList();

                foreach (var item in model.Items)
                {
                    var dish = dishes.FirstOrDefault(x => x.Id == item.DishId);
                    if (dish != null)
                    {
                        item.DishName = dish.DishName;
                        item.Price = dish.Price;
                    }
                }
                
                //Remove fields to avoid validation errors
                for (int i = 0; i < model.Items.Count; i++)
                {
                    ModelState.Remove($"Items[{i}].Price");
                    ModelState.Remove($"Items[{i}].DishName");
                    ModelState.Remove($"Items[{i}].Quantity");
                    ModelState.Remove($"Items[{i}].DishId");
                    ModelState.Remove($"Items[{i}].OrderDetailId");
                }
                
                ModelState.Remove("AllDishes"); 
                return View(model);
            }

            //Attempt to update order via service
            var error = await _orderService.UpdateAsync(order, model);

            //If service error, re-render form with validation message
            if (error != null)
            {
                ModelState.AddModelError("", error);

                var dishes = await _dishService.GetAllAsync();
                model.AllDishes = dishes.Select(d => new DishOption
                {
                    Id = d.Id,
                    Name = d.DishName,
                    Price = d.Price
                }).ToList();

                foreach (var item in model.Items)
                {
                    var dish = dishes.FirstOrDefault(x => x.Id == item.DishId);
                    if (dish != null)
                    {
                        item.DishName = dish.DishName;
                        item.Price = dish.Price;
                    }
                }
                
                //Remove fields to avoid validation errors
                for (int i = 0; i < model.Items.Count; i++)
                {
                    ModelState.Remove($"Items[{i}].Price");
                    ModelState.Remove($"Items[{i}].DishName");
                    ModelState.Remove($"Items[{i}].Quantity");
                    ModelState.Remove($"Items[{i}].DishId");
                    ModelState.Remove($"Items[{i}].OrderDetailId");
                }
                
                ModelState.Remove("AllDishes");
                return View(model);
            }

            return RedirectToAction("MyOrders");
        }
        
        //Displays user-specific order statistics
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

            //Prevent deletion of past or same-day orders
            if (order.TargetDate.Date <= DateTime.Today)
                return Forbid();

            await _orderService.DeleteAsync(order);

            return Ok();
        }
    }
}