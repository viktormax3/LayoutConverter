namespace LayoutConverter.Conversion.Textures;

internal sealed class TgaImage
{
    public TgaImage(int width, int height, Rgba32[] pixels)
    {
        Width = width;
        Height = height;
        Pixels = pixels;
    }

    public int Width { get; }
    public int Height { get; }
    public Rgba32[] Pixels { get; }

    public Rgba32 GetPixel(int x, int y)
        => x >= Width || y >= Height ? default : Pixels[(y * Width) + x];
}

internal readonly record struct Rgba32(byte R, byte G, byte B, byte A)
{
    public int Intensity => (R + G + B) / 3;
}
