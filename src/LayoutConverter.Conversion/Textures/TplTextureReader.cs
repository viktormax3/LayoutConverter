using System.Buffers.Binary;

namespace LayoutConverter.Conversion.Textures;

internal static class TplTextureReader
{
    private const uint Magic = 0x0020AF30;

    public static TgaImage Read(string path)
    {
        var bytes = File.ReadAllBytes(path);
        if (bytes.Length < 0x20)
        {
            throw new InvalidDataException($"Invalid TPL header: {path}");
        }

        uint magic = ReadUInt32(bytes, 0);
        if (magic != Magic)
        {
            throw new InvalidDataException($"Invalid TPL magic: {path}");
        }

        uint imageCount = ReadUInt32(bytes, 4);
        if (imageCount != 1)
        {
            throw new NotSupportedException($"Only single-image TPL files are supported: {path}");
        }

        int tableOffset = checked((int)ReadUInt32(bytes, 8));
        EnsureRange(bytes, tableOffset, 8, path);

        int imageHeaderOffset = checked((int)ReadUInt32(bytes, tableOffset));
        EnsureRange(bytes, imageHeaderOffset, 36, path);

        int height = ReadUInt16(bytes, imageHeaderOffset);
        int width = ReadUInt16(bytes, imageHeaderOffset + 2);
        int format = checked((int)ReadUInt32(bytes, imageHeaderOffset + 4));
        int imageDataOffset = checked((int)ReadUInt32(bytes, imageHeaderOffset + 8));

        if (width <= 0 || height <= 0 || width > 1024 || height > 1024)
        {
            throw new InvalidDataException($"Invalid TPL dimensions in {path}.");
        }

        EnsureRange(bytes, imageDataOffset, 1, path);
        return DecodeImage(bytes, imageDataOffset, width, height, format, path);
    }

    private static TgaImage DecodeImage(byte[] bytes, int dataOffset, int width, int height, int format, string path)
    {
        var pixels = new Rgba32[checked(width * height)];
        int offset = dataOffset;

        switch (format)
        {
        case 0:
            ForBlocks(width, height, 8, 8, (x, y) =>
            {
                byte packed = ReadByte(bytes, ref offset, path);
                SetPixel(pixels, width, height, x, y, FromIntensity(Expand4(packed >> 4)));
                SetPixel(pixels, width, height, x + 1, y, FromIntensity(Expand4(packed & 0x0F)));
            }, stepX: 2);
            break;

        case 1:
            ForBlocks(width, height, 8, 4, (x, y) =>
                SetPixel(pixels, width, height, x, y, FromIntensity(ReadByte(bytes, ref offset, path))));
            break;

        case 2:
            ForBlocks(width, height, 8, 4, (x, y) =>
            {
                byte packed = ReadByte(bytes, ref offset, path);
                SetPixel(pixels, width, height, x, y, new Rgba32(Expand4(packed & 0x0F), Expand4(packed & 0x0F), Expand4(packed & 0x0F), Expand4(packed >> 4)));
            });
            break;

        case 3:
            ForBlocks(width, height, 4, 4, (x, y) =>
            {
                ushort packed = ReadUInt16(bytes, ref offset, path);
                byte alpha = (byte)(packed >> 8);
                byte intensity = (byte)packed;
                SetPixel(pixels, width, height, x, y, new Rgba32(intensity, intensity, intensity, alpha));
            });
            break;

        case 4:
            ForBlocks(width, height, 4, 4, (x, y) =>
                SetPixel(pixels, width, height, x, y, FromRgb565(ReadUInt16(bytes, ref offset, path))));
            break;

        case 5:
            ForBlocks(width, height, 4, 4, (x, y) =>
                SetPixel(pixels, width, height, x, y, FromRgb5A3(ReadUInt16(bytes, ref offset, path))));
            break;

        case 6:
            DecodeRgba8(bytes, ref offset, width, height, pixels, path);
            break;

        default:
            throw new NotSupportedException($"Unsupported TPL texel format {format}: {path}");
        }

        return new TgaImage(width, height, pixels);
    }

    private static void DecodeRgba8(byte[] bytes, ref int offset, int width, int height, Rgba32[] pixels, string path)
    {
        int paddedWidth = PaddedSize(width, 4);
        int paddedHeight = PaddedSize(height, 4);
        Span<byte> alphaRed = stackalloc byte[32];
        Span<byte> greenBlue = stackalloc byte[32];

        for (int blockY = 0; blockY < paddedHeight; blockY += 4)
        {
            for (int blockX = 0; blockX < paddedWidth; blockX += 4)
            {
                ReadBlock(bytes, ref offset, alphaRed, path);
                ReadBlock(bytes, ref offset, greenBlue, path);

                for (int y = 0; y < 4; y++)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        int pixelOffset = (y * 4 + x) * 2;
                        SetPixel(
                            pixels,
                            width,
                            height,
                            blockX + x,
                            blockY + y,
                            new Rgba32(
                                alphaRed[pixelOffset + 1],
                                greenBlue[pixelOffset],
                                greenBlue[pixelOffset + 1],
                                alphaRed[pixelOffset]));
                    }
                }
            }
        }
    }

    private static void ForBlocks(int width, int height, int blockWidth, int blockHeight, Action<int, int> read, int stepX = 1)
    {
        int paddedWidth = PaddedSize(width, blockWidth);
        int paddedHeight = PaddedSize(height, blockHeight);
        for (int blockY = 0; blockY < paddedHeight; blockY += blockHeight)
        {
            for (int blockX = 0; blockX < paddedWidth; blockX += blockWidth)
            {
                for (int y = 0; y < blockHeight; y++)
                {
                    for (int x = 0; x < blockWidth; x += stepX)
                    {
                        read(blockX + x, blockY + y);
                    }
                }
            }
        }
    }

    private static void SetPixel(Rgba32[] pixels, int width, int height, int x, int y, Rgba32 pixel)
    {
        if ((uint)x >= (uint)width || (uint)y >= (uint)height)
        {
            return;
        }

        pixels[y * width + x] = pixel;
    }

    private static Rgba32 FromIntensity(int value)
        => new((byte)value, (byte)value, (byte)value, 255);

    private static Rgba32 FromRgb565(ushort value)
        => new(
            Expand5((value >> 11) & 0x1F),
            Expand6((value >> 5) & 0x3F),
            Expand5(value & 0x1F),
            255);

    private static Rgba32 FromRgb5A3(ushort value)
    {
        if ((value & 0x8000) != 0)
        {
            return new Rgba32(
                Expand5((value >> 10) & 0x1F),
                Expand5((value >> 5) & 0x1F),
                Expand5(value & 0x1F),
                255);
        }

        return new Rgba32(
            Expand4((value >> 8) & 0x0F),
            Expand4((value >> 4) & 0x0F),
            Expand4(value & 0x0F),
            Expand3((value >> 12) & 0x07));
    }

    private static byte Expand3(int value)
        => (byte)((value << 5) | (value << 2) | (value >> 1));

    private static byte Expand4(int value)
        => (byte)((value << 4) | value);

    private static byte Expand5(int value)
        => (byte)((value << 3) | (value >> 2));

    private static byte Expand6(int value)
        => (byte)((value << 2) | (value >> 4));

    private static int PaddedSize(int value, int blockSize)
        => (value + blockSize - 1) / blockSize * blockSize;

    private static byte ReadByte(byte[] bytes, ref int offset, string path)
    {
        EnsureRange(bytes, offset, 1, path);
        return bytes[offset++];
    }

    private static ushort ReadUInt16(byte[] bytes, ref int offset, string path)
    {
        EnsureRange(bytes, offset, 2, path);
        ushort value = BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(offset, 2));
        offset += 2;
        return value;
    }

    private static void ReadBlock(byte[] bytes, ref int offset, Span<byte> destination, string path)
    {
        EnsureRange(bytes, offset, destination.Length, path);
        bytes.AsSpan(offset, destination.Length).CopyTo(destination);
        offset += destination.Length;
    }

    private static ushort ReadUInt16(byte[] bytes, int offset)
        => BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(offset, 2));

    private static uint ReadUInt32(byte[] bytes, int offset)
        => BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(offset, 4));

    private static void EnsureRange(byte[] bytes, int offset, int count, string path)
    {
        if (offset < 0 || count < 0 || offset > bytes.Length - count)
        {
            throw new EndOfStreamException($"Unexpected end of TPL file: {path}");
        }
    }
}
