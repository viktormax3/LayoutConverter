namespace LayoutConverter.Conversion.Textures;

internal static class TgaImageWriter
{
    public static void Write(string destinationPath, TgaImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(destinationPath))!);

        using var output = File.Create(destinationPath);
        Span<byte> header = stackalloc byte[18];
        header[2] = 2; // Uncompressed truecolor.
        WriteUInt16LittleEndian(header, 12, checked((ushort)image.Width));
        WriteUInt16LittleEndian(header, 14, checked((ushort)image.Height));
        header[16] = 32;
        header[17] = 0x28; // 8 alpha bits, top-left origin.
        output.Write(header);

        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                var pixel = image.GetPixel(x, y);
                output.WriteByte(pixel.B);
                output.WriteByte(pixel.G);
                output.WriteByte(pixel.R);
                output.WriteByte(pixel.A);
            }
        }
    }

    private static void WriteUInt16LittleEndian(Span<byte> buffer, int offset, ushort value)
    {
        buffer[offset] = (byte)value;
        buffer[offset + 1] = (byte)(value >> 8);
    }
}
