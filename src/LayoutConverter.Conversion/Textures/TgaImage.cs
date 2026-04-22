namespace LayoutConverter.Conversion.Textures;

internal sealed class TgaImage
{
    public TgaImage(int width, int height, Rgba32[] pixels)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Image dimensions must be positive.");
        }

        if (pixels.Length != width * height)
        {
            throw new ArgumentException("Pixel buffer length does not match image dimensions.", nameof(pixels));
        }

        Width = width;
        Height = height;
        Pixels = pixels;
    }

    public int Width { get; }
    public int Height { get; }
    public Rgba32[] Pixels { get; }

    public Rgba32 GetPixel(int x, int y)
        => x < 0 || y < 0 || x >= Width || y >= Height ? default : Pixels[(y * Width) + x];
}

internal readonly record struct Rgba32(byte R, byte G, byte B, byte A)
{
    public int Intensity => (R + G + B) / 3;
}
