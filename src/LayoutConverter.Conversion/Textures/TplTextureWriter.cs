using System.Buffers.Binary;
using LayoutConverter.Core.Schema.Rlyt;

namespace LayoutConverter.Conversion.Textures;

internal static class TplTextureWriter
{
    private const uint Magic = 0x0020AF30;
    private const int ImageTableOffset = 0x0C;
    private const int ImageHeaderOffset = 0x14;
    private const int ImageDataOffset = 0x40;

    public static void Write(string destinationPath, TgaImage image, TexelFormat format)
    {
        var imageData = EncodeImageData(image, format);

        using var output = File.Create(destinationPath);
        Span<byte> header = stackalloc byte[ImageDataOffset];
        WriteUInt32(header, 0x00, Magic);
        WriteUInt32(header, 0x04, 1);
        WriteUInt32(header, 0x08, ImageTableOffset);
        WriteUInt32(header, 0x0C, ImageHeaderOffset);
        WriteUInt32(header, 0x10, 0);

        WriteUInt16(header, 0x14, (ushort)image.Height);
        WriteUInt16(header, 0x16, (ushort)image.Width);
        WriteUInt32(header, 0x18, MapFormat(format));
        WriteUInt32(header, 0x1C, ImageDataOffset);
        WriteUInt32(header, 0x20, 0);
        WriteUInt32(header, 0x24, 0);
        WriteUInt32(header, 0x28, 1);
        WriteUInt32(header, 0x2C, 1);

        output.Write(header);
        output.Write(imageData);
    }

    private static byte[] EncodeImageData(TgaImage image, TexelFormat format)
    {
        return format switch
        {
            TexelFormat.I4 => EncodeI4(image),
            TexelFormat.I8 => EncodeI8(image),
            TexelFormat.IA4 => EncodeIA4(image),
            TexelFormat.IA8 => EncodeIA8(image),
            TexelFormat.RGB565 => EncodeRgb565(image),
            TexelFormat.RGB5A3 => EncodeRgb5A3(image),
            TexelFormat.RGBA8 => EncodeRgba8(image),
            TexelFormat.CMPR => throw new NotSupportedException("CMPR texture encoding is not implemented yet."),
            TexelFormat.NW4R_TGA => throw new NotSupportedException("NW4R_TGA passthrough requires NW4R TGA additional information and is not implemented yet."),
            _ => throw new NotSupportedException($"Unsupported texel format: {format}."),
        };
    }

    private static byte[] EncodeI4(TgaImage image)
    {
        var output = new byte[BlockCount(image.Width, 8) * BlockCount(image.Height, 8) * 32];
        int offset = 0;
        ForBlocks(image, 8, 8, (x, y) =>
        {
            byte hi = Quantize4(image.GetPixel(x, y).Intensity);
            byte lo = Quantize4(image.GetPixel(x + 1, y).Intensity);
            output[offset++] = (byte)((hi << 4) | lo);
        }, stepX: 2);
        return output;
    }

    private static byte[] EncodeI8(TgaImage image)
    {
        var output = new byte[BlockCount(image.Width, 8) * BlockCount(image.Height, 4) * 32];
        int offset = 0;
        ForBlocks(image, 8, 4, (x, y) => output[offset++] = (byte)image.GetPixel(x, y).Intensity);
        return output;
    }

    private static byte[] EncodeIA4(TgaImage image)
    {
        var output = new byte[BlockCount(image.Width, 8) * BlockCount(image.Height, 4) * 32];
        int offset = 0;
        ForBlocks(image, 8, 4, (x, y) =>
        {
            var pixel = image.GetPixel(x, y);
            output[offset++] = (byte)((Quantize4(pixel.A) << 4) | Quantize4(pixel.Intensity));
        });
        return output;
    }

    private static byte[] EncodeIA8(TgaImage image)
    {
        var output = new byte[BlockCount(image.Width, 4) * BlockCount(image.Height, 4) * 32];
        int offset = 0;
        ForBlocks(image, 4, 4, (x, y) =>
        {
            var pixel = image.GetPixel(x, y);
            WriteUInt16(output, offset, (ushort)((pixel.A << 8) | pixel.Intensity));
            offset += 2;
        });
        return output;
    }

    private static byte[] EncodeRgb565(TgaImage image)
    {
        var output = new byte[BlockCount(image.Width, 4) * BlockCount(image.Height, 4) * 32];
        int offset = 0;
        ForBlocks(image, 4, 4, (x, y) =>
        {
            WriteUInt16(output, offset, ToRgb565(image.GetPixel(x, y)));
            offset += 2;
        });
        return output;
    }

    private static byte[] EncodeRgb5A3(TgaImage image)
    {
        var output = new byte[BlockCount(image.Width, 4) * BlockCount(image.Height, 4) * 32];
        int offset = 0;
        ForBlocks(image, 4, 4, (x, y) =>
        {
            WriteUInt16(output, offset, ToRgb5A3(image.GetPixel(x, y)));
            offset += 2;
        });
        return output;
    }

    private static byte[] EncodeRgba8(TgaImage image)
    {
        var output = new byte[BlockCount(image.Width, 4) * BlockCount(image.Height, 4) * 64];
        int offset = 0;
        for (int blockY = 0; blockY < image.Height; blockY += 4)
        {
            for (int blockX = 0; blockX < image.Width; blockX += 4)
            {
                for (int y = 0; y < 4; y++)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        var pixel = image.GetPixel(blockX + x, blockY + y);
                        output[offset++] = pixel.A;
                        output[offset++] = pixel.R;
                    }
                }

                for (int y = 0; y < 4; y++)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        var pixel = image.GetPixel(blockX + x, blockY + y);
                        output[offset++] = pixel.G;
                        output[offset++] = pixel.B;
                    }
                }
            }
        }

        return output;
    }

    private static void ForBlocks(TgaImage image, int blockWidth, int blockHeight, Action<int, int> write, int stepX = 1)
    {
        for (int blockY = 0; blockY < image.Height; blockY += blockHeight)
        {
            for (int blockX = 0; blockX < image.Width; blockX += blockWidth)
            {
                for (int y = 0; y < blockHeight; y++)
                {
                    for (int x = 0; x < blockWidth; x += stepX)
                    {
                        write(blockX + x, blockY + y);
                    }
                }
            }
        }
    }

    private static int BlockCount(int value, int blockSize)
        => (value + blockSize - 1) / blockSize;

    private static byte Quantize4(int value)
        => (byte)((value * 15 + 127) / 255);

    private static ushort ToRgb565(Rgba32 pixel)
        => (ushort)(((pixel.R >> 3) << 11) | ((pixel.G >> 2) << 5) | (pixel.B >> 3));

    private static ushort ToRgb5A3(Rgba32 pixel)
    {
        if (pixel.A < 224)
        {
            return (ushort)(((pixel.A >> 5) << 12) | ((pixel.R >> 4) << 8) | ((pixel.G >> 4) << 4) | (pixel.B >> 4));
        }

        return (ushort)(0x8000 | ((pixel.R >> 3) << 10) | ((pixel.G >> 3) << 5) | (pixel.B >> 3));
    }

    private static uint MapFormat(TexelFormat format)
        => format switch
        {
            TexelFormat.I4 => 0,
            TexelFormat.I8 => 1,
            TexelFormat.IA4 => 2,
            TexelFormat.IA8 => 3,
            TexelFormat.RGB565 => 4,
            TexelFormat.RGB5A3 => 5,
            TexelFormat.RGBA8 => 6,
            TexelFormat.CMPR => 14,
            _ => throw new NotSupportedException($"Unsupported texel format: {format}."),
        };

    private static void WriteUInt16(Span<byte> buffer, int offset, ushort value)
        => BinaryPrimitives.WriteUInt16BigEndian(buffer.Slice(offset, 2), value);

    private static void WriteUInt32(Span<byte> buffer, int offset, uint value)
        => BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(offset, 4), value);
}
