using System.Buffers.Binary;
using System.Text;

namespace LayoutConverter.Conversion.Textures;

internal static class Nw4rTgaAdditionalInfoReader
{
    private static readonly Encoding TextEncoding = Encoding.ASCII;

    public static Nw4rTgaAdditionalInfo Read(string path)
    {
        var bytes = File.ReadAllBytes(path);
        if (bytes.Length < 18)
        {
            throw new InvalidDataException($"Invalid TGA header: {path}");
        }

        int idLength = bytes[0];
        int width = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(12));
        int height = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(14));
        int metadataOffset = FindMetadataOffset(bytes, idLength, path);

        string? texelFormatName = null;
        int mipmapCount = 1;
        byte[]? imageData = null;
        string? paletteFormatName = null;
        byte[]? paletteData = null;

        int offset = metadataOffset;
        while (offset < bytes.Length)
        {
            EnsureAvailable(bytes, offset, 12, path);
            string tag = TextEncoding.GetString(bytes, offset, 8);
            int sectionSize = checked((int)BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(offset + 8)));
            if (sectionSize < 12)
            {
                throw new InvalidDataException($"Invalid NW4R TGA section size in {path}.");
            }

            int payloadSize = sectionSize - 12;
            int payloadOffset = offset + 12;
            EnsureAvailable(bytes, payloadOffset, payloadSize, path);

            if (tag == "nw4r_end")
            {
                break;
            }

            var payload = bytes.AsSpan(payloadOffset, payloadSize);
            switch (tag)
            {
                case "nw4r_tfm":
                    texelFormatName = ReadText(payload);
                    break;
                case "nw4r_mpl":
                    if (!int.TryParse(ReadText(payload), out mipmapCount))
                    {
                        throw new InvalidDataException($"Invalid NW4R TGA mipmap count in {path}.");
                    }

                    break;
                case "nw4r_txd":
                    imageData = payload.ToArray();
                    break;
                case "nw4r_pfm":
                    paletteFormatName = ReadText(payload);
                    break;
                case "nw4r_pld":
                    paletteData = payload.ToArray();
                    break;
            }

            offset += sectionSize;
        }

        if (string.IsNullOrWhiteSpace(texelFormatName))
        {
            throw new InvalidDataException($"NW4R TGA texel format is missing: {path}");
        }

        if (imageData is null || imageData.Length == 0)
        {
            throw new InvalidDataException($"NW4R TGA image data is missing: {path}");
        }

        int texelFormat = MapTexelFormat(texelFormatName);
        int? paletteFormat = null;
        if (paletteData is not null)
        {
            if (string.IsNullOrWhiteSpace(paletteFormatName))
            {
                throw new InvalidDataException($"NW4R TGA palette format is missing: {path}");
            }

            paletteFormat = MapPaletteFormat(paletteFormatName);
        }

        return new Nw4rTgaAdditionalInfo(
            width,
            height,
            texelFormat,
            mipmapCount,
            imageData,
            paletteFormat,
            paletteData);
    }

    private static int FindMetadataOffset(byte[] bytes, int idLength, string path)
    {
        if (idLength < 12)
        {
            throw new InvalidDataException($"NW4R TGA additional information is missing: {path}");
        }

        EnsureAvailable(bytes, 18, idLength, path);
        string id = TextEncoding.GetString(bytes, 18, 8);
        if (id != "NW4R_Tga")
        {
            throw new InvalidDataException($"NW4R TGA additional information is missing: {path}");
        }

        int offset = checked((int)BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(18 + idLength - 4)));
        EnsureAvailable(bytes, offset, 12, path);

        string firstTag = TextEncoding.GetString(bytes, offset, 8);
        if (firstTag != "nw4r_tfm")
        {
            throw new InvalidDataException($"Invalid NW4R TGA additional information: {path}");
        }

        return offset;
    }

    private static string ReadText(ReadOnlySpan<byte> payload)
    {
        int length = payload.IndexOf((byte)0);
        if (length < 0)
        {
            length = payload.Length;
        }

        return TextEncoding.GetString(payload[..length]).Trim();
    }

    private static int MapTexelFormat(string format)
        => format.ToLowerInvariant() switch
        {
            "i4" => 0,
            "i8" => 1,
            "ia4" => 2,
            "ia8" => 3,
            "rgb565" => 4,
            "rgb5a3" => 5,
            "rgba8" => 6,
            "c4" => 8,
            "c8" => 9,
            "c14" => 10,
            "cmpr" => 14,
            _ => throw new NotSupportedException($"Unsupported NW4R TGA texel format: {format}."),
        };

    private static int MapPaletteFormat(string format)
        => format.ToLowerInvariant() switch
        {
            "ia8" => 0,
            "rgb565" => 1,
            "rgb5a3" => 2,
            _ => throw new NotSupportedException($"Unsupported NW4R TGA palette format: {format}."),
        };

    private static void EnsureAvailable(byte[] bytes, int offset, int count, string path)
    {
        if (offset < 0 || count < 0 || offset + count > bytes.Length)
        {
            throw new EndOfStreamException($"Unexpected end of NW4R TGA file: {path}");
        }
    }
}
