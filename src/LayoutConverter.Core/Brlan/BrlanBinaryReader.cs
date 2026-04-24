using System.Buffers.Binary;
using LayoutConverter.Core.Binary;
using LayoutConverter.Core.Schema.Rlan;

namespace LayoutConverter.Core.Brlan;

public static class BrlanBinaryReader
{
    public static Document ReadDocument(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var container = BinaryLayoutContainerReader.Read(new MemoryStream(bytes), path);
        if (container.Magic != "RLAN")
        {
            throw new InvalidDataException($"Expected RLAN binary, found {container.Magic}: {path}");
        }

        var patTags = new List<AnimTag>();
        AnimShare? animShare = null;
        var rlanBlocks = new List<RLAN>();

        foreach (var section in container.Sections)
        {
            switch (section.Magic)
            {
            case "pat1":
                patTags.Add(ReadPat1(section, bytes));
                break;
            case "pah1":
                animShare = ReadPah1(section, bytes);
                break;
            case "pai1":
                rlanBlocks.AddRange(ReadPai1(section, bytes, patTags.LastOrDefault()));
                break;
            }
        }

        var body = new DocumentBody
        {
            animTag = patTags.ToArray(),
            animShare = animShare is null ? Array.Empty<AnimShare>() : new[] { animShare },
            rlan = rlanBlocks.ToArray(),
        };

        return new Document
        {
            head = new Head
            {
                create = new HeadCreate
                {
                    user = Environment.UserName,
                    host = Environment.MachineName,
                    date = DateTime.Now,
                    source = path,
                },
                title = Path.GetFileNameWithoutExtension(path),
                comment = "Generated from BRLAN.",
                generator = new HeadGenerator { name = "LayoutConverter", version = "0.1.0" },
            },
            body = body,
        };
    }

    private static AnimTag ReadPat1(BinaryLayoutSection section, byte[] bytes)
    {
        int sectionStart = checked((int)section.Offset);
        int payload = checked((int)section.PayloadOffset);
        int groupCount = ReadUInt16(bytes, payload + 2);
        int nameOffset = sectionStart + checked((int)ReadUInt32(bytes, payload + 4));
        int groupOffset = sectionStart + checked((int)ReadUInt32(bytes, payload + 8));

        var tag = new AnimTag
        {
            binaryIndex = ReadUInt16(bytes, payload),
            binaryIndexSpecified = true,
            name = ReadCString(bytes, nameOffset),
            startFrame = ReadInt16(bytes, payload + 12),
            endFrame = ReadInt16(bytes, payload + 14),
            descendingBind = ReadByte(bytes, payload + 16) != 0,
            group = new GroupRef[groupCount],
        };

        for (int i = 0; i < groupCount; i++)
        {
            tag.group[i] = new GroupRef { name = ReadFixedAscii(bytes, groupOffset + i * 20, 16) };
        }

        return tag;
    }

    private static AnimShare ReadPah1(BinaryLayoutSection section, byte[] bytes)
    {
        int sectionStart = checked((int)section.Offset);
        int payload = checked((int)section.PayloadOffset);
        int infoOffset = sectionStart + checked((int)ReadUInt32(bytes, payload));
        int count = ReadUInt16(bytes, payload + 4);
        var infos = new AnimShareInfo[count];
        for (int i = 0; i < count; i++)
        {
            int entry = infoOffset + i * 36;
            infos[i] = new AnimShareInfo
            {
                srcPaneName = ReadFixedAscii(bytes, entry, 16),
                targetGroupName = ReadFixedAscii(bytes, entry + 17, 16),
            };
        }

        return new AnimShare { animShareInfo = infos };
    }

    private static IEnumerable<RLAN> ReadPai1(BinaryLayoutSection section, byte[] bytes, AnimTag? tag)
    {
        int sectionStart = checked((int)section.Offset);
        int payload = checked((int)section.PayloadOffset);
        int frameSize = ReadUInt16(bytes, payload);
        bool loop = ReadByte(bytes, payload + 2) != 0;
        int refCount = ReadUInt16(bytes, payload + 4);
        int contentCount = ReadUInt16(bytes, payload + 6);
        int contentOffsetsOffset = sectionStart + checked((int)ReadUInt32(bytes, payload + 8));
        var refResources = ReadRefResources(bytes, payload + 12, refCount);

        int startFrame = tag?.startFrame ?? 0;
        int endFrame = tag?.endFrame ?? (loop ? frameSize : Math.Max(0, frameSize - 1));
        if (tag is not null)
        {
            tag.animLoop = loop ? AnimLoopType.Loop : AnimLoopType.OneTime;
        }

        var byType = new SortedDictionary<AnimationType, List<AnimContent>>();
        for (int i = 0; i < contentCount; i++)
        {
            int contentOffset = sectionStart + checked((int)ReadUInt32(bytes, contentOffsetsOffset + i * 4));
            foreach (var group in ReadContentGroups(bytes, contentOffset, startFrame, refResources))
            {
                if (!byType.TryGetValue(group.Type, out var contents))
                {
                    contents = new List<AnimContent>();
                    byType.Add(group.Type, contents);
                }

                contents.Add(new AnimContent { name = group.ContentName, Items = group.Targets.ToArray() });
            }
        }

        foreach (var (type, contents) in byType)
        {
            yield return new RLAN
            {
                animType = type,
                startFrame = startFrame,
                endFrame = endFrame,
                convertStartFrame = startFrame,
                convertEndFrame = endFrame,
                animLoop = loop ? AnimLoopType.Loop : AnimLoopType.OneTime,
                animContent = contents.ToArray(),
            };
        }
    }

    private static RefRes[] ReadRefResources(byte[] bytes, int offsetsStart, int count)
    {
        var resources = new RefRes[count];
        for (int i = 0; i < count; i++)
        {
            int offset = offsetsStart + checked((int)ReadUInt32(bytes, offsetsStart + i * 4));
            string name = ReadCString(bytes, offset);
            resources[i] = new RefRes { name = Path.GetFileNameWithoutExtension(name) };
        }

        return resources;
    }

    private static IEnumerable<MutableGroup> ReadContentGroups(byte[] bytes, int contentOffset, int startFrame, RefRes[] refResources)
    {
        string contentName = ReadFixedAscii(bytes, contentOffset, 20);
        int groupCount = ReadByte(bytes, contentOffset + 20);
        for (int groupIndex = 0; groupIndex < groupCount; groupIndex++)
        {
            int groupOffset = contentOffset + checked((int)ReadUInt32(bytes, contentOffset + 24 + groupIndex * 4));
            var type = MapAnimationType(ReadUInt32(bytes, groupOffset));
            int targetCount = ReadByte(bytes, groupOffset + 4);
            var targets = new List<AnimTarget>(targetCount);
            for (int targetIndex = 0; targetIndex < targetCount; targetIndex++)
            {
                int targetOffset = groupOffset + checked((int)ReadUInt32(bytes, groupOffset + 8 + targetIndex * 4));
                targets.Add(ReadTarget(bytes, targetOffset, type, startFrame, refResources));
            }

            yield return new MutableGroup(contentName, type, targets);
        }
    }

    private static AnimTarget ReadTarget(byte[] bytes, int targetOffset, AnimationType type, int startFrame, RefRes[] refResources)
    {
        byte id = ReadByte(bytes, targetOffset);
        byte target = ReadByte(bytes, targetOffset + 1);
        int keyCount = ReadUInt16(bytes, targetOffset + 4);
        int keyOffset = targetOffset + checked((int)ReadUInt32(bytes, targetOffset + 8));

        var animTarget = CreateTarget(type);
        animTarget.id = id;
        animTarget.target = MapTargetType(type, target);
        if (type == AnimationType.TexturePattern)
        {
            animTarget.refRes = refResources;
        }

        animTarget.key = ReadKeys(bytes, keyOffset, keyCount, type, startFrame);
        return animTarget;
    }

    private static Hermite[] ReadKeys(byte[] bytes, int keyOffset, int keyCount, AnimationType type, int startFrame)
    {
        var keys = new Hermite[keyCount];
        int offset = keyOffset;
        for (int i = 0; i < keyCount; i++)
        {
            float frame = ReadSingle(bytes, offset) + startFrame;
            offset += 4;

            if (type == AnimationType.Visibility)
            {
                keys[i] = new StepBool { frame = frame, value = ReadUInt16(bytes, offset), slopeType = SlopeType.Step };
                offset += 4;
            }
            else if (type == AnimationType.TexturePattern)
            {
                keys[i] = new StepU16 { frame = frame, value = ReadUInt16(bytes, offset), slopeType = SlopeType.Step };
                offset += 4;
            }
            else
            {
                keys[i] = new Hermite
                {
                    frame = frame,
                    value = ReadSingle(bytes, offset),
                    slope = ReadSingle(bytes, offset + 4),
                };
                offset += 8;
            }
        }

        return keys;
    }

    private static AnimTarget CreateTarget(AnimationType type)
        => type switch
        {
            AnimationType.PainSRT => new AnimPainSRTTarget(),
            AnimationType.Visibility => new AnimVisibilityTarget(),
            AnimationType.VertexColor => new AnimVertexColorTarget(),
            AnimationType.MaterialColor => new AnimMaterialColorTarget(),
            AnimationType.TextureSRT => new AnimTexSRTTarget(),
            AnimationType.TexturePattern => new AnimTexPatternTarget(),
            AnimationType.IndTextureSRT => new AnimIndTexSRTTarget(),
            _ => new AnimTarget(),
        };

    private static AnimationType MapAnimationType(uint magic)
        => magic switch
        {
            0x524C5041 => AnimationType.PainSRT,
            0x524C5649 => AnimationType.Visibility,
            0x524C5643 => AnimationType.VertexColor,
            0x524C4D43 => AnimationType.MaterialColor,
            0x524C5453 => AnimationType.TextureSRT,
            0x524C5450 => AnimationType.TexturePattern,
            0x524C494D => AnimationType.IndTextureSRT,
            _ => throw new InvalidDataException($"Unsupported BRLAN animation group magic 0x{magic:X8}."),
        };

    private static AnimTargetType MapTargetType(AnimationType type, byte value)
        => type switch
        {
            AnimationType.PainSRT => (AnimTargetType)value,
            AnimationType.Visibility => AnimTargetType.Visibility,
            AnimationType.VertexColor => (AnimTargetType)(AnimTargetType.LT_r + value),
            AnimationType.MaterialColor => (AnimTargetType)(AnimTargetType.MatColor0_r + value),
            AnimationType.TextureSRT or AnimationType.IndTextureSRT => (AnimTargetType)(AnimTargetType.TranslateS + value),
            AnimationType.TexturePattern => value == 1 ? AnimTargetType.Palette : AnimTargetType.Image,
            _ => AnimTargetType.TranslateX,
        };

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

        return System.Text.Encoding.ASCII.GetString(span[..end]);
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

        return System.Text.Encoding.ASCII.GetString(bytes, offset, end - offset);
    }

    private static void EnsureRange(byte[] bytes, int offset, int count)
    {
        if (offset < 0 || count < 0 || offset > bytes.Length - count)
        {
            throw new EndOfStreamException();
        }
    }

    private sealed record MutableGroup(string ContentName, AnimationType Type, IReadOnlyList<AnimTarget> Targets);
}
