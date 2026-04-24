using System.Buffers.Binary;
using System.Globalization;
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
        var materials = new List<MaterialSlot>();
        var panes = new List<Pane>();
        var paneRoots = new List<PaneNode>();
        var paneStack = new Stack<PaneNode>();
        PaneNode? lastPaneNode = null;
        var groups = new List<Group>();
        var groupRoots = new List<GroupNode>();
        var groupStack = new Stack<GroupNode>();
        GroupNode? lastGroupNode = null;
        object? lastUserDataTarget = null;
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
                lastUserDataTarget = pane;
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
            case "usd1":
                object[] userData = ReadUserData(section, bytes);
                switch (lastUserDataTarget)
                {
                    case Pane targetPane:
                        targetPane.userData = userData;
                        break;
                    case Group targetGroup:
                        targetGroup.userData = userData;
                        break;
                }

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
                lastUserDataTarget = group;
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
            paneSet = OrderPanesForMaterialTable(panes, materials),
            paneHierarchy = paneRoots.Count > 0 ? new PaneHierarchy { paneTree = ToPaneTree(paneRoots[0]) } : null!,
            groupSet = groupRoots.Count > 0 ? new GroupSet { group = ToGroup(groupRoots[0]) } : null!,
        };

        return new Document
        {
            version = MapXmlVersion(container.Version),
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
                comment = "Generated from BRLYT.",
                generator = new HeadGenerator { name = "LayoutConverter", version = "0.1.0" },
            },
            body = new DocumentBody { rlyt = rlyt },
        };
    }

    private static string MapXmlVersion(ushort binaryVersion)
        => binaryVersion switch
        {
            0x0008 => "1.0.0",
            0x0009 => "1.1.0",
            0x000A => "1.2.0",
            _ => $"1.{Math.Max(0, binaryVersion - 0x0008)}.0",
        };

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

    private static IReadOnlyList<MaterialSlot> ReadMaterials(BinaryLayoutSection section, byte[] bytes, IReadOnlyList<TextureFile> textures)
    {
        int payload = checked((int)section.PayloadOffset);
        int count = ReadUInt16(bytes, payload);
        var materials = new MaterialSlot[count];
        for (int i = 0; i < count; i++)
        {
            int entryOffset = checked((int)section.Offset) + checked((int)ReadUInt32(bytes, payload + 4 + i * 4));
            materials[i] = ReadMaterial(bytes, entryOffset, textures);
        }

        return materials;
    }

    private static MaterialSlot ReadMaterial(byte[] bytes, int offset, IReadOnlyList<TextureFile> textures)
    {
        uint flags = ReadUInt32(bytes, offset + 60);
        int texMapCount = (int)(flags & 0x0F);
        int texMatrixCount = (int)((flags >> 4) & 0x0F);
        int texCoordGenCount = (int)((flags >> 8) & 0x0F);
        bool hasRevoPayload = ((flags >> 18) & 0x1F) > 0
            || ((flags >> 23) & 0x1F) != 0
            || ((flags >> 13) & 0x1F) != 0;

        var material = new Material
        {
            name = ReadFixedAscii(bytes, offset, 20),
            blackColor = ReadBlackColorRegister(bytes, offset + 20),
            whiteColor = ReadWhiteColorRegister(bytes, offset + 28),
            texMap = new TexMap[texMapCount],
            texMatrix = new TexMatrix[texMatrixCount],
            texCoordGen = new TexCoordGen[texCoordGenCount],
        };

        var tevColorRegisters = new[]
        {
            ReadColorS10_4(bytes, offset + 20),
            ReadColorS10_4(bytes, offset + 28),
            ReadColorS10_4(bytes, offset + 36),
        };
        var tevConstantRegisters = new[]
        {
            ReadColor(bytes, offset + 44),
            ReadColor(bytes, offset + 48),
            ReadColor(bytes, offset + 52),
            ReadColor(bytes, offset + 56),
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

        cursor += texCoordGenCount * 4;
        Material_Revo? materialRevo = hasRevoPayload
            ? ReadRevoMaterial(bytes, cursor, flags, material, tevColorRegisters, tevConstantRegisters)
            : null;

        return new MaterialSlot(material, materialRevo);
    }

    private static Material_Revo ReadRevoMaterial(
        byte[] bytes,
        int payload,
        uint flags,
        Material material,
        ColorS10_4[] tevColorRegisters,
        Color4[] tevConstantRegisters)
    {
        int indirectMatrixCount = (int)((flags >> 13) & 0x03);
        int indirectStageCount = (int)((flags >> 15) & 0x07);
        int tevStageCount = (int)((flags >> 18) & 0x1F);
        bool hasSwapTable = ((flags >> 12) & 0x01) != 0;
        bool hasAlphaCompare = ((flags >> 23) & 0x01) != 0;
        bool hasBlendMode = ((flags >> 24) & 0x01) != 0;
        bool hasChannelControl = ((flags >> 25) & 0x01) != 0;
        bool hasMaterialColorRegister = ((flags >> 27) & 0x01) != 0;

        var materialRevo = new Material_Revo
        {
            name = material.name,
            texMap = material.texMap,
            texMatrix = material.texMatrix,
            texCoordGen = material.texCoordGen,
            tevColReg = tevColorRegisters,
            tevConstReg = tevConstantRegisters,
            tevStageNum = (byte)Math.Min(tevStageCount, byte.MaxValue),
            indirectStageNum = (byte)Math.Min(indirectStageCount, byte.MaxValue),
        };

        int cursor = payload;
        if (hasChannelControl)
        {
            materialRevo.channelControl = ReadChannelControls(bytes, cursor);
            cursor += 4;
        }

        if (hasMaterialColorRegister)
        {
            materialRevo.matColReg = ReadColor(bytes, cursor);
            cursor += 4;
        }

        if (hasSwapTable)
        {
            materialRevo.swapTable = ReadSwapTables(bytes, cursor);
            cursor += 4;
        }

        if (indirectMatrixCount > 0)
        {
            materialRevo.indirectMatrix = new TexMatrix[indirectMatrixCount];
            for (int i = 0; i < indirectMatrixCount; i++)
            {
                materialRevo.indirectMatrix[i] = ReadTexMatrix(bytes, cursor + i * 20);
            }

            cursor += indirectMatrixCount * 20;
        }

        if (indirectStageCount > 0)
        {
            materialRevo.indirectStage = new Material_RevoIndirectStage[indirectStageCount];
            for (int i = 0; i < indirectStageCount; i++)
            {
                materialRevo.indirectStage[i] = ReadIndirectStage(bytes, cursor + i * 4);
            }

            cursor += indirectStageCount * 4;
        }

        if (tevStageCount > 0)
        {
            int count = Math.Min(tevStageCount, 16);
            materialRevo.tevStage = new Material_RevoTevStage[count];
            for (int i = 0; i < count; i++)
            {
                materialRevo.tevStage[i] = ReadTevStage(bytes, cursor + i * 16);
            }

            cursor += count * 16;
        }

        if (hasAlphaCompare)
        {
            materialRevo.alphaCompare = ReadAlphaCompare(bytes, cursor);
            cursor += 4;
        }

        if (hasBlendMode)
        {
            materialRevo.blendMode = ReadBlendMode(bytes, cursor);
        }

        return materialRevo;
    }

    private static Pane ReadPane(BinaryLayoutSection section, byte[] bytes, IReadOnlyList<MaterialSlot> materials, IReadOnlyList<FontFile> fonts)
    {
        int sectionOffset = checked((int)section.Offset);
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
            binaryReservedBytes = ReadPaneReservedBytes(bytes, payload + 20),
        };

        switch (section.Magic)
        {
            case "pic1":
                pane.Item = ReadPicture(bytes, payload + PaneCommonPayloadSize, materials);
                break;
            case "txt1":
                pane.Item = ReadTextBox(bytes, pane.name, sectionOffset, checked(sectionOffset + (int)section.Size), payload, materials, fonts);
                break;
            case "wnd1":
                pane.Item = ReadWindow(bytes, sectionOffset, payload, materials);
                break;
            case "bnd1":
                pane.Item = new Bounding();
                break;
        }

        return pane;
    }

    private static Picture ReadPicture(byte[] bytes, int payload, IReadOnlyList<MaterialSlot> materials)
    {
        int materialIndex = ReadUInt16(bytes, payload + 16);
        int texCoordCount = ReadByte(bytes, payload + 18);
        MaterialSlot material = GetMaterial(materials, materialIndex);
        var picture = new Picture
        {
            vtxColLT = ReadColor(bytes, payload),
            vtxColRT = ReadColor(bytes, payload + 4),
            vtxColLB = ReadColor(bytes, payload + 8),
            vtxColRB = ReadColor(bytes, payload + 12),
            material = CloneMaterial(material.Material),
            materialRevo = CloneMaterialRevo(material.MaterialRevo)!,
            detailSetting = material.MaterialRevo is not null,
            texCoord = new TexCoord[texCoordCount],
        };

        int texCoordOffset = payload + 20;
        for (int i = 0; i < texCoordCount; i++)
        {
            picture.texCoord[i] = ReadTexCoord(bytes, texCoordOffset + i * 32);
        }

        return picture;
    }

    private static TextBox ReadTextBox(byte[] bytes, string? paneName, int sectionOffset, int sectionEndOffset, int panePayload, IReadOnlyList<MaterialSlot> materials, IReadOnlyList<FontFile> fonts)
    {
        int payload = panePayload + PaneCommonPayloadSize;
        int materialIndex = ReadUInt16(bytes, payload);
        int fontIndex = ReadUInt16(bytes, payload + 2);
        int writtenBytes = ReadUInt16(bytes, payload + 4);
        int storedBytes = ReadUInt16(bytes, payload + 6);
        uint textOffset = ReadUInt32(bytes, payload + 12);

        MaterialSlot material = ResolveTextBoxMaterial(materials, materialIndex, paneName);
        var textBox = new TextBox
        {
            material = CloneMaterial(material.Material),
            materialRevo = CloneMaterialRevo(material.MaterialRevo)!,
            font = GetFontName(fonts, fontIndex),
            topColor = ReadColor(bytes, payload + 16),
            bottomColor = ReadColor(bytes, payload + 20),
            fontSize = new Vec2(ReadSingle(bytes, payload + 24), ReadSingle(bytes, payload + 28)),
            charSpace = ReadSingle(bytes, payload + 32),
            lineSpace = ReadSingle(bytes, payload + 36),
            positionType = MapBasePosition(ReadByte(bytes, payload + 8)),
            textAlignment = MapTextAlignment(ReadByte(bytes, payload + 9)),
            text = writtenBytes > 0 && textOffset > 0 ? ReadUtf16BeString(bytes, checked(sectionOffset + (int)textOffset), sectionEndOffset) : string.Empty,
        };

        if ((uint)materialIndex >= (uint)materials.Count)
        {
            textBox.binaryMaterialIndex = materialIndex;
            textBox.binaryMaterialIndexSpecified = true;
        }

        textBox.binaryWrittenBytes = writtenBytes;
        textBox.binaryWrittenBytesSpecified = true;
        textBox.binaryStoredBytes = storedBytes;
        textBox.binaryStoredBytesSpecified = true;

        if (storedBytes > 0)
        {
            textBox.allocateStringLength = (uint)Math.Max(0, (storedBytes / 2) - 1);
            textBox.allocateStringLengthSpecified = true;
        }

        return textBox;
    }

    private static Window ReadWindow(byte[] bytes, int sectionOffset, int panePayload, IReadOnlyList<MaterialSlot> materials)
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
            window.content = ReadWindowContent(bytes, checked(sectionOffset + (int)contentOffset), materials);
        }

        for (int i = 0; i < frameCount; i++)
        {
            int frameRecordOffset = frameTableOffset > 0
                ? checked(sectionOffset + (int)ReadUInt32(bytes, checked(sectionOffset + (int)frameTableOffset) + i * 4))
                : payload + WindowPayloadSize + (i * 4);

            window.frame[i] = new WindowFrame
            {
                frameType = MapWindowFrameType(i),
                material = CloneMaterial(GetMaterial(materials, ReadUInt16(bytes, frameRecordOffset)).Material),
                materialRevo = CloneMaterialRevo(GetMaterial(materials, ReadUInt16(bytes, frameRecordOffset)).MaterialRevo)!,
                detailSetting = GetMaterial(materials, ReadUInt16(bytes, frameRecordOffset)).MaterialRevo is not null,
                textureFlip = [MapTextureFlip(ReadUInt16(bytes, frameRecordOffset + 2))],
            };
        }

        return window;
    }

    private static WindowContent ReadWindowContent(byte[] bytes, int payload, IReadOnlyList<MaterialSlot> materials)
    {
        int materialIndex = ReadUInt16(bytes, payload + 16);
        int texCoordCount = ReadByte(bytes, payload + 18);
        MaterialSlot material = GetMaterial(materials, materialIndex);
        var content = new WindowContent
        {
            vtxColLT = ReadColor(bytes, payload),
            vtxColRT = ReadColor(bytes, payload + 4),
            vtxColLB = ReadColor(bytes, payload + 8),
            vtxColRB = ReadColor(bytes, payload + 12),
            material = CloneMaterial(material.Material),
            materialRevo = CloneMaterialRevo(material.MaterialRevo)!,
            detailSetting = material.MaterialRevo is not null,
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

    private static object[] ReadUserData(BinaryLayoutSection section, byte[] bytes)
    {
        int payload = checked((int)section.PayloadOffset);
        int count = ReadUInt16(bytes, payload);
        var values = new object[count];
        int descriptorStart = payload + 4;

        for (int i = 0; i < count; i++)
        {
            int descriptorOffset = descriptorStart + i * 12;
            uint nameOffset = ReadUInt32(bytes, descriptorOffset);
            uint valueOffset = ReadUInt32(bytes, descriptorOffset + 4);
            int elementCount = ReadUInt16(bytes, descriptorOffset + 8);
            byte valueType = ReadByte(bytes, descriptorOffset + 10);

            string name = nameOffset == 0 ? string.Empty : ReadCString(bytes, checked(descriptorOffset + (int)nameOffset));
            int absoluteValueOffset = checked(descriptorOffset + (int)valueOffset);
            values[i] = valueType switch
            {
                0 => new UserDataString
                {
                    name = name,
                    Value = ReadCString(bytes, absoluteValueOffset),
                },
                1 => new UserDataIntList
                {
                    name = name,
                    Value = string.Join(' ', Enumerable.Range(0, elementCount)
                        .Select(index => ReadInt32(bytes, absoluteValueOffset + index * 4).ToString(CultureInfo.InvariantCulture))),
                },
                2 => new UserDataFloatList
                {
                    name = name,
                    Value = string.Join(' ', Enumerable.Range(0, elementCount)
                        .Select(index => ReadSingle(bytes, absoluteValueOffset + index * 4).ToString("R", CultureInfo.InvariantCulture))),
                },
                _ => new UserDataString
                {
                    name = name,
                    Value = string.Empty,
                },
            };
        }

        return values;
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

    private static Pane[] OrderPanesForMaterialTable(IReadOnlyList<Pane> panes, IReadOnlyList<MaterialSlot> materials)
    {
        if (materials.Count == 0)
        {
            return panes.ToArray();
        }

        var materialOrder = materials
            .Select(static (slot, index) => (slot.Material.name, index))
            .Where(static item => !string.IsNullOrWhiteSpace(item.name))
            .GroupBy(static item => item.name!, StringComparer.Ordinal)
            .ToDictionary(static group => group.Key, static group => group.First().index, StringComparer.Ordinal);

        return panes
            .Select(static (pane, index) => (pane, index))
            .OrderBy(item => GetPaneMaterialName(item.pane) is { } name && materialOrder.TryGetValue(name, out int materialIndex)
                ? materialIndex
                : int.MaxValue)
            .ThenBy(static item => item.index)
            .Select(static item => item.pane)
            .ToArray();
    }

    private static string? GetPaneMaterialName(Pane pane)
        => pane.Item switch
        {
            Picture picture => picture.materialRevo?.name ?? picture.material?.name,
            TextBox textBox => textBox.materialRevo?.name ?? textBox.material?.name,
            Window { content: { } content } => content.materialRevo?.name ?? content.material?.name,
            _ => null,
        };

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

    private static MaterialSlot GetMaterial(IReadOnlyList<MaterialSlot> materials, int index)
        => (uint)index < (uint)materials.Count ? materials[index] : new MaterialSlot(new Material($"Material{index}"), null);

    private static string ReadPaneReservedBytes(byte[] bytes, int offset)
    {
        Span<byte> reserved = stackalloc byte[8];
        bytes.AsSpan(offset, reserved.Length).CopyTo(reserved);
        return reserved.IndexOfAnyExcept((byte)0) >= 0
            ? Convert.ToHexString(reserved)
            : string.Empty;
    }

    private static MaterialSlot ResolveTextBoxMaterial(IReadOnlyList<MaterialSlot> materials, int index, string? paneName)
    {
        if ((uint)index < (uint)materials.Count)
        {
            return materials[index];
        }

        if (!string.IsNullOrWhiteSpace(paneName))
        {
            foreach (MaterialSlot slot in materials)
            {
                if (string.Equals(slot.Material.name, paneName, StringComparison.Ordinal))
                {
                    return slot;
                }
            }
        }

        return GetMaterial(materials, index);
    }

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

    private static Material_Revo? CloneMaterialRevo(Material_Revo? source)
        => source is null
            ? null
            : new Material_Revo
            {
                name = source.name,
                channelControl = source.channelControl?.Select(static channel => new Material_RevoChannelControl
                {
                    channel = channel.channel,
                    materialSource = channel.materialSource,
                }).ToArray() ?? Array.Empty<Material_RevoChannelControl>(),
                matColReg = source.matColReg,
                tevColReg = source.tevColReg?.Select(static color => new ColorS10_4(color.r, color.g, color.b, color.a)).ToArray() ?? Array.Empty<ColorS10_4>(),
                tevConstReg = source.tevConstReg?.Select(static color => new Color4(color.r, color.g, color.b, color.a)).ToArray() ?? Array.Empty<Color4>(),
                texMap = source.texMap?.Select(CloneTexMap).ToArray() ?? Array.Empty<TexMap>(),
                texMatrix = source.texMatrix?.Select(CloneTexMatrix).ToArray() ?? Array.Empty<TexMatrix>(),
                texCoordGen = source.texCoordGen?.Select(CloneTexCoordGen).ToArray() ?? Array.Empty<TexCoordGen>(),
                swapTable = source.swapTable?.Select(static table => new Material_RevoSwapTable
                {
                    r = table.r,
                    g = table.g,
                    b = table.b,
                    a = table.a,
                }).ToArray() ?? Array.Empty<Material_RevoSwapTable>(),
                indirectMatrix = source.indirectMatrix?.Select(CloneTexMatrix).ToArray() ?? Array.Empty<TexMatrix>(),
                indirectStage = source.indirectStage?.Select(static stage => new Material_RevoIndirectStage
                {
                    texCoordGen = stage.texCoordGen,
                    texMap = stage.texMap,
                    scale_s = stage.scale_s,
                    scale_t = stage.scale_t,
                }).ToArray() ?? Array.Empty<Material_RevoIndirectStage>(),
                tevStage = source.tevStage?.Select(CloneTevStage).ToArray() ?? Array.Empty<Material_RevoTevStage>(),
                alphaCompare = source.alphaCompare is null ? null! : new Material_RevoAlphaCompare
                {
                    comp0 = source.alphaCompare.comp0,
                    ref0 = source.alphaCompare.ref0,
                    op = source.alphaCompare.op,
                    comp1 = source.alphaCompare.comp1,
                    ref1 = source.alphaCompare.ref1,
                },
                blendMode = source.blendMode is null ? null! : new Material_RevoBlendMode
                {
                    type = source.blendMode.type,
                    srcFactor = source.blendMode.srcFactor,
                    dstFactor = source.blendMode.dstFactor,
                    op = source.blendMode.op,
                },
                tevStageNum = source.tevStageNum,
                indirectStageNum = source.indirectStageNum,
                displayFace = source.displayFace,
            };

    private static Material_RevoTevStage CloneTevStage(Material_RevoTevStage source)
        => new()
        {
            colorChannel = source.colorChannel,
            texMap = source.texMap,
            texCoordGen = source.texCoordGen,
            rasColSwap = source.rasColSwap,
            texColSwap = source.texColSwap,
            color = source.color is null ? null! : new Material_RevoTevStageColor
            {
                a = source.color.a,
                b = source.color.b,
                c = source.color.c,
                d = source.color.d,
                konst = source.color.konst,
                op = source.color.op,
                bias = source.color.bias,
                scale = source.color.scale,
                clamp = source.color.clamp,
                outReg = source.color.outReg,
            },
            alpha = source.alpha is null ? null! : new Material_RevoTevStageAlpha
            {
                a = source.alpha.a,
                b = source.alpha.b,
                c = source.alpha.c,
                d = source.alpha.d,
                konst = source.alpha.konst,
                op = source.alpha.op,
                bias = source.alpha.bias,
                scale = source.alpha.scale,
                clamp = source.alpha.clamp,
                outReg = source.alpha.outReg,
            },
            indirect = source.indirect is null ? null! : new Material_RevoTevStageIndirect
            {
                indStage = source.indirect.indStage,
                format = source.indirect.format,
                bias = source.indirect.bias,
                matrix = source.indirect.matrix,
                wrap_s = source.indirect.wrap_s,
                wrap_t = source.indirect.wrap_t,
                addPrev = source.indirect.addPrev,
                utcLod = source.indirect.utcLod,
                alpha = source.indirect.alpha,
            },
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

    private static ColorS10_4 ReadColorS10_4(byte[] bytes, int offset)
        => new(ReadInt16(bytes, offset), ReadInt16(bytes, offset + 2), ReadInt16(bytes, offset + 4), ReadInt16(bytes, offset + 6));

    private static Material_RevoChannelControl[] ReadChannelControls(byte[] bytes, int offset)
        =>
        [
            new Material_RevoChannelControl
            {
                channel = ChannelID.Color0,
                materialSource = ReadByte(bytes, offset) == 0 ? ColorSource.Register : ColorSource.Vertex,
            },
            new Material_RevoChannelControl
            {
                channel = ChannelID.Alpha0,
                materialSource = ReadByte(bytes, offset + 1) == 0 ? ColorSource.Register : ColorSource.Vertex,
            },
        ];

    private static Material_RevoSwapTable[] ReadSwapTables(byte[] bytes, int offset)
    {
        var tables = new Material_RevoSwapTable[4];
        for (int i = 0; i < tables.Length; i++)
        {
            byte packed = ReadByte(bytes, offset + i);
            tables[i] = new Material_RevoSwapTable
            {
                r = MapTevColorChannel(packed & 0x03),
                g = MapTevColorChannel((packed >> 2) & 0x03),
                b = MapTevColorChannel((packed >> 4) & 0x03),
                a = MapTevColorChannel((packed >> 6) & 0x03),
            };
        }

        return tables;
    }

    private static Material_RevoIndirectStage ReadIndirectStage(byte[] bytes, int offset)
        => new()
        {
            texCoordGen = ReadByte(bytes, offset),
            texMap = ReadByte(bytes, offset + 1),
            scale_s = MapIndirectScale(ReadByte(bytes, offset + 2)),
            scale_t = MapIndirectScale(ReadByte(bytes, offset + 3)),
        };

    private static Material_RevoTevStage ReadTevStage(byte[] bytes, int offset)
    {
        byte texCoord = ReadByte(bytes, offset);
        byte colorChannel = ReadByte(bytes, offset + 1);
        byte texMap = ReadByte(bytes, offset + 2);
        byte packed3 = ReadByte(bytes, offset + 3);

        return new Material_RevoTevStage
        {
            texCoordGen = texCoord == 0xFF ? (sbyte)-1 : unchecked((sbyte)texCoord),
            colorChannel = MapTevChannel(colorChannel),
            texMap = texMap == 0xFF ? (sbyte)-1 : unchecked((sbyte)texMap),
            rasColSwap = (sbyte)((packed3 >> 1) & 0x03),
            texColSwap = (sbyte)((packed3 >> 3) & 0x03),
            color = ReadTevStageColor(bytes, offset + 4),
            alpha = ReadTevStageAlpha(bytes, offset + 8),
            indirect = ReadTevStageIndirect(bytes, offset + 12)!,
        };
    }

    private static Material_RevoTevStageColor ReadTevStageColor(byte[] bytes, int offset)
    {
        byte packed0 = ReadByte(bytes, offset);
        byte packed1 = ReadByte(bytes, offset + 1);
        byte packed2 = ReadByte(bytes, offset + 2);
        byte packed3 = ReadByte(bytes, offset + 3);
        return new Material_RevoTevStageColor
        {
            a = MapTevColorArg(packed0 & 0x0F),
            b = MapTevColorArg((packed0 >> 4) & 0x0F),
            c = MapTevColorArg(packed1 & 0x0F),
            d = MapTevColorArg((packed1 >> 4) & 0x0F),
            op = MapTevColorOp(packed2 & 0x0F),
            bias = MapTevBias((packed2 >> 4) & 0x03),
            scale = MapTevScale((packed2 >> 6) & 0x03),
            konst = MapTevKonstColor((packed3 >> 3) & 0x1F),
            outReg = MapTevReg((packed3 >> 1) & 0x03),
            clamp = (packed3 & 0x01) != 0,
        };
    }

    private static Material_RevoTevStageAlpha ReadTevStageAlpha(byte[] bytes, int offset)
    {
        byte packed0 = ReadByte(bytes, offset);
        byte packed1 = ReadByte(bytes, offset + 1);
        byte packed2 = ReadByte(bytes, offset + 2);
        byte packed3 = ReadByte(bytes, offset + 3);
        return new Material_RevoTevStageAlpha
        {
            a = MapTevAlphaArg(packed0 & 0x0F),
            b = MapTevAlphaArg((packed0 >> 4) & 0x0F),
            c = MapTevAlphaArg(packed1 & 0x0F),
            d = MapTevAlphaArg((packed1 >> 4) & 0x0F),
            op = MapTevAlphaOp(packed2 & 0x0F),
            bias = MapTevBias((packed2 >> 4) & 0x03),
            scale = MapTevScale((packed2 >> 6) & 0x03),
            konst = MapTevKonstAlpha((packed3 >> 3) & 0x1F),
            outReg = MapTevReg((packed3 >> 1) & 0x03),
            clamp = (packed3 & 0x01) != 0,
        };
    }

    private static Material_RevoTevStageIndirect? ReadTevStageIndirect(byte[] bytes, int offset)
    {
        byte packed0 = ReadByte(bytes, offset);
        byte packed1 = ReadByte(bytes, offset + 1);
        byte packed2 = ReadByte(bytes, offset + 2);
        byte packed3 = ReadByte(bytes, offset + 3);
        if ((packed0 | packed1 | packed2 | packed3) == 0)
        {
            return null;
        }

        return new Material_RevoTevStageIndirect
        {
            indStage = packed0,
            bias = MapIndTexBias(packed1 & 0x07),
            matrix = MapIndTexMatrix((packed1 >> 3) & 0x0F),
            wrap_s = MapIndTexWrap(packed2 & 0x07),
            wrap_t = MapIndTexWrap((packed2 >> 3) & 0x07),
            format = MapIndTexFormat(packed3 & 0x03),
            addPrev = (packed3 & 0x04) != 0,
            utcLod = (packed3 & 0x08) != 0,
            alpha = MapIndTexAlpha((packed3 >> 4) & 0x03),
        };
    }

    private static Material_RevoAlphaCompare ReadAlphaCompare(byte[] bytes, int offset)
    {
        byte packedCompare = ReadByte(bytes, offset);
        return new Material_RevoAlphaCompare
        {
            comp0 = MapCompare(packedCompare & 0x0F),
            comp1 = MapCompare((packedCompare >> 4) & 0x0F),
            op = MapAlphaOp(ReadByte(bytes, offset + 1)),
            ref0 = ReadByte(bytes, offset + 2),
            ref1 = ReadByte(bytes, offset + 3),
        };
    }

    private static Material_RevoBlendMode ReadBlendMode(byte[] bytes, int offset)
        => new()
        {
            type = MapBlendMode(ReadByte(bytes, offset)),
            srcFactor = MapBlendFactorSrc(ReadByte(bytes, offset + 1)),
            dstFactor = MapBlendFactorDst(ReadByte(bytes, offset + 2)),
            op = MapLogicOp(ReadByte(bytes, offset + 3)),
        };

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

    private static TevChannelID MapTevChannel(byte value)
        => value switch
        {
            6 => TevChannelID.ColorZero,
            0xFF => TevChannelID.ColorNull,
            _ => TevChannelID.Color0a0,
        };

    private static TevColorArg MapTevColorArg(int value)
        => value switch
        {
            0 => TevColorArg.CPrev,
            1 => TevColorArg.APrev,
            2 => TevColorArg.C0,
            3 => TevColorArg.A0,
            4 => TevColorArg.C1,
            5 => TevColorArg.A1,
            6 => TevColorArg.C2,
            7 => TevColorArg.A2,
            8 => TevColorArg.TexC,
            9 => TevColorArg.TexA,
            10 => TevColorArg.RasC,
            11 => TevColorArg.RasA,
            12 => TevColorArg.V1_0,
            13 => TevColorArg.V0_5,
            14 => TevColorArg.Konst,
            15 => TevColorArg.V0,
            _ => TevColorArg.C0,
        };

    private static TevAlphaArg MapTevAlphaArg(int value)
        => value switch
        {
            0 => TevAlphaArg.APrev,
            1 => TevAlphaArg.A0,
            2 => TevAlphaArg.A1,
            3 => TevAlphaArg.A2,
            4 => TevAlphaArg.TexA,
            5 => TevAlphaArg.RasA,
            6 => TevAlphaArg.Konst,
            7 => TevAlphaArg.V0,
            _ => TevAlphaArg.A0,
        };

    private static TevOpC MapTevColorOp(int value)
        => value switch
        {
            0 => TevOpC.Add,
            1 => TevOpC.Sub,
            >= 8 and <= 15 => (TevOpC)(value - 6),
            _ => TevOpC.Add,
        };

    private static TevOpA MapTevAlphaOp(int value)
        => Enum.IsDefined(typeof(TevOpA), value) ? (TevOpA)value : TevOpA.Add;

    private static TevBias MapTevBias(int value)
        => value switch
        {
            1 => TevBias.P0_5,
            2 => TevBias.M0_5,
            _ => TevBias.V0,
        };

    private static TevScale MapTevScale(int value)
        => value switch
        {
            1 => TevScale.V2,
            2 => TevScale.V4,
            3 => TevScale.V1_2,
            _ => TevScale.V1,
        };

    private static TevRegID MapTevReg(int value)
        => value switch
        {
            1 => TevRegID.Reg0,
            2 => TevRegID.Reg1,
            3 => TevRegID.Reg2,
            _ => TevRegID.Prev,
        };

    private static TevKColorSel MapTevKonstColor(int value)
        => value switch
        {
            <= 7 => (TevKColorSel)(value + 20),
            >= 12 and <= 31 => (TevKColorSel)(value - 12),
            _ => TevKColorSel.K0,
        };

    private static TevKAlphaSel MapTevKonstAlpha(int value)
        => value switch
        {
            <= 7 => (TevKAlphaSel)(value + 16),
            >= 16 and <= 31 => (TevKAlphaSel)(value - 16),
            _ => TevKAlphaSel.K0_r,
        };

    private static IndTexFormat MapIndTexFormat(int value)
        => Enum.IsDefined(typeof(IndTexFormat), value) ? (IndTexFormat)value : IndTexFormat.V8;

    private static IndTexBiasSel MapIndTexBias(int value)
        => value switch
        {
            3 => IndTexBiasSel.ST,
            4 => IndTexBiasSel.U,
            _ => Enum.IsDefined(typeof(IndTexBiasSel), value) ? (IndTexBiasSel)value : IndTexBiasSel.None,
        };

    private static IndTexMtxID MapIndTexMatrix(int value)
        => value switch
        {
            5 => IndTexMtxID.S0,
            6 => IndTexMtxID.S1,
            7 => IndTexMtxID.S2,
            9 => IndTexMtxID.T0,
            10 => IndTexMtxID.T1,
            11 => IndTexMtxID.T2,
            _ => Enum.IsDefined(typeof(IndTexMtxID), value) ? (IndTexMtxID)value : IndTexMtxID.Off,
        };

    private static IndTexWrap MapIndTexWrap(int value)
        => Enum.IsDefined(typeof(IndTexWrap), value) ? (IndTexWrap)value : IndTexWrap.Off;

    private static IndTexAlphaSel MapIndTexAlpha(int value)
        => Enum.IsDefined(typeof(IndTexAlphaSel), value) ? (IndTexAlphaSel)value : IndTexAlphaSel.Off;

    private static TevColorChannel MapTevColorChannel(int value)
        => value switch
        {
            1 => TevColorChannel.Green,
            2 => TevColorChannel.Blue,
            3 => TevColorChannel.Alpha,
            _ => TevColorChannel.Red,
        };

    private static IndTexScale MapIndirectScale(int value)
        => value switch
        {
            1 => IndTexScale.V2,
            2 => IndTexScale.V4,
            3 => IndTexScale.V8,
            4 => IndTexScale.V16,
            5 => IndTexScale.V32,
            6 => IndTexScale.V64,
            7 => IndTexScale.V128,
            8 => IndTexScale.V256,
            _ => IndTexScale.V1,
        };

    private static Compare MapCompare(int value)
        => value switch
        {
            1 => Compare.Less,
            2 => Compare.Equal,
            3 => Compare.LEqual,
            4 => Compare.Greater,
            5 => Compare.NEqual,
            6 => Compare.GEqual,
            7 => Compare.Always,
            _ => Compare.Never,
        };

    private static AlphaOp MapAlphaOp(int value)
        => value switch
        {
            1 => AlphaOp.Or,
            2 => AlphaOp.Xor,
            3 => AlphaOp.Xnor,
            _ => AlphaOp.And,
        };

    private static BlendMode MapBlendMode(int value)
        => value switch
        {
            1 => BlendMode.Blend,
            2 => BlendMode.Logic,
            3 => BlendMode.Subtract,
            _ => BlendMode.None,
        };

    private static BlendFactorSrc MapBlendFactorSrc(int value)
        => value switch
        {
            1 => BlendFactorSrc.V1_0,
            2 => BlendFactorSrc.DstClr,
            3 => BlendFactorSrc.InvDstClr,
            4 => BlendFactorSrc.SrcAlpha,
            5 => BlendFactorSrc.InvSrcAlpha,
            6 => BlendFactorSrc.DstAlpha,
            7 => BlendFactorSrc.InvDstAlpha,
            _ => BlendFactorSrc.V0,
        };

    private static BlendFactorDst MapBlendFactorDst(int value)
        => value switch
        {
            1 => BlendFactorDst.V1_0,
            2 => BlendFactorDst.SrcClr,
            3 => BlendFactorDst.InvSrcClr,
            4 => BlendFactorDst.SrcAlpha,
            5 => BlendFactorDst.InvSrcAlpha,
            6 => BlendFactorDst.DstAlpha,
            7 => BlendFactorDst.InvDstAlpha,
            _ => BlendFactorDst.V0,
        };

    private static LogicOp MapLogicOp(int value)
        => value switch
        {
            0 => LogicOp.Clear,
            1 => LogicOp.And,
            2 => LogicOp.RevAnd,
            3 => LogicOp.Copy,
            4 => LogicOp.InvAnd,
            5 => LogicOp.NoOp,
            6 => LogicOp.Xor,
            7 => LogicOp.Or,
            8 => LogicOp.Nor,
            9 => LogicOp.Equiv,
            10 => LogicOp.Inv,
            11 => LogicOp.RevOr,
            12 => LogicOp.InvCopy,
            13 => LogicOp.InvOr,
            14 => LogicOp.Nand,
            _ => LogicOp.Set,
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

    private static int ReadInt32(byte[] bytes, int offset)
    {
        EnsureRange(bytes, offset, 4);
        return BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(offset, 4));
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

    private static string ReadUtf16BeString(byte[] bytes, int offset, int endOffset)
    {
        int byteCount = 0;
        while (offset + byteCount + 1 < endOffset)
        {
            if (bytes[offset + byteCount] == 0 && bytes[offset + byteCount + 1] == 0)
            {
                byteCount += 2;
                break;
            }

            byteCount += 2;
        }

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

    private sealed record MaterialSlot(Material Material, Material_Revo? MaterialRevo);
}
