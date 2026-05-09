using System.Buffers.Binary;
using LayoutConverter.Core.Schema.Rlyt;

namespace LayoutConverter.Conversion.Textures;

public static class TplTextureInfoReader
{
    private const uint Magic = 0x0020AF30;

    public static bool TryReadInfo(string path, out TplTextureInfo info)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        info = default;
        byte[] bytes = File.ReadAllBytes(path);
        if (bytes.Length < 0x20)
        {
            return false;
        }

        if (ReadUInt32(bytes, 0) != Magic)
        {
            return false;
        }

        if (ReadUInt32(bytes, 4) != 1)
        {
            return false;
        }

        int tableOffset = checked((int)ReadUInt32(bytes, 8));
        if (!InRange(bytes, tableOffset, 8))
        {
            return false;
        }

        int imageHeaderOffset = checked((int)ReadUInt32(bytes, tableOffset));
        if (!InRange(bytes, imageHeaderOffset, 36))
        {
            return false;
        }

        int format = checked((int)ReadUInt32(bytes, imageHeaderOffset + 4));
        if (!TryMapFormat(format, out TexelFormat texelFormat))
        {
            return false;
        }

        info = new TplTextureInfo(texelFormat, format is >= 0 and <= 6);
        return true;
    }

    private static bool TryMapFormat(int format, out TexelFormat texelFormat)
    {
        texelFormat = format switch
        {
            0 => TexelFormat.I4,
            1 => TexelFormat.I8,
            2 => TexelFormat.IA4,
            3 => TexelFormat.IA8,
            4 => TexelFormat.RGB565,
            5 => TexelFormat.RGB5A3,
            6 => TexelFormat.RGBA8,
            14 => TexelFormat.CMPR,
            _ => default,
        };

        return Enum.IsDefined(texelFormat);
    }

    private static uint ReadUInt32(byte[] bytes, int offset)
        => BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(offset, 4));

    private static bool InRange(byte[] bytes, int offset, int count)
        => offset >= 0 && count >= 0 && offset <= bytes.Length - count;
}

public readonly record struct TplTextureInfo(TexelFormat Format, bool CanDecodeToTga);
