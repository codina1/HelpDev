using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;

namespace HelpDev.Media.Tests;

public static class ImageTestFixtures
{
    public static byte[] CreatePngBytes(int width = 2, int height = 2)
    {
        using var image = new Image<Rgba32>(width, height);
        image[0, 0] = new Rgba32(255, 0, 0, 255);
        using var ms = new MemoryStream();
        image.Save(ms, new PngEncoder());
        return ms.ToArray();
    }

    public static byte[] CreateJpegBytes(int width = 2, int height = 2)
    {
        using var image = new Image<Rgba32>(width, height);
        using var ms = new MemoryStream();
        image.Save(ms, new JpegEncoder());
        return ms.ToArray();
    }

    public static byte[] CreateWebpBytes(int width = 2, int height = 2)
    {
        using var image = new Image<Rgba32>(width, height);
        using var ms = new MemoryStream();
        image.Save(ms, new WebpEncoder());
        return ms.ToArray();
    }

    public static byte[] FakeJpegExtensionBytes() =>
        [0xFF, 0xD8, 0xFF, 0x00, 0x00, 0x00, 0x00];

    public static byte[] SvgBytes() => "<svg xmlns=\"http://www.w3.org/2000/svg\"></svg>"u8.ToArray();

    /// <summary>
    /// Valid PNG with an early tEXt chunk containing <c>&lt;?xml</c> (XMP-style),
    /// which used to be rejected as a disguised XML/SVG file.
    /// </summary>
    public static byte[] CreatePngBytesWithXmlMetadata()
    {
        var png = CreatePngBytes();
        if (png.Length < 33)
        {
            throw new InvalidOperationException("Unexpected PNG fixture size.");
        }

        var ihdrDataLength = (png[8] << 24) | (png[9] << 16) | (png[10] << 8) | png[11];
        var afterIhdr = 8 + 4 + 4 + ihdrDataLength + 4;

        var payload = "Comment\0<?xml version=\"1.0\"?><x:xmpmeta xmlns:x=\"adobe:ns:meta/\"/>"u8.ToArray();
        var chunkType = "tEXt"u8.ToArray();
        var crcInput = new byte[chunkType.Length + payload.Length];
        chunkType.CopyTo(crcInput, 0);
        payload.CopyTo(crcInput, chunkType.Length);

        using var ms = new MemoryStream();
        ms.Write(png, 0, afterIhdr);
        WriteBe32(ms, payload.Length);
        ms.Write(chunkType);
        ms.Write(payload);
        WriteBe32(ms, unchecked((int)PngCrc32(crcInput)));
        ms.Write(png, afterIhdr, png.Length - afterIhdr);
        return ms.ToArray();
    }

    private static void WriteBe32(Stream stream, int value)
    {
        stream.WriteByte((byte)(value >> 24));
        stream.WriteByte((byte)(value >> 16));
        stream.WriteByte((byte)(value >> 8));
        stream.WriteByte((byte)value);
    }

    private static uint PngCrc32(ReadOnlySpan<byte> data)
    {
        var crc = 0xFFFFFFFFu;
        foreach (var b in data)
        {
            crc ^= b;
            for (var i = 0; i < 8; i++)
            {
                var mask = (crc & 1u) != 0 ? 0xEDB88320u : 0u;
                crc = (crc >> 1) ^ mask;
            }
        }

        return crc ^ 0xFFFFFFFFu;
    }
}
