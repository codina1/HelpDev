using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HelpDev.SharedContracts.Ai;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HelpDev.Infrastructure.Ai;

/// <summary>
/// Generic HTTP AI adapter. Posts JSON to configured Endpoint — no OpenAI/Claude SDK.
/// Expects response shape: { "text": "...", "model": "...", "usage": { "inputTokens": n, "outputTokens": n } }.
/// </summary>
public sealed class HttpAiTextGenerator : IAiTextGenerator
{
    private readonly HttpClient _httpClient;
    private readonly AiProviderOptions _options;
    private readonly ILogger<HttpAiTextGenerator> _logger;

    public HttpAiTextGenerator(
        HttpClient httpClient,
        IOptions<AiProviderOptions> options,
        ILogger<HttpAiTextGenerator> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public Task<AiGenerationResult> GenerateSafeAsync(
        AiTextRequest request,
        CancellationToken cancellationToken = default) =>
        AiTextGeneratorCompat.SafeFromAsync(ct => GenerateAsync(request, ct), "Http", cancellationToken);

    public async Task<AiTextResponse> GenerateAsync(
        AiTextRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_options.Enabled)
        {
            throw new InvalidOperationException("AI provider is disabled.");
        }

        if (string.IsNullOrWhiteSpace(_options.Endpoint))
        {
            throw new InvalidOperationException("AI endpoint is not configured.");
        }

        using var message = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint);
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }

        message.Content = JsonContent.Create(new
        {
            taskType = request.TaskType,
            systemInstruction = request.SystemInstruction,
            inputText = request.InputText,
            maxTokens = request.MaxTokens,
            model = _options.Model,
        });

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(message, cancellationToken);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("AI HTTP provider timed out for task {TaskType}", request.TaskType);
            throw new TimeoutException("AI provider timeout.");
        }
        catch (Exception ex)
        {
            // Never log request body, prompts, or API keys.
            _logger.LogWarning(ex, "AI HTTP provider call failed for task {TaskType}", request.TaskType);
            throw new InvalidOperationException("AI provider request failed.", ex);
        }

        if ((int)response.StatusCode is 401 or 403)
        {
            _logger.LogWarning(
                "AI HTTP provider unauthorized {StatusCode} for task {TaskType}",
                (int)response.StatusCode,
                request.TaskType);
            throw new InvalidOperationException("AI provider unauthorized.");
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "AI HTTP provider returned {StatusCode} for task {TaskType}",
                (int)response.StatusCode,
                request.TaskType);
            throw new InvalidOperationException("AI provider returned an error status.");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<HttpAiPayload>(
            stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            cancellationToken);

        if (payload is null || string.IsNullOrWhiteSpace(payload.Text))
        {
            throw new InvalidOperationException("AI provider returned an empty response.");
        }

        return new AiTextResponse(
            payload.Text.Trim(),
            string.IsNullOrWhiteSpace(payload.Model) ? _options.Model : payload.Model!,
            "Http",
            payload.Usage is null
                ? null
                : new AiTokenUsage(payload.Usage.InputTokens, payload.Usage.OutputTokens));
    }

    private sealed class HttpAiPayload
    {
        public string? Text { get; set; }
        public string? Model { get; set; }
        public HttpAiUsagePayload? Usage { get; set; }
    }

    private sealed class HttpAiUsagePayload
    {
        [JsonPropertyName("inputTokens")]
        public int InputTokens { get; set; }

        [JsonPropertyName("outputTokens")]
        public int OutputTokens { get; set; }
    }
}
