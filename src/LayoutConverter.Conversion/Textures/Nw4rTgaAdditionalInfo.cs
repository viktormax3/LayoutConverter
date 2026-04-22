namespace LayoutConverter.Conversion.Textures;

internal sealed record Nw4rTgaAdditionalInfo(
    int Width,
    int Height,
    int TexelFormat,
    int MipmapCount,
    byte[] ImageData,
    int? PaletteFormat,
    byte[]? PaletteData);
