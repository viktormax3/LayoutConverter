using LayoutConverter.Core.Schema.Rlyt;

namespace LayoutConverter.Conversion.Textures;

public static class TgaToTplConverter
{
    public static void Convert(string sourcePath, string destinationPath, TexelFormat format)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);

        if (format == TexelFormat.NW4R_TGA)
        {
            throw new NotSupportedException(
                "NW4R_TGA requires parsing NW4R_Tga additional chunks and direct GX payload passthrough; this path is not implemented yet.");
        }

        var image = TgaImageReader.Read(sourcePath);
        TplTextureWriter.Write(destinationPath, image, format);
    }
}
