namespace CanteenReservationSystem.Services.Ai;

public interface IOpenRouterClient
{
    // True when an API key is present, so callers can degrade gracefully
    // (show a hint) instead of failing when AI is not configured.
    bool IsConfigured { get; }

    // Sends a single-turn chat completion. When <paramref name="jsonResponse"/>
    // is true the model is asked to return a strict JSON object.
    Task<string> CompleteAsync(
        string systemPrompt,
        string userPrompt,
        bool jsonResponse = false,
        CancellationToken cancellationToken = default);
}
