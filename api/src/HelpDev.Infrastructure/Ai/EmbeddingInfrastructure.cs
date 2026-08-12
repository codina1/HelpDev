using HelpDev.Modules.Search.Infrastructure.Persistence;
using HelpDev.SharedContracts.Ai;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HelpDev.Infrastructure.Ai;

public sealed class EmbeddingOptions
{
    public const string SectionName = "Embedding";

    public bool Enabled { get; set; } = true;

    /// <summary>Fake | Http</summary>
    public string ProviderName { get; set; } = "Fake";

    public string Model { get; set; } = "fake-embed-v1";

    public string? Endpoint { get; set; }

    public string? ApiKey { get; set; }

    public int Dimensions { get; set; } = SearchVectorConfiguration.DefaultDimensions;
}

public sealed class EmbeddingOptionsValidator : IValidateOptions<EmbeddingOptions>
{
    public ValidateOptionsResult Validate(string? name, EmbeddingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.Dimensions is < 8 or > 4096)
        {
            return ValidateOptionsResult.Fail("Embedding:Dimensions must be between 8 and 4096.");
        }

        if (options.Dimensions != SearchVectorConfiguration.DefaultDimensions)
        {
            return ValidateOptionsResult.Fail(
                $"Embedding:Dimensions must be {SearchVectorConfiguration.DefaultDimensions} to match search_vectors schema.");
        }

        var provider = (options.ProviderName ?? string.Empty).Trim();
        if (!string.Equals(provider, "Fake", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(provider, "Http", StringComparison.OrdinalIgnoreCase))
        {
            return ValidateOptionsResult.Fail("Embedding:ProviderName must be Fake or Http.");
        }

        if (options.Enabled
            && string.Equals(provider, "Http", StringComparison.OrdinalIgnoreCase)
            && (string.IsNullOrWhiteSpace(options.Endpoint)
                || !Uri.TryCreate(options.Endpoint.Trim(), UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)))
        {
            return ValidateOptionsResult.Fail("Embedding:Endpoint must be an absolute http(s) URL when ProviderName is Http.");
        }

        if (string.IsNullOrWhiteSpace(options.Model) || options.Model.Length > 100)
        {
            return ValidateOptionsResult.Fail("Embedding:Model is required and must be <= 100 characters.");
        }

        // Never include ApiKey in validation failure messages.
        return ValidateOptionsResult.Success;
    }
}

/// <summary>
/// Deterministic bag-of-tokens hasher for tests/dev. Not a production embedding model.
/// Similarity over these vectors is still computed by real pgvector cosine distance.
/// </summary>
public sealed class FakeEmbeddingGenerator : IEmbeddingGenerator
{
    private readonly EmbeddingOptions _options;

    public FakeEmbeddingGenerator(IOptions<EmbeddingOptions> options)
    {
        _options = options.Value;
    }

    public Task<EmbeddingResult> GenerateAsync(string text, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_options.Enabled)
        {
            throw new InvalidOperationException("Embedding provider is disabled.");
        }

        var vector = new float[_options.Dimensions];
        var tokens = Tokenize(text);
        foreach (var token in tokens)
        {
            var hash = StableHash(token);
            var index = (int)((uint)hash % (uint)_options.Dimensions);
            var sign = (hash & 1) == 0 ? 1f : -1f;
            vector[index] += sign;
        }

        Normalize(vector);
        return Task.FromResult(new EmbeddingResult(vector, vector.Length, _options.Model, "Fake"));
    }

    private static IEnumerable<string> Tokenize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            yield return "empty";
            yield break;
        }

        var parts = text.ToLowerInvariant()
            .Split([' ', '\n', '\t', '\r', '.', ',', ';', ':', '!', '?', '/', '\\', '-', '_', '(', ')', '[', ']', '{', '}', '"', '\''],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            yield return "empty";
            yield break;
        }

        foreach (var part in parts)
        {
            yield return part;
        }
    }

    private static int StableHash(string value)
    {
        unchecked
        {
            var hash = 23;
            foreach (var ch in value)
            {
                hash = (hash * 31) + ch;
            }

            return hash;
        }
    }

    private static void Normalize(float[] vector)
    {
        double sumSquares = 0;
        for (var i = 0; i < vector.Length; i++)
        {
            sumSquares += vector[i] * vector[i];
        }

        if (sumSquares <= 0)
        {
            vector[0] = 1f;
            return;
        }

        var norm = (float)Math.Sqrt(sumSquares);
        for (var i = 0; i < vector.Length; i++)
        {
            vector[i] /= norm;
        }
    }
}

/// <summary>
/// Generic HTTP embedding adapter. Expects JSON:
/// { "vector": [..], "dimensions": n, "model": "..." }
/// </summary>
public sealed class HttpEmbeddingGenerator : IEmbeddingGenerator
{
    private readonly HttpClient _httpClient;
    private readonly EmbeddingOptions _options;
    private readonly ILogger<HttpEmbeddingGenerator> _logger;

    public HttpEmbeddingGenerator(
        HttpClient httpClient,
        IOptions<EmbeddingOptions> options,
        ILogger<HttpEmbeddingGenerator> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<EmbeddingResult> GenerateAsync(string text, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            throw new InvalidOperationException("Embedding provider is disabled.");
        }

        if (string.IsNullOrWhiteSpace(_options.Endpoint))
        {
            throw new InvalidOperationException("Embedding endpoint is not configured.");
        }

        using var message = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint);
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            message.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }

        message.Content = System.Net.Http.Json.JsonContent.Create(new
        {
            text,
            model = _options.Model,
            dimensions = _options.Dimensions,
        });

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            // Never log request body or API keys.
            _logger.LogWarning(ex, "Embedding HTTP provider call failed.");
            throw new InvalidOperationException("Embedding provider request failed.", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Embedding HTTP provider returned {StatusCode}", (int)response.StatusCode);
            throw new InvalidOperationException("Embedding provider returned an error status.");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await System.Text.Json.JsonSerializer.DeserializeAsync<HttpEmbeddingPayload>(
            stream,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            cancellationToken);

        if (payload?.Vector is null || payload.Vector.Length != _options.Dimensions)
        {
            throw new InvalidOperationException("Embedding provider returned an invalid vector.");
        }

        return new EmbeddingResult(
            payload.Vector,
            payload.Vector.Length,
            string.IsNullOrWhiteSpace(payload.Model) ? _options.Model : payload.Model!,
            "Http");
    }

    private sealed class HttpEmbeddingPayload
    {
        public float[]? Vector { get; set; }

        public string? Model { get; set; }
    }
}
