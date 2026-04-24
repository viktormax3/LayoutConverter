using LayoutConverter.Core.IO;

namespace LayoutConverter.Core.Binary;

public static class BinaryLayoutContainerReader
{
    private const ushort ExpectedByteOrderMark = 0xFEFF;

    public static BinaryLayoutContainer Read(string path)
    {
        using var stream = File.OpenRead(path);
        return Read(stream, path);
    }

    public static BinaryLayoutContainer Read(Stream stream, string? path = null)
    {
        using var reader = new BigEndianBinaryInputReader(stream, leaveOpen: true);
        if (reader.Length < 16)
        {
            throw new InvalidDataException($"Invalid binary layout header: {path ?? "<stream>"}");
        }

        string magic = reader.ReadFixedAscii(4);
        if (magic is not ("RLYT" or "RLAN"))
        {
            throw new InvalidDataException($"Unsupported binary layout magic '{magic}': {path ?? "<stream>"}");
        }

        ushort bom = reader.ReadUInt16();
        if (bom != ExpectedByteOrderMark)
        {
            throw new InvalidDataException($"Unsupported byte order mark 0x{bom:X4}: {path ?? "<stream>"}");
        }

        ushort version = reader.ReadUInt16();
        uint fileSize = reader.ReadUInt32();
        ushort headerSize = reader.ReadUInt16();
        ushort sectionCount = reader.ReadUInt16();

        if (fileSize > reader.Length)
        {
            throw new InvalidDataException($"Declared file size exceeds stream length: {path ?? "<stream>"}");
        }

        reader.Position = headerSize;
        var sections = new List<BinaryLayoutSection>(sectionCount);
        while (reader.Position < fileSize)
        {
            long sectionOffset = reader.Position;
            if (sectionOffset + 8 > fileSize)
            {
                throw new EndOfStreamException($"Unexpected end of binary layout section table: {path ?? "<stream>"}");
            }

            string sectionMagic = reader.ReadFixedAscii(4);
            uint sectionSize = reader.ReadUInt32();
            if (sectionSize < 8 || sectionOffset + sectionSize > fileSize)
            {
                throw new InvalidDataException($"Invalid section '{sectionMagic}' size 0x{sectionSize:X8}: {path ?? "<stream>"}");
            }

            sections.Add(new BinaryLayoutSection(
                sectionMagic,
                sectionSize,
                sectionOffset,
                sectionOffset + 8,
                sectionSize - 8));

            reader.Position = sectionOffset + sectionSize;
        }

        return new BinaryLayoutContainer(magic, bom, version, fileSize, headerSize, sectionCount, sections);
    }
}
