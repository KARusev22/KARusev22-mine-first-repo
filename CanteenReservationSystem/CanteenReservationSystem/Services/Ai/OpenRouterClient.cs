using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace CanteenReservationSystem.Services.Ai;

// Thin, dependency-light client around the OpenRouter chat-completions API.
// Uses a typed HttpClient (registered with AddHttpClient) so connection pooling
// and the configured timeout keep the AI call from ever blocking the app.
public class OpenRouterClient : IOpenRouterClient
{
    private readonly HttpClient _http;
    private readonly OpenRouterOptions _options;
    private readonly ILogger<OpenRouterClient> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public OpenRouterClient(HttpClient http, IOptions<OpenRouterOptions> options, ILogger<OpenRouterClient> logger)
    {
        _options = options.Value;
        _logger = logger;
        _http = http;
        _http.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.ApiKey);

    public async Task<string> CompleteAsync(
        string systemPrompt,
        string userPrompt,
        bool jsonResponse = false,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("OpenRouter API key is not configured.");

        var payload = new ChatRequest
        {
            Model = _options.Model,
            Temperature = 0.7,
            Messages = new[]
            {
                new ChatMessage { Role = "system", Content = systemPrompt },
                new ChatMessage { Role = "user", Content = userPrompt }
            },
            ResponseFormat = jsonResponse ? new ResponseFormat { Type = "json_object" } : null
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.BaseUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        // Optional attribution headers recommended by OpenRouter.
        request.Headers.TryAddWithoutValidation("HTTP-Referer", "https://figusta.local");
        request.Headers.TryAddWithoutValidation("X-Title", "FIGusta Canteen");
        request.Content = JsonContent.Create(payload, options: JsonOpts);

        using var response = await _http.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("OpenRouter request failed: {Status} {Body}", response.StatusCode, body);
            throw new HttpRequestException($"OpenRouter returned {(int)response.StatusCode}.");
        }

        using var doc = JsonDocument.Parse(body);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return content ?? string.Empty;
    }

    private sealed class ChatRequest
    {
        public string Model { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public ChatMessage[] Messages { get; set; } = Array.Empty<ChatMessage>();

        [JsonPropertyName("response_format")]
        public ResponseFormat? ResponseFormat { get; set; }
    }

    private sealed class ChatMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    private sealed class ResponseFormat
    {
        public string Type { get; set; } = "json_object";
    }
}
