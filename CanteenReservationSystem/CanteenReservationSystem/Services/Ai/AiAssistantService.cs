using System.Text;
using System.Text.Json;
using CanteenReservationSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace CanteenReservationSystem.Services.Ai;

public class AiAssistantService : IAiAssistantService
{
    private readonly ApplicationDbContext _context;
    private readonly IOpenRouterClient _client;
    private readonly ILogger<AiAssistantService> _logger;

    private static readonly JsonSerializerOptions ParseOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AiAssistantService(ApplicationDbContext context, IOpenRouterClient client, ILogger<AiAssistantService> logger)
    {
        _context = context;
        _client = client;
        _logger = logger;
    }

    public bool IsConfigured => _client.IsConfigured;

    // ---------------------------------------------------------------- USER ----
    public async Task<AiResult<WeeklyPlan>> BuildWeeklyMenuAsync(WeeklyMenuRequest request, CancellationToken ct = default)
    {
        if (!IsConfigured) return AiResult<WeeklyPlan>.Fail("AI is not configured.");

        var days = Math.Clamp(request.Days, 1, 7);
        var start = DateTime.Today.AddDays(1);

        // Collect the dishes actually scheduled (via MonthlyMenu) for each upcoming day.
        var sb = new StringBuilder();
        var anyAvailable = false;
        for (var i = 0; i < days; i++)
        {
            var date = start.AddDays(i);
            var dishes = await _context.MonthlyMenu
                .Where(m => m.DishId != null
                            && m.DayOfWeek == date.DayOfWeek
                            && m.Month == date.Month
                            && m.Year == date.Year)
                .Include(m => m.Dish).ThenInclude(d => d.Category)
                .Include(m => m.Dish).ThenInclude(d => d.Nutrition)
                .Include(m => m.Dish).ThenInclude(d => d.DishAllergens).ThenInclude(da => da.Allergen)
                .Select(m => m.Dish!)
                .Distinct()
                .ToListAsync(ct);

            sb.AppendLine($"{date:dddd, dd MMM yyyy}:");
            if (dishes.Count == 0)
            {
                sb.AppendLine("  (no dishes scheduled)");
                continue;
            }
            anyAvailable = true;
            foreach (var d in dishes)
            {
                var allergens = d.DishAllergens?.Select(a => a.Allergen.AllergenName) ?? Enumerable.Empty<string>();
                var allergenStr = allergens.Any() ? $"; allergens: {string.Join(", ", allergens)}" : "";
                var cal = d.Nutrition != null ? $"{d.Nutrition.Calories} kcal" : "n/a";
                sb.AppendLine($"  - {d.DishName} [{d.Category?.CategoryName}] {d.Price:0.00} EUR; {cal}{allergenStr}");
            }
        }

        if (!anyAvailable)
            return AiResult<WeeklyPlan>.Fail("There are no dishes scheduled for the selected days.");

        var system =
            "You are a helpful canteen meal planner for the FIGusta app. " +
            "Build a weekly meal plan choosing ONLY from the dishes listed as available for each day. " +
            "Respect the user's preferences, calorie target and allergens to avoid. " +
            "Pick 1-2 dishes per day. Reply ONLY with a JSON object of the shape: " +
            "{\"summary\": string, \"days\": [{\"day\": string, \"items\": [{\"dish\": string, \"category\": string, \"reason\": string}], \"totalCalories\": number, \"note\": string}]}.";

        var user = new StringBuilder();
        user.AppendLine($"Number of days: {days}");
        user.AppendLine($"Preferences: {(string.IsNullOrWhiteSpace(request.Preferences) ? "none" : request.Preferences)}");
        user.AppendLine($"Daily calorie target: {(request.CalorieTarget.HasValue ? request.CalorieTarget + " kcal" : "no specific target")}");
        user.AppendLine($"Allergens to avoid: {(request.AvoidAllergens.Any() ? string.Join(", ", request.AvoidAllergens) : "none")}");
        user.AppendLine();
        user.AppendLine("Available dishes per day:");
        user.Append(sb);

        return await CallJsonAsync<WeeklyPlan>(system, user.ToString(), ct);
    }

    // ------------------------------------------------------------- KITCHEN ----
    public async Task<AiResult<KitchenInsight>> AnalyzeKitchenDayAsync(DateTime date, CancellationToken ct = default)
    {
        if (!IsConfigured) return AiResult<KitchenInsight>.Fail("AI is not configured.");

        var details = await _context.OrderDetails
            .Include(x => x.Dish).ThenInclude(d => d.DishIngredients).ThenInclude(di => di.Ingredient)
            .Include(x => x.Order)
            .Where(x => x.Order.TargetDate.Date == date.Date)
            .ToListAsync(ct);

        if (details.Count == 0)
            return AiResult<KitchenInsight>.Fail($"There are no orders for {date:dd.MM.yyyy}.");

        // Deterministic required totals (skip an ingredient if the note removes it).
        var required = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var notes = new List<string>();
        foreach (var d in details)
        {
            if (!string.IsNullOrWhiteSpace(d.Note))
                notes.Add($"{d.Quantity}x {d.Dish.DishName}: \"{d.Note}\"");

            foreach (var ing in d.Dish.DishIngredients)
            {
                if (d.Note != null && d.Note.ToLower().Contains(ing.Ingredient.IngredientName.ToLower()))
                    continue;
                required.TryGetValue(ing.Ingredient.IngredientName, out var cur);
                required[ing.Ingredient.IngredientName] = cur + ing.GramsPerPortion * d.Quantity;
            }
        }

        var stock = await _context.Ingredients
            .ToDictionaryAsync(i => i.IngredientName, i => i.AvailableGrams, StringComparer.OrdinalIgnoreCase, ct);

        var ctx = new StringBuilder();
        ctx.AppendLine($"Date: {date:dddd, dd MMM yyyy}");
        ctx.AppendLine($"Total order lines: {details.Count}");
        ctx.AppendLine();
        ctx.AppendLine("Required ingredients (after applying customer notes) vs available stock (grams):");
        foreach (var kv in required.OrderByDescending(k => k.Value))
        {
            stock.TryGetValue(kv.Key, out var avail);
            ctx.AppendLine($"  - {kv.Key}: need {kv.Value} g, available {avail} g");
        }
        ctx.AppendLine();
        ctx.AppendLine("Customer notes:");
        ctx.AppendLine(notes.Count > 0 ? string.Join("\n", notes.Select(n => "  - " + n)) : "  (none)");

        var system =
            "You are a kitchen operations assistant for the FIGusta canteen. " +
            "Given the required ingredients for all of a day's orders and the available stock, " +
            "decide whether the kitchen can prepare every order. Account for the customer notes " +
            "(e.g. a note removing an ingredient reduces what is needed; flag allergy-related notes). " +
            "Reply ONLY with a JSON object: " +
            "{\"verdict\": \"ok\" | \"shortage\", \"summary\": string, " +
            "\"shortages\": [{\"ingredient\": string, \"needed\": number, \"available\": number, \"deficit\": number}], " +
            "\"notes\": [string]}. Keep the summary short and actionable.";

        return await CallJsonAsync<KitchenInsight>(system, ctx.ToString(), ct);
    }

    // --------------------------------------------------------------- ADMIN ----
    public async Task<AiResult<PollSuggestion>> SuggestPollAsync(CancellationToken ct = default)
    {
        if (!IsConfigured) return AiResult<PollSuggestion>.Fail("AI is not configured.");

        var topDishes = await _context.OrderDetails
            .Include(od => od.Dish)
            .GroupBy(od => od.Dish.DishName)
            .Select(g => new { Dish = g.Key, Qty = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.Qty)
            .Take(10)
            .ToListAsync(ct);

        var comments = await _context.OrderDetails
            .Include(od => od.Dish)
            .Where(od => od.Note != null && od.Note != "")
            .OrderByDescending(od => od.Id)
            .Take(20)
            .Select(od => new { od.Dish.DishName, od.Note })
            .ToListAsync(ct);

        if (topDishes.Count == 0)
            return AiResult<PollSuggestion>.Fail("There is not enough order history yet to suggest a poll.");

        var ctx = new StringBuilder();
        ctx.AppendLine("Most ordered dishes (name: total quantity):");
        foreach (var t in topDishes)
            ctx.AppendLine($"  - {t.Dish}: {t.Qty}");
        ctx.AppendLine();
        ctx.AppendLine("Recent customer comments on orders:");
        ctx.AppendLine(comments.Count > 0
            ? string.Join("\n", comments.Select(c => $"  - [{c.DishName}] {c.Note}"))
            : "  (none)");

        var system =
            "You help a canteen administrator create engaging feedback polls for customers. " +
            "Using the most-ordered dishes and the recent customer comments, propose ONE poll " +
            "question with 3-5 concise answer options that will gather useful feedback " +
            "(e.g. new dishes to add, improvements to popular dishes). " +
            "Reply ONLY with a JSON object: {\"question\": string, \"options\": [string, ...]}.";

        return await CallJsonAsync<PollSuggestion>(system, ctx.ToString(), ct);
    }

    // ------------------------------------------------------------- CASHIER ----
    public async Task<AiResult<OrderHandover>> SummarizeOrderForPickupAsync(string code, CancellationToken ct = default)
    {
        if (!IsConfigured) return AiResult<OrderHandover>.Fail("AI is not configured.");

        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderDetails).ThenInclude(d => d.Dish).ThenInclude(di => di.DishAllergens).ThenInclude(da => da.Allergen)
            .FirstOrDefaultAsync(o => o.UniqueCode == code, ct);

        if (order == null)
            return AiResult<OrderHandover>.Fail("Order not found.");

        var blackPoints = order.User?.BlackPoints ?? 0;
        var reliability = blackPoints == 0
            ? "good (no missed pickups)"
            : $"{blackPoints} missed pickup(s) on record";

        var ctx = new StringBuilder();
        ctx.AppendLine($"Order code: {order.UniqueCode}");
        ctx.AppendLine($"Customer: {order.User?.FullName ?? order.User?.UserName}");
        ctx.AppendLine($"Customer reliability: {reliability}");
        ctx.AppendLine($"Status: {order.Status}; Total: {order.TotalPrice:0.00} EUR");
        ctx.AppendLine("Items:");
        foreach (var d in order.OrderDetails)
        {
            var allergens = d.Dish.DishAllergens?.Select(a => a.Allergen.AllergenName) ?? Enumerable.Empty<string>();
            var allergenStr = allergens.Any() ? $" (allergens: {string.Join(", ", allergens)})" : "";
            var note = string.IsNullOrWhiteSpace(d.Note) ? "" : $" — note: \"{d.Note}\"";
            ctx.AppendLine($"  - {d.Quantity}x {d.Dish.DishName}{allergenStr}{note}");
        }

        var system =
            "You are a friendly assistant for a canteen cashier handing an order to a customer. " +
            "Write a very short, warm handover summary the cashier can read while verifying the order, " +
            "and a list of short alerts to mention (allergens present, special preparation notes, and a " +
            "polite reliability reminder if the customer has missed pickups before). " +
            "Reply ONLY with a JSON object: {\"summary\": string, \"alerts\": [string]}.";

        return await CallJsonAsync<OrderHandover>(system, ctx.ToString(), ct);
    }

    // --------------------------------------------------------------- helpers --
    private async Task<AiResult<T>> CallJsonAsync<T>(string system, string user, CancellationToken ct)
    {
        try
        {
            var raw = await _client.CompleteAsync(system, user, jsonResponse: true, ct);
            var json = ExtractJson(raw);
            var data = JsonSerializer.Deserialize<T>(json, ParseOpts);
            if (data == null) return AiResult<T>.Fail("The AI returned an empty response.");
            return AiResult<T>.Success(data);
        }
        catch (TaskCanceledException)
        {
            return AiResult<T>.Fail("The AI request timed out. Please try again.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI assistant call failed.");
            return AiResult<T>.Fail("The AI service is temporarily unavailable.");
        }
    }

    // Defensive: strip ```json fences if the model adds them despite json mode.
    private static string ExtractJson(string raw)
    {
        var s = raw.Trim();
        if (s.StartsWith("```"))
        {
            var firstNewline = s.IndexOf('\n');
            if (firstNewline >= 0) s = s[(firstNewline + 1)..];
            if (s.EndsWith("```")) s = s[..^3];
        }
        return s.Trim();
    }
}
