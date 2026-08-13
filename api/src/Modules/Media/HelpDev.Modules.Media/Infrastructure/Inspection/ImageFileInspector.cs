using HelpDev.Modules.Media.Application.Assets;
using HelpDev.Modules.Media.Application.Options;
using HelpDev.Modules.Media.Application.Validation;
using HelpDev.Modules.Media.Domain.ValueObjects;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;

namespace HelpDev.Modules.Media.Infrastructure.Inspection;

/// <summary>
/// Signature + decode inspection via SixLabors.ImageSharp 3.x (Apache-2.0 / Six Labors License).
/// Cross-platform; rejects SVG/HTML/PDF/executables and magic/format mismatches.
/// XML/SVG/HTML text hints are ignored once JPEG/PNG/WebP magic is present
/// (real images often embed XMP <c>&lt;?xml</c> in the first 512 bytes).
/// </summary>
public sealed class ImageFileInspector : IImageFileInspector
{
    private static readonly byte[] JpegMagic = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PngMagic = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] RiffMagic = [0x52, 0x49, 0x46, 0x46];
    private static readonly byte[] SvgHint = "<svg"u8.ToArray();
    private static readonly byte[] HtmlHint = "<html"u8.ToArray();
    private static readonly byte[] XmlHint = "<?xml"u8.ToArray();

    private readonly MediaOptions _options;

    public ImageFileInspector(IOptions<MediaOptions> options)
    {
        _options = options.Value;
    }

    public async Task<ImageInspectionResult> InspectAsync(
        Stream content,
        string? declaredContentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (!content.CanSeek)
        {
            throw new InvalidOperationException("Image inspection requires a seekable stream.");
        }

        content.Position = 0;
        RejectDangerousSignatures(content);
        content.Position = 0;

        var headerType = DetectByMagic(content);
        content.Position = 0;

        if (!string.IsNullOrWhiteSpace(declaredContentType))
        {
            var declared = declaredContentType.Trim().ToLowerInvariant();
            if (declared is "image/svg+xml" or "text/html" or "application/xml" or "text/xml"
                or "application/javascript" or "text/javascript" or "application/pdf")
            {
                throw new MediaException("نوع فایل پشتیبانی نمی‌شود.", MediaErrorCodes.UnsupportedType);
            }
        }

        ImageInfo info;
        try
        {
            info = await Image.IdentifyAsync(content, cancellationToken).ConfigureAwait(false);
        }
        catch (UnknownImageFormatException ex)
        {
            throw new MediaException("فرمت تصویر شناخته نشد.", MediaErrorCodes.UnsupportedType, ex);
        }
        catch (InvalidImageContentException ex)
        {
            throw new MediaException("تصویر خراب یا نامعتبر است.", MediaErrorCodes.UnsupportedType, ex);
        }

        if (info.Width <= 0 || info.Height <= 0)
        {
            throw new MediaException("ابعاد تصویر نامعتبر است.", MediaErrorCodes.Validation);
        }

        if (info.Width > _options.MaxWidth || info.Height > _options.MaxHeight)
        {
            throw new MediaException(
                $"ابعاد تصویر از حد مجاز ({_options.MaxWidth}×{_options.MaxHeight}) بیشتر است.",
                MediaErrorCodes.Validation);
        }

        var detected = MapFormat(info.Metadata.DecodedImageFormat)
            ?? headerType
            ?? throw new MediaException("نوع تصویر پشتیبانی نمی‌شود.", MediaErrorCodes.UnsupportedType);

        if (headerType is not null && !string.Equals(headerType, detected, StringComparison.OrdinalIgnoreCase))
        {
            throw new MediaException(
                "امضای فایل با نوع تصویر هم‌خوانی ندارد.",
                MediaErrorCodes.UnsupportedType);
        }

        if (!MediaContentType.Allowed.Contains(detected))
        {
            throw new MediaException("نوع تصویر پشتیبانی نمی‌شود.", MediaErrorCodes.UnsupportedType);
        }

        var contentType = MediaContentType.Create(detected);
        content.Position = 0;
        return new ImageInspectionResult(contentType.Value, info.Width, info.Height, contentType.SafeExtension);
    }

    private static string? MapFormat(IImageFormat? format) => format switch
    {
        JpegFormat => MediaContentType.Jpeg,
        PngFormat => MediaContentType.Png,
        WebpFormat => MediaContentType.Webp,
        _ => null,
    };

    private static string? DetectByMagic(Stream content)
    {
        Span<byte> buffer = stackalloc byte[16];
        var read = content.Read(buffer);
        return DetectByMagic(buffer[..read]);
    }

    private static string? DetectByMagic(ReadOnlySpan<byte> header)
    {
        if (header.Length < 3)
        {
            return null;
        }

        if (StartsWith(header, JpegMagic))
        {
            return MediaContentType.Jpeg;
        }

        if (header.Length >= 8 && StartsWith(header, PngMagic))
        {
            return MediaContentType.Png;
        }

        if (header.Length >= 12
            && StartsWith(header, RiffMagic)
            && header[8] == (byte)'W'
            && header[9] == (byte)'E'
            && header[10] == (byte)'B'
            && header[11] == (byte)'P')
        {
            return MediaContentType.Webp;
        }

        return null;
    }

    private static void RejectDangerousSignatures(Stream content)
    {
        Span<byte> buffer = stackalloc byte[512];
        var read = content.Read(buffer);
        if (read == 0)
        {
            throw new MediaException("فایل خالی است.", MediaErrorCodes.Validation);
        }

        var slice = buffer[..read];

        // JPEG/PNG/WebP often embed XMP (`<?xml`) or similar text in early
        // metadata chunks. That is not a disguised SVG/HTML/XML document.
        if (DetectByMagic(slice) is not null)
        {
            return;
        }

        if (ContainsIgnoreCaseAscii(slice, SvgHint)
            || ContainsIgnoreCaseAscii(slice, HtmlHint)
            || ContainsIgnoreCaseAscii(slice, XmlHint)
            || (read >= 2 && slice[0] == (byte)'M' && slice[1] == (byte)'Z')
            || (read >= 4 && slice[0] == 0x25 && slice[1] == 0x50 && slice[2] == 0x44 && slice[3] == 0x46))
        {
            throw new MediaException("نوع فایل پشتیبانی نمی‌شود.", MediaErrorCodes.UnsupportedType);
        }
    }

    private static bool StartsWith(ReadOnlySpan<byte> data, ReadOnlySpan<byte> magic) =>
        data.Length >= magic.Length && data[..magic.Length].SequenceEqual(magic);

    private static bool ContainsIgnoreCaseAscii(ReadOnlySpan<byte> haystack, ReadOnlySpan<byte> needle)
    {
        if (needle.Length == 0 || haystack.Length < needle.Length)
        {
            return false;
        }

        for (var i = 0; i <= haystack.Length - needle.Length; i++)
        {
            var match = true;
            for (var j = 0; j < needle.Length; j++)
            {
                var a = ToLowerAscii(haystack[i + j]);
                var b = ToLowerAscii(needle[j]);
                if (a != b)
                {
                    match = false;
                    break;
                }
            }

            if (match)
            {
                return true;
            }
        }

        return false;
    }

    private static byte ToLowerAscii(byte value) =>
        value is >= (byte)'A' and <= (byte)'Z' ? (byte)(value + 32) : value;
}
