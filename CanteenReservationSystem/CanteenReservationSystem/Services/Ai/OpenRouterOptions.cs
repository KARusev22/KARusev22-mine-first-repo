namespace CanteenReservationSystem.Services.Ai;

// Strongly-typed configuration for the OpenRouter AI integration.
// The API key is intentionally NOT stored in source control. Provide it through
// one of the following (any standard ASP.NET Core configuration source):
//   - Environment variable:  OpenRouter__ApiKey=<your-openrouter-key>
//   - User secrets (dev):     dotnet user-secrets set "OpenRouter:ApiKey" "<your-openrouter-key>"
//   - appsettings overrides that are kept out of git
public class OpenRouterOptions
{
    public const string SectionName = "OpenRouter";

    public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1/chat/completions";

    public string Model { get; set; } = "openai/gpt-4o-mini";

    public string ApiKey { get; set; } = string.Empty;

    // Keep AI calls bounded so they can never hang a request handler.
    public int TimeoutSeconds { get; set; } = 45;
}
