namespace CanteenReservationSystem.Services.Ai;

// Generic result wrapper so the UI can always render something sensible,
// even when AI is unconfigured or the model returns an unexpected payload.
public class AiResult<T>
{
    public bool Ok { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }

    public static AiResult<T> Success(T data) => new() { Ok = true, Data = data };
    public static AiResult<T> Fail(string error) => new() { Ok = false, Error = error };
}

// ---- User: weekly menu planner ----
public class WeeklyMenuRequest
{
    public int Days { get; set; } = 5;
    public string? Preferences { get; set; }
    public int? CalorieTarget { get; set; }
    public List<string> AvoidAllergens { get; set; } = new();
}

public class WeeklyPlan
{
    public List<PlanDay> Days { get; set; } = new();
    public string? Summary { get; set; }
}

public class PlanDay
{
    public string Day { get; set; } = string.Empty;
    public List<PlanItem> Items { get; set; } = new();
    public int? TotalCalories { get; set; }
    public string? Note { get; set; }
}

public class PlanItem
{
    public string Dish { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Reason { get; set; }
}

// ---- Kitchen: stock / availability insight ----
public class KitchenInsight
{
    // "ok" | "shortage"
    public string Verdict { get; set; } = "ok";
    public string Summary { get; set; } = string.Empty;
    public List<Shortage> Shortages { get; set; } = new();
    public List<string> Notes { get; set; } = new();
}

public class Shortage
{
    public string Ingredient { get; set; } = string.Empty;
    public double Needed { get; set; }
    public double Available { get; set; }
    public double Deficit { get; set; }
}

// ---- Admin: poll suggestion ----
public class PollSuggestion
{
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
}

// ---- Cashier: pickup handover summary ----
public class OrderHandover
{
    public string Summary { get; set; } = string.Empty;
    // Short reminders to mention at the counter (allergens, special notes, reliability).
    public List<string> Alerts { get; set; } = new();
}
