using System.Text;
using LayoutConverter.Core.IO;
using LayoutConverter.Core.Schema.Rlan;

namespace LayoutConverter.Core.Brlan;

public sealed class BrlanBinaryWriter
{
    private const uint BinaryMagic = 0x524C414E; // RLAN
    private const ushort ByteOrderMark = 0xFEFF;
    private const ushort BrlanVersion = 0x0008;
    private const ushort BrlanRefResVersion = 0x000A;
    private const ushort HeaderSize = 0x0010;
    private const int SectionAlignment = 4;

    private readonly BigEndianBinaryOutputWriter _writer;
    private ushort _sectionCount;

    public BrlanBinaryWriter(Stream output, bool leaveOpen = false)
    {
        _writer = new BigEndianBinaryOutputWriter(output, leaveOpen);
    }

    public void Write(
        Document document,
        AnimTag tag,
        IReadOnlyList<BrlanAnimationContent> contents,
        IReadOnlyList<RefRes>? refResources = null,
        BrlanBinaryWriteOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(contents);

        options ??= BrlanBinaryWriteOptions.Default;
        var resources = refResources ?? Array.Empty<RefRes>();
        long fileSizePatchOffset = WriteFileHeader(options.UseBannerVersion);
        if (options.IncludeTagInfo)
        {
            WritePat1Section(tag, options.TagIndex);
        }

        if (options.AnimShare is not null)
        {
            WritePah1Section(options.AnimShare);
        }

        WritePai1Section(tag, contents, resources);

        _writer.Align(SectionAlignment);
        _writer.PatchUInt32(fileSizePatchOffset, checked((uint)_writer.Position));
        _writer.PatchUInt16(14, _sectionCount);
    }

    private long WriteFileHeader(bool useBannerVersion)
    {
        _writer.WriteUInt32(BinaryMagic);
        _writer.WriteUInt16(ByteOrderMark);
        _writer.WriteUInt16(useBannerVersion ? BrlanVersion : BrlanRefResVersion);
        long fileSizePatchOffset = _writer.ReserveUInt32();
        _writer.WriteUInt16(HeaderSize);
        _writer.WriteUInt16(0);
        return fileSizePatchOffset;
    }

    private void WritePat1Section(AnimTag tag, int tagIndex)
    {
        using var section = new BrlanSectionScope(_writer, "pat1");
        long sectionStart = section.SectionStart;

        _writer.WriteUInt16(checked((ushort)tagIndex));
        _writer.WriteUInt16(checked((ushort)(tag.group?.Length ?? 0)));
        long nameOffsetPatch = _writer.ReserveUInt32();
        long groupOffsetPatch = _writer.ReserveUInt32();
        _writer.WriteInt16(ClampInt16(tag.startFrame));
        _writer.WriteInt16(ClampInt16(tag.endFrame));
        _writer.WriteByte(tag.descendingBind ? (byte)1 : (byte)0);
        WriteZeroBytes(3);

        _writer.PatchUInt32(nameOffsetPatch, RelativeTo(sectionStart));
        WriteCString(tag.name);
        _writer.Align(SectionAlignment);

        _writer.PatchUInt32(groupOffsetPatch, RelativeTo(sectionStart));
        foreach (var group in tag.group ?? Array.Empty<GroupRef>())
        {
            WriteFixedAscii(group.name, 16);
            WriteZeroBytes(4);
        }

        _sectionCount++;
    }

    private void WritePah1Section(AnimShare animShare)
    {
        var shareInfo = animShare.animShareInfo
            ?? throw new InvalidOperationException("animShare.animShareInfo must be not null.");

        using var section = new BrlanSectionScope(_writer, "pah1");
        long sectionStart = section.SectionStart;

        long infoOffsetPatch = _writer.ReserveUInt32();
        _writer.WriteUInt16(checked((ushort)shareInfo.Length));
        _writer.WriteUInt16(0);

        _writer.PatchUInt32(infoOffsetPatch, RelativeTo(sectionStart));
        foreach (var info in shareInfo)
        {
            WriteFixedAscii(info.srcPaneName, 16);
            _writer.WriteByte(0);
            WriteFixedAscii(info.targetGroupName, 16);
            _writer.WriteByte(0);
            WriteZeroBytes(2);
        }

        _sectionCount++;
    }

    private void WritePai1Section(
        AnimTag tag,
        IReadOnlyList<BrlanAnimationContent> contents,
        IReadOnlyList<RefRes> refResources)
    {
        using var section = new BrlanSectionScope(_writer, "pai1");
        long sectionStart = section.SectionStart;

        _writer.WriteUInt16(checked((ushort)tag.FrameSize));
        _writer.WriteByte(tag.animLoop == AnimLoopType.Loop ? (byte)1 : (byte)0);
        _writer.WriteByte(0);
        _writer.WriteUInt16(checked((ushort)refResources.Count));
        _writer.WriteUInt16(checked((ushort)contents.Count));
        long contentOffsetsOffsetPatch = _writer.ReserveUInt32();

        if (refResources.Count > 0)
        {
            WriteRefResourceTable(refResources);
        }

        _writer.PatchUInt32(contentOffsetsOffsetPatch, RelativeTo(sectionStart));

        var contentOffsetPatches = new List<long>(contents.Count);
        foreach (var _ in contents)
        {
            contentOffsetPatches.Add(_writer.ReserveUInt32());
        }

        for (int i = 0; i < contents.Count; i++)
        {
            var content = contents[i];
            _writer.PatchUInt32(contentOffsetPatches[i], RelativeTo(sectionStart));
            long contentStart = _writer.Position;

            var groups = content.Groups
                .Where(static group => group.Targets.Count > 0)
                .ToArray();

            WriteFixedAscii(content.Name, 20);
            _writer.WriteByte(checked((byte)groups.Length));
            _writer.WriteByte(content.Kind == BrlanAnimationContentKind.Pane ? (byte)0 : (byte)1);
            _writer.WriteUInt16(0);

            var groupOffsetPatches = new List<long>(groups.Length);
            foreach (var _ in groups)
            {
                groupOffsetPatches.Add(_writer.ReserveUInt32());
            }

            for (int groupIndex = 0; groupIndex < groups.Length; groupIndex++)
            {
                var group = groups[groupIndex];
                _writer.PatchUInt32(groupOffsetPatches[groupIndex], checked((uint)(_writer.Position - contentStart)));
                long groupStart = _writer.Position;
                _writer.WriteUInt32(MapAnimationTypeMagic(group.AnimationType));
                _writer.WriteByte(checked((byte)group.Targets.Count));
                WriteZeroBytes(3);

                var targetOffsetPatches = new List<long>(group.Targets.Count);
                foreach (var _ in group.Targets)
                {
                    targetOffsetPatches.Add(_writer.ReserveUInt32());
                }

                for (int targetIndex = 0; targetIndex < group.Targets.Count; targetIndex++)
                {
                    var target = group.Targets[targetIndex];
                    _writer.PatchUInt32(targetOffsetPatches[targetIndex], checked((uint)(_writer.Position - groupStart)));
                    _writer.WriteByte(target.id);
                    _writer.WriteByte(MapTargetType(target.target));
                    _writer.WriteByte(GetKeyFormat(group.AnimationType));
                    _writer.WriteByte(0);
                    _writer.WriteUInt16(checked((ushort)(target.key?.Length ?? 0)));
                    _writer.WriteUInt16(0);
                    _writer.WriteUInt32(12);
                    WriteKeys(target, group.AnimationType, tag.startFrame, refResources);
                }
            }
        }

        _sectionCount++;
    }

    private void WriteKeys(
        AnimTarget target,
        AnimationType animationType,
        int startFrame,
        IReadOnlyList<RefRes> refResources)
    {
        foreach (var key in target.key ?? Array.Empty<Hermite>())
        {
            _writer.WriteSingle(key.frame - startFrame);

            if (animationType == AnimationType.Visibility)
            {
                _writer.WriteUInt16(checked((ushort)key.value));
                _writer.WriteUInt16(0);
            }
            else if (animationType == AnimationType.TexturePattern)
            {
                _writer.WriteUInt16(ResolveTexturePatternRefIndex(target, key, refResources));
                _writer.WriteUInt16(0);
            }
            else
            {
                _writer.WriteSingle(key.value);
                _writer.WriteSingle(key.slope);
            }
        }
    }

    private static ushort ResolveTexturePatternRefIndex(
        AnimTarget target,
        Hermite key,
        IReadOnlyList<RefRes> refResources)
    {
        int targetRefIndex = checked((int)key.value);
        var targetResources = target.refRes ?? Array.Empty<RefRes>();
        if ((uint)targetRefIndex >= (uint)targetResources.Length)
        {
            throw new InvalidOperationException($"TexturePattern key references refRes index {targetRefIndex}, but the target only has {targetResources.Length} resources.");
        }

        string name = targetResources[targetRefIndex].name;
        for (int index = 0; index < refResources.Count; index++)
        {
            if (string.Equals(refResources[index].name, name, StringComparison.InvariantCultureIgnoreCase))
            {
                return checked((ushort)index);
            }
        }

        throw new InvalidOperationException($"TexturePattern refRes '{name}' was not found in the BRLAN resource table.");
    }

    private static byte GetKeyFormat(AnimationType animationType)
        => animationType is AnimationType.Visibility or AnimationType.TexturePattern ? (byte)1 : (byte)2;

    private static uint MapAnimationTypeMagic(AnimationType animationType)
        => animationType switch
        {
            AnimationType.PainSRT => 0x524C5041,
            AnimationType.Visibility => 0x524C5649,
            AnimationType.VertexColor => 0x524C5643,
            AnimationType.MaterialColor => 0x524C4D43,
            AnimationType.TextureSRT => 0x524C5453,
            AnimationType.TexturePattern => 0x524C5450,
            AnimationType.IndTextureSRT => 0x524C494D,
            _ => throw new ArgumentOutOfRangeException(nameof(animationType), animationType, null),
        };

    private static byte MapTargetType(AnimTargetType target)
        => target switch
        {
            AnimTargetType.TranslateX => 0,
            AnimTargetType.TranslateY => 1,
            AnimTargetType.TranslateZ => 2,
            AnimTargetType.RotateX => 3,
            AnimTargetType.RotateY => 4,
            AnimTargetType.RotateZ => 5,
            AnimTargetType.ScaleX => 6,
            AnimTargetType.ScaleY => 7,
            AnimTargetType.SizeW => 8,
            AnimTargetType.SizeH => 9,
            AnimTargetType.Visibility => 0,
            AnimTargetType.LT_r => 0,
            AnimTargetType.LT_g => 1,
            AnimTargetType.LT_b => 2,
            AnimTargetType.LT_a => 3,
            AnimTargetType.RT_r => 4,
            AnimTargetType.RT_g => 5,
            AnimTargetType.RT_b => 6,
            AnimTargetType.RT_a => 7,
            AnimTargetType.LB_r => 8,
            AnimTargetType.LB_g => 9,
            AnimTargetType.LB_b => 10,
            AnimTargetType.LB_a => 11,
            AnimTargetType.RB_r => 12,
            AnimTargetType.RB_g => 13,
            AnimTargetType.RB_b => 14,
            AnimTargetType.RB_a => 15,
            AnimTargetType.PaneAlpha => 16,
            AnimTargetType.MatColor0_r => 0,
            AnimTargetType.MatColor0_g => 1,
            AnimTargetType.MatColor0_b => 2,
            AnimTargetType.MatColor0_a => 3,
            AnimTargetType.TevColor0_r => 4,
            AnimTargetType.TevColor0_g => 5,
            AnimTargetType.TevColor0_b => 6,
            AnimTargetType.TevColor0_a => 7,
            AnimTargetType.TevColor1_r => 8,
            AnimTargetType.TevColor1_g => 9,
            AnimTargetType.TevColor1_b => 10,
            AnimTargetType.TevColor1_a => 11,
            AnimTargetType.TevColor2_r => 12,
            AnimTargetType.TevColor2_g => 13,
            AnimTargetType.TevColor2_b => 14,
            AnimTargetType.TevColor2_a => 15,
            AnimTargetType.TevKonst0_r => 16,
            AnimTargetType.TevKonst0_g => 17,
            AnimTargetType.TevKonst0_b => 18,
            AnimTargetType.TevKonst0_a => 19,
            AnimTargetType.TevKonst1_r => 20,
            AnimTargetType.TevKonst1_g => 21,
            AnimTargetType.TevKonst1_b => 22,
            AnimTargetType.TevKonst1_a => 23,
            AnimTargetType.TevKonst2_r => 24,
            AnimTargetType.TevKonst2_g => 25,
            AnimTargetType.TevKonst2_b => 26,
            AnimTargetType.TevKonst2_a => 27,
            AnimTargetType.TevKonst3_r => 28,
            AnimTargetType.TevKonst3_g => 29,
            AnimTargetType.TevKonst3_b => 30,
            AnimTargetType.TevKonst3_a => 31,
            AnimTargetType.TranslateS => 0,
            AnimTargetType.TranslateT => 1,
            AnimTargetType.Rotate => 2,
            AnimTargetType.ScaleS => 3,
            AnimTargetType.ScaleT => 4,
            AnimTargetType.Image => 0,
            AnimTargetType.Palette => 1,
            _ => 0,
        };

    private void WriteFixedAscii(string? text, int length)
    {
        var bytes = Encoding.ASCII.GetBytes(text ?? string.Empty);
        if (bytes.Length > length)
        {
            _writer.Write(bytes.AsSpan(0, length));
            return;
        }

        _writer.Write(bytes);
        WriteZeroBytes(length - bytes.Length);
    }

    private void WriteRefResourceTable(IReadOnlyList<RefRes> resources)
    {
        long offsetsStart = _writer.Position;
        long[] offsetPatches = new long[resources.Count];
        for (int i = 0; i < resources.Count; i++)
        {
            offsetPatches[i] = _writer.ReserveUInt32();
        }

        for (int i = 0; i < resources.Count; i++)
        {
            uint relativeOffset = checked((uint)(_writer.Position - offsetsStart));
            _writer.PatchUInt32(offsetPatches[i], relativeOffset);
            WriteCString(resources[i].GetFileName());
        }

        _writer.Align(SectionAlignment);
    }

    private void WriteCString(string? text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            _writer.Write(Encoding.ASCII.GetBytes(text));
        }

        _writer.WriteByte(0);
    }

    private uint RelativeTo(long basePosition)
        => checked((uint)(_writer.Position - basePosition));

    private static short ClampInt16(int value)
        => checked((short)Math.Min(Math.Max(value, short.MinValue), short.MaxValue));

    private void WriteZeroBytes(int count)
    {
        for (int i = 0; i < count; i++)
        {
            _writer.WriteByte(0);
        }
    }
}

internal sealed class BrlanSectionScope : IDisposable
{
    private readonly BigEndianBinaryOutputWriter _writer;
    private readonly long _sectionSizePatchOffset;

    public BrlanSectionScope(BigEndianBinaryOutputWriter writer, string fourCc)
    {
        _writer = writer;
        SectionStart = writer.Position;
        writer.WriteFixedAscii(fourCc, 4);
        _sectionSizePatchOffset = writer.ReserveUInt32();
    }

    public long SectionStart { get; }

    public void Dispose()
    {
        _writer.Align(4);
        _writer.PatchUInt32(_sectionSizePatchOffset, checked((uint)(_writer.Position - SectionStart)));
    }
}

public sealed record BrlanAnimationContent(
    string Name,
    BrlanAnimationContentKind Kind,
    IReadOnlyList<BrlanAnimationGroup> Groups);

public sealed record BrlanAnimationGroup(AnimationType AnimationType, IReadOnlyList<AnimTarget> Targets);

public sealed class BrlanBinaryWriteOptions
{
    public static BrlanBinaryWriteOptions Default { get; } = new();

    public int TagIndex { get; init; }
    public bool IncludeTagInfo { get; init; }
    public AnimShare? AnimShare { get; init; }
    public bool UseBannerVersion { get; init; }
}

public enum BrlanAnimationContentKind
{
    Pane,
    Material,
}
