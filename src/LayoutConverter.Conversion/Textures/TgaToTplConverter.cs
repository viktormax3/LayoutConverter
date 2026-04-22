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
            var additionalInfo = Nw4rTgaAdditionalInfoReader.Read(sourcePath);
            TplTextureWriter.WriteRaw(destinationPath, additionalInfo);
            return;
        }

        var image = TgaImageReader.Read(sourcePath);
        TplTextureWriter.Write(destinationPath, image, format);
    }
}
