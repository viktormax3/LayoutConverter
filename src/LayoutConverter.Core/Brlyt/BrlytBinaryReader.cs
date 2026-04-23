using System.Buffers.Binary;
using LayoutConverter.Core.Binary;
using LayoutConverter.Core.Schema.Rlyt;

namespace LayoutConverter.Core.Brlyt;

public static class BrlytBinaryReader
{
    private const int PaneCommonPayloadSize = 68;
    private const int TextBoxPayloadSize = 40;
    private const int WindowPayloadSize = 28;
    private const int WindowContentPayloadSize = 20;

    public static Document ReadDocument(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var container = BinaryLayoutContainerReader.Read(new MemoryStream(bytes), path);
        if (container.Magic != "RLYT")
        {
            throw new InvalidDataException($"Expected RLYT binary, found {container.Magic}: {path}");
        }

        var textures = new List<TextureFile>();
        var fonts = new List<FontFile>();
        var materials = new List<Material>();
        var panes = new List<Pane>();
        var paneRoots = new List<PaneNode>();
        var paneStack = new Stack<PaneNode>();
        PaneNode? lastPaneNode = null;
        var groups = new List<Group>();
        var groupRoots = new List<GroupNode>();
        var groupStack = new Stack<GroupNode>();
        GroupNode? lastGroupNode = null;
        ScreenSetting? screenSetting = null;

        foreach (var section in container.Sections)
        {
            switch (section.Magic)
            {
            case "lyt1":
                screenSetting = ReadLyt1(section, bytes);
                break;
            case "txl1":
                textures.AddRange(ReadStringTable(section, bytes).Select(static value => new TextureFile
                {
                    imagePath = value,
                    format = TexelFormat.NW4R_TGA,
                }));
                break;
            case "fnl1":
                fonts.AddRange(ReadStringTable(section, bytes).Select(static value => new FontFile { path = value }));
                break;
            case "mat1":
                materials.AddRange(ReadMaterials(section, bytes, textures));
                break;
            case "pan1":
            case "pic1":
            case "txt1":
            case "wnd1":
            case "bnd1":
                var pane = ReadPane(section, bytes, materials, fonts);
                panes.Add(pane);
                var paneNode = new PaneNode(pane.name);
                if (paneStack.Count > 0)
                {
                    paneStack.Peek().Children.Add(paneNode);
                }
                else
                {
                    paneRoots.Add(paneNode);
                }
                lastPaneNode = paneNode;
                break;
            case "pas1":
                if (lastPaneNode is not null)
                {
                    paneStack.Push(lastPaneNode);
                }

                break;
            case "pae1":
                if (paneStack.Count > 0)
                {
                    paneStack.Pop();
                }
                break;
            case "grp1":
                var group = ReadGroup(section, bytes);
                groups.Add(group);
                var groupNode = new GroupNode(group);
                if (groupStack.Count > 0)
                {
                    groupStack.Peek().Children.Add(groupNode);
                }
                else
                {
                    groupRoots.Add(groupNode);
                }
                lastGroupNode = groupNode;
                break;
            case "grs1":
                if (lastGroupNode is not null)
                {
                    groupStack.Push(lastGroupNode);
                }

                break;
            case "gre1":
                if (groupStack.Count > 0)
                {
                    groupStack.Pop();
                }
                break;
            }
        }

        var rlyt = new RLYT
        {
            screenSetting = screenSetting ?? new ScreenSetting
            {
                origin = ScreenOriginType.Classic,
                layoutSize = new Vec2(0f, 0f),
            },
            textureFile = textures.ToArray(),
            fontFile = fonts.ToArray(),
            paneSet = panes.ToArray(),
            paneHierarchy = paneRoots.Count > 0 ? new PaneHierarchy { paneTree = ToPaneTree(paneRoots[0]) } : null!,
            groupSet = groupRoots.Count > 0 ? new GroupSet { group = ToGroup(groupRoots[0]) } : null!,
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
                comment = "Generated from BRLYT. Material reconstruction is partial.",
                generator = new HeadGenerator { name = "LayoutConverter", version = "0.1.0" },
            },
            body = new DocumentBody { rlyt = rlyt },
        };
    }

    private static ScreenSetting ReadLyt1(BinaryLayoutSection section, byte[] bytes)
    {
        int payload = checked((int)section.PayloadOffset);
        return new ScreenSetting
        {
            origin = ReadByte(bytes, payload) == 1 ? ScreenOriginType.Normal : ScreenOriginType.Classic,
            layoutSize = new Vec2(ReadSingle(bytes, payload + 4), ReadSingle(bytes, payload + 8)),
        };
    }

    private static IReadOnlyList<string> ReadStringTable(BinaryLayoutSection section, byte[] bytes)
    {
        int payload = checked((int)section.PayloadOffset);
        int count = ReadUInt16(bytes, payload);
        int offsetsStart = payload + 4;
        var values = new string[count];
        for (int i = 0; i < count; i++)
        {
            uint relativeOffset = ReadUInt32(bytes, offsetsStart + i * 8);
            values[i] = ReadCString(bytes, checked(offsetsStart + (int)relativeOffset));
        }

        return values;
    }

    private static IReadOnlyList<Material> ReadMaterials(BinaryLayoutSection section, byte[] bytes, IReadOnlyList<TextureFile> textures)
    {
        int payload = checked((int)section.PayloadOffset);
        int count = ReadUInt16(bytes, payload);
        var materials = new Material[count];
        for (int i = 0; i < count; i++)
        {
            int entryOffset = checked((int)section.Offset) + checked((int)ReadUInt32(bytes, payload + 4 + i * 4));
            materials[i] = ReadMaterial(bytes, entryOffset, textures);
        }

        return materials;
    }

    private static Material ReadMaterial(byte[] bytes, int offset, IReadOnlyList<TextureFile> textures)
    {
        uint flags = ReadUInt32(bytes, offset + 60);
        int texMapCount = (int)(flags & 0x0F);
        int texMatrixCount = (int)((flags >> 4) & 0x0F);
        int texCoordGenCount = (int)((flags >> 8) & 0x0F);

        var material = new Material
        {
            name = ReadFixedAscii(bytes, offset, 20),
            blackColor = ReadBlackColorRegister(bytes, offset + 20),
            whiteColor = ReadWhiteColorRegister(bytes, offset + 28),
            texMap = new TexMap[texMapCount],
            texMatrix = new TexMatrix[texMatrixCount],
            texCoordGen = new TexCoordGen[texCoordGenCount],
        };

        int cursor = offset + 64;
        for (int i = 0; i < texMapCount; i++)
        {
            material.texMap[i] = ReadTexMap(bytes, cursor + i * 4, textures);
        }

        cursor += texMapCount * 4;
        for (int i = 0; i < texMatrixCount; i++)
        {
            material.texMatrix[i] = ReadTexMatrix(bytes, cursor + i * 20);
        }

        cursor += texMatrixCount * 20;
        for (int i = 0; i < texCoordGenCount; i++)
        {
            material.texCoordGen[i] = ReadTexCoordGen(bytes, cursor + i * 4);
        }

        return material;
    }

    private static Pane ReadPane(BinaryLayoutSection section, byte[] bytes, IReadOnlyList<Material> materials, IReadOnlyList<FontFile> fonts)
    {
        int payload = checked((int)section.PayloadOffset);
        byte flags = ReadByte(bytes, payload);
        var pane = new Pane
        {
            kind = MapPaneKind(section.Magic),
            name = ReadFixedAscii(bytes, payload + 4, 16),
            visible = (flags & 0x01) != 0,
            influencedAlpha = (flags & 0x02) != 0,
            locationAdjust = (flags & 0x04) != 0,
            alpha = ReadByte(bytes, payload + 2),
            basePositionType = MapBasePosition(ReadByte(bytes, payload + 1)),
            translate = new Vec3(ReadSingle(bytes, payload + 28), ReadSingle(bytes, payload + 32), ReadSingle(bytes, payload + 36)),
            rotate = new Vec3(ReadSingle(bytes, payload + 40), ReadSingle(bytes, payload + 44), ReadSingle(bytes, payload + 48)),
            scale = new Vec2(ReadSingle(bytes, payload + 52), ReadSingle(bytes, payload + 56)),
            size = new Vec2(ReadSingle(bytes, payload + 60), ReadSingle(bytes, payload + 64)),
        };

        switch (section.Magic)
        {
            case "pic1":
                pane.Item = ReadPicture(bytes, payload + PaneCommonPayloadSize, materials);
                break;
            case "txt1":
                pane.Item = ReadTextBox(bytes, payload, materials, fonts);
                break;
            case "wnd1":
                pane.Item = ReadWindow(bytes, payload, materials);
                break;
            case "bnd1":
                pane.Item = new Bounding();
                break;
        }

        return pane;
    }

    private static Picture ReadPicture(byte[] bytes, int payload, IReadOnlyList<Material> materials)
    {
        int materialIndex = ReadUInt16(bytes, payload + 16);
        int texCoordCount = ReadByte(bytes, payload + 18);
        var picture = new Picture
        {
            vtxColLT = ReadColor(bytes, payload),
            vtxColRT = ReadColor(bytes, payload + 4),
            vtxColLB = ReadColor(bytes, payload + 8),
            vtxColRB = ReadColor(bytes, payload + 12),
            material = CloneMaterial(GetMaterial(materials, materialIndex)),
            texCoord = new TexCoord[texCoordCount],
        };

        int texCoordOffset = payload + 20;
        for (int i = 0; i < texCoordCount; i++)
        {
            picture.texCoord[i] = ReadTexCoord(bytes, texCoordOffset + i * 32);
        }

        return picture;
    }

    private static TextBox ReadTextBox(byte[] bytes, int panePayload, IReadOnlyList<Material> materials, IReadOnlyList<FontFile> fonts)
    {
        int payload = panePayload + PaneCommonPayloadSize;
        int materialIndex = ReadUInt16(bytes, payload);
        int fontIndex = ReadUInt16(bytes, payload + 2);
        int writtenBytes = ReadUInt16(bytes, payload + 4);
        int storedBytes = ReadUInt16(bytes, payload + 6);
        uint textOffset = ReadUInt32(bytes, payload + 12);

        var textBox = new TextBox
        {
            material = CloneMaterial(GetMaterial(materials, materialIndex)),
            font = GetFontName(fonts, fontIndex),
            topColor = ReadColor(bytes, payload + 16),
            bottomColor = ReadColor(bytes, payload + 20),
            fontSize = new Vec2(ReadSingle(bytes, payload + 24), ReadSingle(bytes, payload + 28)),
            charSpace = ReadSingle(bytes, payload + 32),
            lineSpace = ReadSingle(bytes, payload + 36),
            positionType = MapBasePosition(ReadByte(bytes, payload + 8)),
            textAlignment = MapTextAlignment(ReadByte(bytes, payload + 9)),
            text = writtenBytes > 0 && textOffset > 0 ? ReadUtf16BeString(bytes, checked(panePayload + (int)textOffset), writtenBytes) : string.Empty,
        };

        if (storedBytes > 0)
        {
            textBox.allocateStringLength = (uint)Math.Max(0, (storedBytes / 2) - 1);
            textBox.allocateStringLengthSpecified = true;
        }

        return textBox;
    }

    private static Window ReadWindow(byte[] bytes, int panePayload, IReadOnlyList<Material> materials)
    {
        int payload = panePayload + PaneCommonPayloadSize;
        int frameCount = ReadByte(bytes, payload + 16);
        uint contentOffset = ReadUInt32(bytes, payload + 20);
        uint frameTableOffset = ReadUInt32(bytes, payload + 24);
        var window = new Window
        {
            contentInflation = new InflationRect
            {
                l = ReadSingle(bytes, payload),
                r = ReadSingle(bytes, payload + 4),
                t = ReadSingle(bytes, payload + 8),
                b = ReadSingle(bytes, payload + 12),
            },
            frame = new WindowFrame[frameCount],
        };

        if (contentOffset > 0)
        {
            window.content = ReadWindowContent(bytes, checked(panePayload + (int)contentOffset), materials);
        }

        for (int i = 0; i < frameCount; i++)
        {
            int frameRecordOffset = frameTableOffset > 0
                ? checked(panePayload + (int)ReadUInt32(bytes, checked(panePayload + (int)frameTableOffset) + i * 4))
                : payload + WindowPayloadSize + (i * 4);

            window.frame[i] = new WindowFrame
            {
                frameType = MapWindowFrameType(i),
                material = CloneMaterial(GetMaterial(materials, ReadUInt16(bytes, frameRecordOffset))),
                textureFlip = [MapTextureFlip(ReadUInt16(bytes, frameRecordOffset + 2))],
            };
        }

        return window;
    }

    private static WindowContent ReadWindowContent(byte[] bytes, int payload, IReadOnlyList<Material> materials)
    {
        int materialIndex = ReadUInt16(bytes, payload + 16);
        int texCoordCount = ReadByte(bytes, payload + 18);
        var content = new WindowContent
        {
            vtxColLT = ReadColor(bytes, payload),
            vtxColRT = ReadColor(bytes, payload + 4),
            vtxColLB = ReadColor(bytes, payload + 8),
            vtxColRB = ReadColor(bytes, payload + 12),
            material = CloneMaterial(GetMaterial(materials, materialIndex)),
            texCoord = new TexCoord[texCoordCount],
        };

        int texCoordOffset = payload + WindowContentPayloadSize;
        for (int i = 0; i < texCoordCount; i++)
        {
            content.texCoord[i] = ReadTexCoord(bytes, texCoordOffset + i * 32);
        }

        return content;
    }

    private static Group ReadGroup(BinaryLayoutSection section, byte[] bytes)
    {
        int payload = checked((int)section.PayloadOffset);
        int paneRefCount = ReadUInt16(bytes, payload + 16);
        var group = new Group
        {
            name = ReadFixedAscii(bytes, payload, 16),
            paneRef = new GroupPaneRef[paneRefCount],
        };

        for (int i = 0; i < paneRefCount; i++)
        {
            group.paneRef[i] = new GroupPaneRef { name = ReadFixedAscii(bytes, payload + 20 + i * 16, 16) };
        }

        return group;
    }

    private static PaneTree ToPaneTree(PaneNode node)
        => new()
        {
            name = node.Name,
            paneTree = node.Children.Select(ToPaneTree).ToArray(),
        };

    private static Group ToGroup(GroupNode node)
    {
        node.Group.group = node.Children.Select(ToGroup).ToArray();
        return node.Group;
    }

    private static PaneKind MapPaneKind(string magic)
        => magic switch
        {
            "pic1" => PaneKind.Picture,
            "txt1" => PaneKind.TextBox,
            "wnd1" => PaneKind.Window,
            "bnd1" => PaneKind.Bounding,
            _ => PaneKind.Null,
        };

    private static Position MapBasePosition(byte value)
        => new()
        {
            x = (value % 3) switch
            {
                1 => HorizontalPosition.Center,
                2 => HorizontalPosition.Right,
                _ => HorizontalPosition.Left,
            },
            y = (value / 3) switch
            {
                1 => VerticalPosition.Center,
                2 => VerticalPosition.Bottom,
                _ => VerticalPosition.Top,
            },
        };

    private static Material GetMaterial(IReadOnlyList<Material> materials, int index)
        => (uint)index < (uint)materials.Count ? materials[index] : new Material($"Material{index}");

    private static Material CloneMaterial(Material source)
        => new()
        {
            name = source.name,
            blackColor = source.blackColor is null ? null! : new BlackColor(source.blackColor.r, source.blackColor.g, source.blackColor.b) { a = source.blackColor.a },
            whiteColor = source.whiteColor is null ? null! : new WhiteColor(source.whiteColor.r, source.whiteColor.g, source.whiteColor.b, source.whiteColor.a),
            texMap = source.texMap?.Select(CloneTexMap).ToArray() ?? Array.Empty<TexMap>(),
            texMatrix = source.texMatrix?.Select(CloneTexMatrix).ToArray() ?? Array.Empty<TexMatrix>(),
            texCoordGen = source.texCoordGen?.Select(CloneTexCoordGen).ToArray() ?? Array.Empty<TexCoordGen>(),
        };

    private static TexMap CloneTexMap(TexMap source)
        => new()
        {
            imageName = source.imageName,
            paletteName = source.paletteName,
            wrap_s = source.wrap_s,
            wrap_t = source.wrap_t,
            minFilter = source.minFilter,
            magFilter = source.magFilter,
        };

    private static TexMatrix CloneTexMatrix(TexMatrix source)
        => new()
        {
            translate = source.translate is null ? null! : new Vec2(source.translate.x, source.translate.y),
            scale = source.scale is null ? null! : new Vec2(source.scale.x, source.scale.y),
            rotate = source.rotate,
        };

    private static TexCoordGen CloneTexCoordGen(TexCoordGen source)
        => new()
        {
            func = source.func,
            srcParam = source.srcParam,
            matrix = source.matrix,
        };

    private static BlackColor ReadBlackColorRegister(byte[] bytes, int offset)
        => new(ClampToByte(ReadInt16(bytes, offset)), ClampToByte(ReadInt16(bytes, offset + 2)), ClampToByte(ReadInt16(bytes, offset + 4)))
        {
            a = ClampToByte(ReadInt16(bytes, offset + 6)),
        };

    private static WhiteColor ReadWhiteColorRegister(byte[] bytes, int offset)
        => new(
            ClampToByte(ReadInt16(bytes, offset)),
            ClampToByte(ReadInt16(bytes, offset + 2)),
            ClampToByte(ReadInt16(bytes, offset + 4)),
            ClampToByte(ReadInt16(bytes, offset + 6)));

    private static TexMap ReadTexMap(byte[] bytes, int offset, IReadOnlyList<TextureFile> textures)
    {
        int textureIndex = ReadUInt16(bytes, offset);
        byte packed2 = ReadByte(bytes, offset + 2);
        byte packed3 = ReadByte(bytes, offset + 3);
        return new TexMap
        {
            imageName = GetTextureName(textures, textureIndex),
            wrap_s = MapTexWrapMode(packed2 & 0x03),
            wrap_t = MapTexWrapMode(packed3 & 0x03),
            minFilter = MapTexFilter((packed2 >> 2) & 0x07),
            magFilter = MapTexFilter((packed3 >> 2) & 0x01),
        };
    }

    private static TexMatrix ReadTexMatrix(byte[] bytes, int offset)
        => new()
        {
            translate = new Vec2(ReadSingle(bytes, offset), ReadSingle(bytes, offset + 4)),
            rotate = ReadSingle(bytes, offset + 8),
            scale = new Vec2(ReadSingle(bytes, offset + 12), ReadSingle(bytes, offset + 16)),
        };

    private static TexCoordGen ReadTexCoordGen(byte[] bytes, int offset)
        => new()
        {
            func = TexGenType.Mtx2x4,
            srcParam = MapTexGenSource(ReadByte(bytes, offset + 1)),
            matrix = MapTexGenMatrix(ReadByte(bytes, offset + 2)),
        };

    private static string GetTextureName(IReadOnlyList<TextureFile> textures, int index)
        => (uint)index < (uint)textures.Count ? textures[index].GetName() : $"Texture{index}";

    private static string GetFontName(IReadOnlyList<FontFile> fonts, int index)
        => (uint)index < (uint)fonts.Count ? fonts[index].GetName() : $"Font{index}";

    private static TexWrapMode MapTexWrapMode(int value)
        => value switch
        {
            1 => TexWrapMode.Repeat,
            2 => TexWrapMode.Mirror,
            _ => TexWrapMode.Clamp,
        };

    private static TexFilter MapTexFilter(int value)
        => value == 0 ? TexFilter.Linear : TexFilter.Near;

    private static TexGenSrc MapTexGenSource(byte value)
        => value switch
        {
            5 => TexGenSrc.Tex1,
            6 => TexGenSrc.Tex2,
            7 => TexGenSrc.Tex3,
            8 => TexGenSrc.Tex4,
            9 => TexGenSrc.Tex5,
            10 => TexGenSrc.Tex6,
            11 => TexGenSrc.Tex7,
            _ => TexGenSrc.Tex0,
        };

    private static sbyte MapTexGenMatrix(byte value)
        => value switch
        {
            30 => 0,
            33 => 1,
            36 => 2,
            39 => 3,
            42 => 4,
            45 => 5,
            48 => 6,
            51 => 7,
            _ => -1,
        };

    private static TextAlignment MapTextAlignment(byte value)
        => value switch
        {
            1 => TextAlignment.Left,
            2 => TextAlignment.Center,
            3 => TextAlignment.Right,
            _ => TextAlignment.Synchronous,
        };

    private static WindowFrameType MapWindowFrameType(int index)
        => index switch
        {
            1 => WindowFrameType.CornerRT,
            2 => WindowFrameType.CornerLB,
            3 => WindowFrameType.CornerRB,
            4 => WindowFrameType.FrameL,
            5 => WindowFrameType.FrameR,
            6 => WindowFrameType.FrameT,
            7 => WindowFrameType.FrameB,
            _ => WindowFrameType.CornerLT,
        };

    private static TextureFlip MapTextureFlip(int value)
        => value switch
        {
            1 => TextureFlip.FlipH,
            2 => TextureFlip.FlipV,
            3 => TextureFlip.Rotate90,
            4 => TextureFlip.Rotate180,
            5 => TextureFlip.Rotate270,
            _ => TextureFlip.None,
        };

    private static byte ClampToByte(short value)
        => (byte)Math.Clamp(value, (short)0, (short)byte.MaxValue);

    private static Color4 ReadColor(byte[] bytes, int offset)
        => new(ReadByte(bytes, offset), ReadByte(bytes, offset + 1), ReadByte(bytes, offset + 2), ReadByte(bytes, offset + 3));

    private static TexCoord ReadTexCoord(byte[] bytes, int offset)
        => new()
        {
            texLT = new TexVec2(ReadSingle(bytes, offset), ReadSingle(bytes, offset + 4)),
            texRT = new TexVec2(ReadSingle(bytes, offset + 8), ReadSingle(bytes, offset + 12)),
            texLB = new TexVec2(ReadSingle(bytes, offset + 16), ReadSingle(bytes, offset + 20)),
            texRB = new TexVec2(ReadSingle(bytes, offset + 24), ReadSingle(bytes, offset + 28)),
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

    private static string ReadUtf16BeString(byte[] bytes, int offset, int byteCount)
    {
        EnsureRange(bytes, offset, byteCount);
        string value = System.Text.Encoding.BigEndianUnicode.GetString(bytes, offset, byteCount);
        int terminator = value.IndexOf('\0');
        return terminator >= 0 ? value[..terminator] : value;
    }

    private static void EnsureRange(byte[] bytes, int offset, int count)
    {
        if (offset < 0 || count < 0 || offset > bytes.Length - count)
        {
            throw new EndOfStreamException();
        }
    }

    private sealed record PaneNode(string Name)
    {
        public List<PaneNode> Children { get; } = new();
    }

    private sealed record GroupNode(Group Group)
    {
        public List<GroupNode> Children { get; } = new();
    }
}
