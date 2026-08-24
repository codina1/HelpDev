using Asp.Versioning;
using HelpDev.API.OpenApi;
using HelpDev.Modules.Media.Application.Options;
using HelpDev.Modules.Media.Application.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HelpDev.API.Controllers;

/// <summary>
/// Public immutable media serving. Only files inside the configured media root are accessible.
/// No directory listing. Cache-Control: public, immutable (unique storage keys).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiAudience(ApiAudiences.Public)]
[Route("media")]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class PublicMediaController : ControllerBase
{
    private readonly IMediaStorage _storage;
    private readonly MediaOptions _options;

    public PublicMediaController(IMediaStorage storage, IOptions<MediaOptions> options)
    {
        _storage = storage;
        _options = options.Value;
    }

    [HttpGet("{year:int:min(2000):max(2100)}/{month:int:min(1):max(12)}/{fileName}")]
    [ResponseCache(Duration = 31536000, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Get(
        int year,
        int month,
        string fileName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fileName)
            || fileName.Contains("..", StringComparison.Ordinal)
            || fileName.Contains('/')
            || fileName.Contains('\\')
            || fileName.Contains(':'))
        {
            return NotFound();
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var contentType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => null,
        };

        if (contentType is null)
        {
            return NotFound();
        }

        var storageKey = $"{year:D4}/{month:D2}/{fileName}";
        if (!await _storage.ExistsAsync(storageKey, cancellationToken))
        {
            return NotFound();
        }

        var stream = await _storage.OpenReadAsync(storageKey, cancellationToken);
        Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        Response.Headers.XContentTypeOptions = "nosniff";
        // Media is intentionally embedded by the public site on a different
        // origin (helpdev.ir -> api.helpdev.ir).
        Response.Headers["Cross-Origin-Resource-Policy"] = "cross-origin";
        // inline for images; filename is server-generated so header injection risk is low,
        // but still strip quotes/CRLF defensively.
        var safeName = fileName.Replace("\"", string.Empty, StringComparison.Ordinal)
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Replace("\n", string.Empty, StringComparison.Ordinal);
        Response.Headers.ContentDisposition = $"inline; filename=\"{safeName}\"";

        return File(stream, contentType);
    }
}
