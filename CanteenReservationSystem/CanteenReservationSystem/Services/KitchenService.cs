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

        vm.Categories = details
            .GroupBy(x => x.Dish.Category.CategoryName)
            .Select(g => new KitchenCategoryViewModel
            {
                CategoryName = g.Key,
                TotalPortions = g.Sum(x => x.Quantity)
            })
            .ToList();

        var ingredientTotals = new Dictionary<string, decimal>();

        foreach (var d in details)
        {
            foreach (var ing in d.Dish.DishIngredients)
            {
                if (d.Note != null &&
                    d.Note.ToLower().Contains(ing.Ingredient.IngredientName.ToLower()))
                    continue;

                var total = ing.GramsPerPortion * d.Quantity;

                if (!ingredientTotals.ContainsKey(ing.Ingredient.IngredientName))
                    ingredientTotals[ing.Ingredient.IngredientName] = 0;

                ingredientTotals[ing.Ingredient.IngredientName] += total;
            }
        }

        vm.Ingredients = ingredientTotals
            .Select(i => new KitchenIngredientViewModel
            {
                IngredientName = i.Key,
                TotalQuantity = i.Value,
                Unit = "g"
            })
            .ToList();

        return vm;
    }
}