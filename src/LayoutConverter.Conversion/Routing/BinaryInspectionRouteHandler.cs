using LayoutConverter.Conversion.Options;
using LayoutConverter.Conversion.Requests;
using LayoutConverter.Core.Binary;
using System.Buffers.Binary;
using System.Text;

namespace LayoutConverter.Conversion.Routing;

public sealed class BinaryInspectionRouteHandler : IConversionRouteHandler
{
    public bool CanHandle(ConversionRequest request)
        => request is BinaryInspectionRequest;

    public ConversionExitCode Execute(ConversionRequest request, TextWriter log)
    {
        var inspectionRequest = (BinaryInspectionRequest)request;
        try
        {
            var container = BinaryLayoutContainerReader.Read(inspectionRequest.SourcePath);
            var bytes = File.ReadAllBytes(inspectionRequest.SourcePath);
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(inspectionRequest.DestinationPath))!);

            using var output = new StreamWriter(inspectionRequest.DestinationPath, append: false);
            WriteReport(container, bytes, output);
            log.WriteLine($"Wrote {inspectionRequest.DestinationPath}");
            return ConversionExitCode.Success;
        }
        catch (InvalidDataException ex)
        {
            log.WriteLine(ex.Message);
            return ConversionExitCode.InputAccessFailure;
        }
        catch (IOException ex)
        {
            log.WriteLine(ex.Message);
            return ConversionExitCode.InputAccessFailure;
        }
    }

    private static void WriteReport(BinaryLayoutContainer container, byte[] bytes, TextWriter output)
    {
        output.WriteLine($"Magic: {container.Magic}");
        output.WriteLine($"Version: 0x{container.Version:X4}");
        output.WriteLine($"FileSize: 0x{container.FileSize:X8} ({container.FileSize})");
        output.WriteLine($"HeaderSize: 0x{container.HeaderSize:X4} ({container.HeaderSize})");
        output.WriteLine($"SectionCount: {container.SectionCount}");
        output.WriteLine();
        output.WriteLine("Sections:");

        for (int i = 0; i < container.Sections.Count; i++)
        {
            var section = container.Sections[i];
            output.WriteLine(
                $"{i:D2} {section.Magic} offset=0x{section.Offset:X8} size=0x{section.Size:X8} payload=0x{section.PayloadSize:X8}");
            WriteSectionDetails(container, section, bytes, output);
        }
    }

    private static void WriteSectionDetails(
        BinaryLayoutContainer container,
        BinaryLayoutSection section,
        byte[] bytes,
        TextWriter output)
    {
        try
        {
            switch (section.Magic)
            {
            case "lyt1":
                WriteLyt1(section, bytes, output);
                break;
            case "txl1":
            case "fnl1":
                WriteStringTable(section, bytes, output);
                break;
            case "pat1":
                WritePat1(section, bytes, output);
                break;
            case "pah1":
                WritePah1(section, bytes, output);
                break;
            case "pai1":
                WritePai1(section, bytes, output);
                break;
            case "pan1":
            case "pic1":
            case "txt1":
            case "wnd1":
            case "bnd1":
                WritePane(section, bytes, output);
                break;
            }
        }
        catch (Exception ex) when (ex is EndOfStreamException or InvalidDataException or ArgumentOutOfRangeException)
        {
            output.WriteLine($"    detail-error: {ex.Message}");
        }
    }

    private static void WriteLyt1(BinaryLayoutSection section, byte[] bytes, TextWriter output)
    {
        int payload = checked((int)section.PayloadOffset);
        byte originMode = ReadByte(bytes, payload);
        float width = ReadSingle(bytes, payload + 4);
        float height = ReadSingle(bytes, payload + 8);
        output.WriteLine($"    originMode={originMode} size=({width}, {height})");
    }

    private static void WriteStringTable(BinaryLayoutSection section, byte[] bytes, TextWriter output)
    {
        int payload = checked((int)section.PayloadOffset);
        int count = ReadUInt16(bytes, payload);
        int offsetsStart = payload + 4;
        output.WriteLine($"    count={count}");

        for (int i = 0; i < count; i++)
        {
            int recordOffset = offsetsStart + i * 8;
            uint relativeOffset = ReadUInt32(bytes, recordOffset);
            string value = ReadCString(bytes, checked(offsetsStart + (int)relativeOffset));
            output.WriteLine($"    [{i}] {value}");
        }
    }

    private static void WritePat1(BinaryLayoutSection section, byte[] bytes, TextWriter output)
    {
        int sectionStart = checked((int)section.Offset);
        int payload = checked((int)section.PayloadOffset);
        int tagIndex = ReadUInt16(bytes, payload);
        int groupCount = ReadUInt16(bytes, payload + 2);
        int nameOffset = sectionStart + checked((int)ReadUInt32(bytes, payload + 4));
        int groupOffset = sectionStart + checked((int)ReadUInt32(bytes, payload + 8));
        short startFrame = ReadInt16(bytes, payload + 12);
        short endFrame = ReadInt16(bytes, payload + 14);
        bool descendingBind = ReadByte(bytes, payload + 16) != 0;

        output.WriteLine($"    tagIndex={tagIndex} name={ReadCString(bytes, nameOffset)} frames={startFrame}..{endFrame} descendingBind={descendingBind}");
        output.WriteLine($"    groups={groupCount}");
        for (int i = 0; i < groupCount; i++)
        {
            output.WriteLine($"    group[{i}] {ReadFixedAscii(bytes, groupOffset + i * 20, 16)}");
        }
    }

    private static void WritePah1(BinaryLayoutSection section, byte[] bytes, TextWriter output)
    {
        int sectionStart = checked((int)section.Offset);
        int payload = checked((int)section.PayloadOffset);
        int infoOffset = sectionStart + checked((int)ReadUInt32(bytes, payload));
        int count = ReadUInt16(bytes, payload + 4);

        output.WriteLine($"    shareCount={count}");
        for (int i = 0; i < count; i++)
        {
            int entry = infoOffset + i * 36;
            string source = ReadFixedAscii(bytes, entry, 16);
            string target = ReadFixedAscii(bytes, entry + 17, 16);
            output.WriteLine($"    share[{i}] {source} -> {target}");
        }
    }

    private static void WritePai1(BinaryLayoutSection section, byte[] bytes, TextWriter output)
    {
        int sectionStart = checked((int)section.Offset);
        int payload = checked((int)section.PayloadOffset);
        int frameSize = ReadUInt16(bytes, payload);
        bool loop = ReadByte(bytes, payload + 2) != 0;
        int refCount = ReadUInt16(bytes, payload + 4);
        int contentCount = ReadUInt16(bytes, payload + 6);
        int contentOffsetsOffset = sectionStart + checked((int)ReadUInt32(bytes, payload + 8));

        output.WriteLine($"    frameSize={frameSize} loop={loop} refResources={refCount} contents={contentCount}");
        if (refCount > 0)
        {
            WritePaiRefResources(section, bytes, output, payload + 12, refCount);
        }

        for (int i = 0; i < contentCount; i++)
        {
            int contentOffset = sectionStart + checked((int)ReadUInt32(bytes, contentOffsetsOffset + i * 4));
            WritePaiContent(bytes, output, sectionStart, i, contentOffset);
        }
    }

    private static void WritePane(BinaryLayoutSection section, byte[] bytes, TextWriter output)
    {
        int payload = checked((int)section.PayloadOffset);
        byte flags = ReadByte(bytes, payload);
        byte basePosition = ReadByte(bytes, payload + 1);
        byte alpha = ReadByte(bytes, payload + 2);
        string name = ReadFixedAscii(bytes, payload + 4, 16);
        float tx = ReadSingle(bytes, payload + 28);
        float ty = ReadSingle(bytes, payload + 32);
        float tz = ReadSingle(bytes, payload + 36);
        float rx = ReadSingle(bytes, payload + 40);
        float ry = ReadSingle(bytes, payload + 44);
        float rz = ReadSingle(bytes, payload + 48);
        float sx = ReadSingle(bytes, payload + 52);
        float sy = ReadSingle(bytes, payload + 56);
        float w = ReadSingle(bytes, payload + 60);
        float h = ReadSingle(bytes, payload + 64);

        output.WriteLine(
            $"    name={name} flags=0x{flags:X2} basePosition={basePosition} alpha={alpha} translate=({tx}, {ty}, {tz}) rotate=({rx}, {ry}, {rz}) scale=({sx}, {sy}) size=({w}, {h})");

        if (section.Magic == "pic1")
        {
            WritePicturePaneDetails(payload + 76, bytes, output);
        }
    }

    private static void WritePicturePaneDetails(int payload, byte[] bytes, TextWriter output)
    {
        output.WriteLine(
            $"    vtxLT={ReadColor(bytes, payload)} vtxRT={ReadColor(bytes, payload + 4)} vtxLB={ReadColor(bytes, payload + 8)} vtxRB={ReadColor(bytes, payload + 12)}");
        output.WriteLine(
            $"    materialIndex={ReadUInt16(bytes, payload + 16)} texCoordCount={ReadByte(bytes, payload + 18)}");
    }

    private static void WritePaiRefResources(
        BinaryLayoutSection section,
        byte[] bytes,
        TextWriter output,
        int offsetsStart,
        int refCount)
    {
        for (int i = 0; i < refCount; i++)
        {
            int refOffset = offsetsStart + checked((int)ReadUInt32(bytes, offsetsStart + i * 4));
            output.WriteLine($"    refRes[{i}] {ReadCString(bytes, refOffset)}");
        }
    }

    private static void WritePaiContent(byte[] bytes, TextWriter output, int sectionStart, int index, int contentOffset)
    {
        string name = ReadFixedAscii(bytes, contentOffset, 20);
        int groupCount = ReadByte(bytes, contentOffset + 20);
        byte kind = ReadByte(bytes, contentOffset + 21);
        output.WriteLine($"    content[{index}] name={name} kind={(kind == 0 ? "pane" : "material")} groups={groupCount}");

        for (int groupIndex = 0; groupIndex < groupCount; groupIndex++)
        {
            int groupOffset = contentOffset + checked((int)ReadUInt32(bytes, contentOffset + 24 + groupIndex * 4));
            uint magic = ReadUInt32(bytes, groupOffset);
            int targetCount = ReadByte(bytes, groupOffset + 4);
            output.WriteLine($"      group[{groupIndex}] {FourCc(magic)} targets={targetCount}");

            for (int targetIndex = 0; targetIndex < targetCount; targetIndex++)
            {
                int targetOffset = groupOffset + checked((int)ReadUInt32(bytes, groupOffset + 8 + targetIndex * 4));
                byte id = ReadByte(bytes, targetOffset);
                byte target = ReadByte(bytes, targetOffset + 1);
                byte keyFormat = ReadByte(bytes, targetOffset + 2);
                int keyCount = ReadUInt16(bytes, targetOffset + 4);
                output.WriteLine($"        target[{targetIndex}] id={id} target={target} keyFormat={keyFormat} keys={keyCount}");
            }
        }
    }

    private static byte ReadByte(byte[] bytes, int offset)
    {
        EnsureRange(bytes, offset, 1);
        return bytes[offset];
    }

    private static ushort ReadUInt16(byte[] bytes, int offset)
    {
        EnsureRange(bytes, offset, 2);
        return BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(offset, 2));
    }

    private static short ReadInt16(byte[] bytes, int offset)
    {
        EnsureRange(bytes, offset, 2);
        return BinaryPrimitives.ReadInt16BigEndian(bytes.AsSpan(offset, 2));
    }

    private static uint ReadUInt32(byte[] bytes, int offset)
    {
        EnsureRange(bytes, offset, 4);
        return BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(offset, 4));
    }

    private static float ReadSingle(byte[] bytes, int offset)
        => BitConverter.Int32BitsToSingle(unchecked((int)ReadUInt32(bytes, offset)));

    private static string ReadFixedAscii(byte[] bytes, int offset, int length)
    {
        EnsureRange(bytes, offset, length);
        var span = bytes.AsSpan(offset, length);
        int end = span.IndexOf((byte)0);
        if (end < 0)
        {
            end = span.Length;
        }

        return Encoding.ASCII.GetString(span[..end]);
    }

    private static string ReadCString(byte[] bytes, int offset)
    {
        EnsureRange(bytes, offset, 1);
        int end = offset;
        while (end < bytes.Length && bytes[end] != 0)
        {
            end++;
        }

        if (end >= bytes.Length)
        {
            throw new EndOfStreamException("Unterminated string.");
        }

        return Encoding.ASCII.GetString(bytes, offset, end - offset);
    }

    private static string FourCc(uint value)
    {
        Span<byte> bytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(bytes, value);
        return Encoding.ASCII.GetString(bytes);
    }

    private static string ReadColor(byte[] bytes, int offset)
        => $"#{ReadByte(bytes, offset):X2}{ReadByte(bytes, offset + 1):X2}{ReadByte(bytes, offset + 2):X2}{ReadByte(bytes, offset + 3):X2}";

    private static void EnsureRange(byte[] bytes, int offset, int count)
    {
        if (offset < 0 || count < 0 || offset > bytes.Length - count)
        {
            throw new EndOfStreamException();
        }
    }
}
