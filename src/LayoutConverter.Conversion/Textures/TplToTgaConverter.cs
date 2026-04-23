namespace LayoutConverter.Conversion.Textures;

public static class TplToTgaConverter
{
    public static void Convert(string sourcePath, string destinationPath)
    {
        var image = TplTextureReader.Read(sourcePath);
        TgaImageWriter.Write(destinationPath, image);
    }
}
