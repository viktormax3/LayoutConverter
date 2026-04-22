using System.Buffers.Binary;

namespace LayoutConverter.Conversion.Textures;

internal static class TgaImageReader
{
    public static TgaImage Read(string path)
    {
        var bytes = File.ReadAllBytes(path);
        if (bytes.Length < 18)
        {
            throw new InvalidDataException($"Invalid TGA header: {path}");
        }

        int idLength = bytes[0];
        int colorMapType = bytes[1];
        int imageType = bytes[2];
        int colorMapStart = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(3));
        int colorMapLength = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(5));
        int colorMapDepth = bytes[7];
        int width = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(12));
        int height = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(14));
        int pixelDepth = bytes[16];
        byte descriptor = bytes[17];

        if (width <= 0 || height <= 0 || width > 1024 || height > 1024)
        {
            throw new InvalidDataException($"Invalid TGA dimensions in {path}.");
        }

        int offset = 18 + idLength;
        EnsureAvailable(bytes, offset, 0, path);
        var palette = ReadPalette(bytes, ref offset, colorMapType, colorMapStart, colorMapLength, colorMapDepth, path);
        var pixels = new Rgba32[width * height];
        bool rle = imageType is 9 or 10 or 11;
        int rawType = rle ? imageType - 8 : imageType;
        bool originTop = (descriptor & 0x20) != 0;
        bool originRight = (descriptor & 0x10) != 0;

        if (rawType is not (1 or 2 or 3))
        {
            throw new NotSupportedException($"Unsupported TGA image type {imageType} in {path}.");
        }

        int pixelIndex = 0;
        while (pixelIndex < pixels.Length)
        {
            int count = 1;
            bool repeat = false;
            if (rle)
            {
                EnsureAvailable(bytes, offset, 1, path);
                byte packet = bytes[offset++];
                repeat = (packet & 0x80) != 0;
                count = (packet & 0x7F) + 1;
            }

            var pixel = ReadPixel(bytes, ref offset, rawType, pixelDepth, palette, path);
            for (int i = 0; i < count && pixelIndex < pixels.Length; i++)
            {
                if (!repeat && i > 0)
                {
                    pixel = ReadPixel(bytes, ref offset, rawType, pixelDepth, palette, path);
                }

                int sourceX = pixelIndex % width;
                int sourceY = pixelIndex / width;
                int x = originRight ? width - 1 - sourceX : sourceX;
                int y = originTop ? sourceY : height - 1 - sourceY;
                pixels[(y * width) + x] = pixel;
                pixelIndex++;
            }
        }

        return new TgaImage(width, height, pixels);
    }

    private static Rgba32[]? ReadPalette(
        byte[] bytes,
        ref int offset,
        int colorMapType,
        int colorMapStart,
        int colorMapLength,
        int colorMapDepth,
        string path)
    {
        if (colorMapType == 0)
        {
            return null;
        }

        if (colorMapType != 1)
        {
            throw new NotSupportedException($"Unsupported TGA color map type {colorMapType} in {path}.");
        }

        int entryBytes = (colorMapDepth + 7) / 8;
        if (entryBytes is not (2 or 3 or 4))
        {
            throw new NotSupportedException($"Unsupported TGA palette depth {colorMapDepth} in {path}.");
        }

        var palette = new Rgba32[colorMapStart + colorMapLength];
        for (int i = 0; i < colorMapLength; i++)
        {
            palette[colorMapStart + i] = ReadColor(bytes, ref offset, entryBytes, path);
        }

        return palette;
    }

    private static Rgba32 ReadPixel(
        byte[] bytes,
        ref int offset,
        int imageType,
        int pixelDepth,
        Rgba32[]? palette,
        string path)
    {
        return imageType switch
        {
            1 => ReadPalettePixel(bytes, ref offset, pixelDepth, palette, path),
            2 => ReadColor(bytes, ref offset, (pixelDepth + 7) / 8, path),
            3 => ReadGray(bytes, ref offset, pixelDepth, path),
            _ => throw new NotSupportedException($"Unsupported TGA image type {imageType} in {path}."),
        };
    }

    private static Rgba32 ReadPalettePixel(byte[] bytes, ref int offset, int pixelDepth, Rgba32[]? palette, string path)
    {
        if (palette is null)
        {
            throw new InvalidDataException($"TGA color-mapped image has no palette: {path}");
        }

        int index = pixelDepth switch
        {
            8 => ReadByte(bytes, ref offset, path),
            16 => ReadUInt16(bytes, ref offset, path),
            _ => throw new NotSupportedException($"Unsupported TGA palette index depth {pixelDepth} in {path}."),
        };

        if (index < 0 || index >= palette.Length)
        {
            throw new InvalidDataException($"TGA palette index {index} is out of range in {path}.");
        }

        return palette[index];
    }

    private static Rgba32 ReadGray(byte[] bytes, ref int offset, int pixelDepth, string path)
    {
        if (pixelDepth == 8)
        {
            EnsureAvailable(bytes, offset, 1, path);
            byte value = bytes[offset++];
            return new Rgba32(value, value, value, 255);
        }

        if (pixelDepth == 16)
        {
            EnsureAvailable(bytes, offset, 2, path);
            byte value = bytes[offset++];
            byte alpha = bytes[offset++];
            return new Rgba32(value, value, value, alpha);
        }

        throw new NotSupportedException($"Unsupported TGA grayscale depth {pixelDepth} in {path}.");
    }

    private static Rgba32 ReadColor(byte[] bytes, ref int offset, int byteCount, string path)
    {
        if (byteCount is not (2 or 3 or 4))
        {
            throw new NotSupportedException($"Unsupported TGA color depth {byteCount * 8} in {path}.");
        }

        EnsureAvailable(bytes, offset, byteCount, path);

        if (byteCount == 2)
        {
            ushort value = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset));
            offset += 2;
            byte b = Expand5(value & 0x1F);
            byte g = Expand5((value >> 5) & 0x1F);
            byte r = Expand5((value >> 10) & 0x1F);
            byte a = (value & 0x8000) != 0 ? (byte)255 : (byte)0;
            return new Rgba32(r, g, b, a);
        }

        byte blue = bytes[offset++];
        byte green = bytes[offset++];
        byte red = bytes[offset++];
        byte alpha = byteCount == 4 ? bytes[offset++] : (byte)255;
        return new Rgba32(red, green, blue, alpha);
    }

    private static byte ReadByte(byte[] bytes, ref int offset, string path)
    {
        EnsureAvailable(bytes, offset, 1, path);
        return bytes[offset++];
    }

    private static ushort ReadUInt16(byte[] bytes, ref int offset, string path)
    {
        EnsureAvailable(bytes, offset, 2, path);
        ushort value = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset));
        offset += 2;
        return value;
    }

    private static byte Expand5(int value)
        => (byte)((value << 3) | (value >> 2));

    private static void EnsureAvailable(byte[] bytes, int offset, int count, string path)
    {
        if (offset + count > bytes.Length)
        {
            throw new EndOfStreamException($"Unexpected end of TGA file: {path}");
        }
    }
}
