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
}
