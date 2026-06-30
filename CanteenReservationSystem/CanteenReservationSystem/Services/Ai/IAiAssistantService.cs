namespace CanteenReservationSystem.Services.Ai;

// High-level, domain-specific AI helpers. Each method gathers the relevant
// context from the database, asks the model, and returns a typed result.
public interface IAiAssistantService
{
    bool IsConfigured { get; }

    // USER: build a weekly meal plan from the dishes actually available that week.
    Task<AiResult<WeeklyPlan>> BuildWeeklyMenuAsync(WeeklyMenuRequest request, CancellationToken ct = default);

    // KITCHEN: check whether available stock covers every order for a given day,
    // taking customer notes into account.
    Task<AiResult<KitchenInsight>> AnalyzeKitchenDayAsync(DateTime date, CancellationToken ct = default);

    // ADMIN: suggest a poll question + options from the most-ordered dishes and
    // the comments customers left on their orders.
    Task<AiResult<PollSuggestion>> SuggestPollAsync(CancellationToken ct = default);

    // CASHIER: produce a concise pickup handover summary for an order, highlighting
    // allergens, special notes and customer reliability (black points).
    Task<AiResult<OrderHandover>> SummarizeOrderForPickupAsync(string code, CancellationToken ct = default);
}
