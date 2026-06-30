using CanteenReservationSystem.Data;
using CanteenReservationSystem.Models.ViewModels;
using CanteenReservationSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CanteenReservationSystem.Services;

public class KitchenService : IKitchenService
{
    private readonly ApplicationDbContext _context;

    public KitchenService(ApplicationDbContext context)
    {
        _context = context;
    }

    public KitchenViewModel GetKitchenData(DateTime date)
    {
        var details = _context.OrderDetails
            .Include(x => x.Dish)
                .ThenInclude(d => d.Category)
            .Include(x => x.Dish)
                .ThenInclude(d => d.DishIngredients)
                    .ThenInclude(di => di.Ingredient)
            .Include(x => x.Order)
            .Where(x => x.Order.TargetDate.Date == date.Date)
            .ToList();

        var vm = new KitchenViewModel
        {
            SelectedDate = date
        };

        //Aggregate dish-level data
        vm.Dishes = details
            .GroupBy(x => x.DishId)
            .Select(g => new KitchenDishViewModel
            {
                DishName = g.First().Dish.DishName,
                TotalPortions = g.Sum(x => x.Quantity),
                Notes = g.Select(x => new KitchenNoteViewModel
                {
                    Quantity = x.Quantity,
                    Note = x.Note ?? ""
                }).ToList()
            })
            .ToList();

        //Aggregate category-level totals
        vm.Categories = details
            .GroupBy(x => x.Dish.Category.CategoryName)
            .Select(g => new KitchenCategoryViewModel
            {
                CategoryName = g.Key,
                TotalPortions = g.Sum(x => x.Quantity)
            })
            .ToList();

        //Calculate total ingredient quantities needed for the day
        var ingredientTotals = new Dictionary<string, decimal>();

        foreach (var d in details)
        {
            foreach (var ing in d.Dish.DishIngredients)
            {
                //If the note explicitly mentions removing this ingredient, skip it
                if (d.Note != null &&
                    d.Note.ToLower().Contains(ing.Ingredient.IngredientName.ToLower()))
                    continue;

                var total = ing.GramsPerPortion * d.Quantity;

                if (!ingredientTotals.ContainsKey(ing.Ingredient.IngredientName))
                    ingredientTotals[ing.Ingredient.IngredientName] = 0;

                ingredientTotals[ing.Ingredient.IngredientName] += total;
            }
        }

        //Stock on hand for every ingredient (used by the inventory editor + AI).
        var stock = _context.Ingredients
            .OrderBy(i => i.IngredientName)
            .Select(i => new { i.Id, i.IngredientName, i.AvailableGrams })
            .ToList();

        var stockByName = stock.ToDictionary(
            s => s.IngredientName,
            s => s.AvailableGrams,
            StringComparer.OrdinalIgnoreCase);

        //Convert ingredient totals into view model entries
        vm.Ingredients = ingredientTotals
            .Select(i => new KitchenIngredientViewModel
            {
                IngredientName = i.Key,
                TotalQuantity = i.Value,
                Unit = "g",
                AvailableQuantity = stockByName.TryGetValue(i.Key, out var avail) ? avail : 0
            })
            .ToList();

        vm.Stock = stock
            .Select(s => new KitchenStockViewModel
            {
                IngredientId = s.Id,
                IngredientName = s.IngredientName,
                AvailableGrams = s.AvailableGrams
            })
            .ToList();

        return vm;
    }
}