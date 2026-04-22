using System.Globalization;
using System.Text;
using LayoutConverter.Core.IO;
using LayoutConverter.Core.Models;
using LayoutConverter.Core.Schema.Rlyt;

namespace LayoutConverter.Core.Brlyt;

public sealed class BrlytBinaryWriter
{
    private const int SectionAlignment = BrlytSectionScope.DefaultAlignment;
    private const uint BinaryMagic = 0x524C5954; // RLYT
    private const ushort ByteOrderMark = 0xFEFF;
    private const ushort HeaderSize = 0x0010;
    private const ushort BrlytVersion = 0x0008;
    private const uint PaneCommonPayloadSize = 76u;
    private const uint PicturePayloadSize = 20u;
    private const uint TextBoxPayloadSize = 40u;
    private const uint WindowPayloadSize = 28u;
    private const uint WindowContentPayloadSize = 20u;

    private readonly BigEndianBinaryOutputWriter _writer;
    private readonly LayoutFlavor _flavor;
    private readonly bool _suppressCvtrCharConversion;
    private ushort _sectionCount;

    public BrlytBinaryWriter(
        Stream output,
        LayoutFlavor flavor = LayoutFlavor.Default,
        bool suppressCvtrCharConversion = false,
        bool leaveOpen = false)
    {
        _writer = new BigEndianBinaryOutputWriter(output, leaveOpen);
        _flavor = flavor;
        _suppressCvtrCharConversion = suppressCvtrCharConversion;
    }

    public void WriteDocument(BrlytDocumentContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        long fileSizePatchOffset = WriteFileHeader();

        WriteLyt1Section(context.Layout.screenSetting);
        WriteTxl1Section(context.Textures);
        WriteFnl1Section(context.Fonts);
        WriteMat1Section(context.Materials, context.Textures, context.Fonts);
        WritePaneHierarchy(context.RootPaneTree, context);
        WriteGroupHierarchy(context.RootGroup);

        _writer.Align(SectionAlignment);
        _writer.PatchUInt32(fileSizePatchOffset, checked((uint)_writer.Position));
        _writer.PatchUInt16(14, _sectionCount);
    }

    private void IncSectionCount()
    {
        _sectionCount++;
    }

    private long WriteFileHeader()
    {
        _writer.WriteUInt32(BinaryMagic);
        _writer.WriteUInt16(ByteOrderMark);
        _writer.WriteUInt16(BrlytVersion);
        long fileSizePatchOffset = _writer.ReserveUInt32();
        _writer.WriteUInt16(HeaderSize);
        _writer.WriteUInt16(0); // section count placeholder
        return fileSizePatchOffset;
    }

    private void WriteLyt1Section(ScreenSetting? screenSetting)
    {
        using var section = new BrlytSectionScope(_writer, "lyt1");

        _writer.WriteByte(MapScreenOriginMode(screenSetting?.origin));
        WriteZeroBytes(3);
        _writer.WriteSingle(screenSetting?.layoutSize?.x ?? 0f);
        _writer.WriteSingle(screenSetting?.layoutSize?.y ?? 0f);
        IncSectionCount();
    }

    private void WriteTxl1Section(IReadOnlyList<TextureFile> textures)
    {
        if (textures.Count == 0)
        {
            return;
        }

        using var section = new BrlytSectionScope(_writer, "txl1");
        WriteStringTable(textures.Select(static texture => texture.GetConvertedFileName()), SectionAlignment);
        IncSectionCount();
    }

    private void WriteFnl1Section(IReadOnlyList<FontFile> fonts)
    {
        if (fonts.Count == 0)
        {
            return;
        }

        using var section = new BrlytSectionScope(_writer, "fnl1");
        WriteStringTable(fonts.Select(static font => font.GetFileName()), SectionAlignment);
        IncSectionCount();
    }

    private void WriteMat1Section(IReadOnlyList<BrlytMaterialEntry> materials, IReadOnlyList<TextureFile> textures, IReadOnlyList<FontFile> fonts)
    {
        if (materials.Count == 0)
        {
            return;
        }

        using var section = new BrlytSectionScope(_writer, "mat1");
        _writer.WriteUInt16((ushort)materials.Count);
        _writer.WriteUInt16(0);

        long[] entryOffsetPatches = new long[materials.Count];
        for (int i = 0; i < materials.Count; i++)
        {
            entryOffsetPatches[i] = _writer.ReserveUInt32();
        }

        for (int i = 0; i < materials.Count; i++)
        {
            _writer.PatchUInt32(entryOffsetPatches[i], section.ContentStartPositionRelativeToSection);
            WriteMaterialEntry(materials[i], textures, fonts);
            _writer.Align(SectionAlignment);
        }

        IncSectionCount();
    }

    private void WriteMaterialEntry(BrlytMaterialEntry material, IReadOnlyList<TextureFile> textures, IReadOnlyList<FontFile> fonts)
    {
        WriteMaterialHeader(material);
        WriteTexMapBlock(material, textures);
        WriteTexMatrixBlock(material);
        WriteTexCoordGenBlock(material);

        if (!material.HasRevoMaterial)
        {
            WriteLegacyMaterialPayload(material);
            return;
        }

        WriteRevoMaterialPayload(material);
    }

    private void WriteMaterialHeader(BrlytMaterialEntry material)
    {
        _writer.WriteFixedAscii(material.Name, 20, zeroTerminate: true);

        ColorS10_4[] tevColorRegisters = material.TevColorRegisters ?? Array.Empty<ColorS10_4>();
        for (int i = 0; i < 3; i++)
        {
            WriteColorS10_4(i < tevColorRegisters.Length ? tevColorRegisters[i] : new ColorS10_4());
        }

        Color4[] tevConstantRegisters = material.TevConstantRegisters ?? Array.Empty<Color4>();
        for (int i = 0; i < 4; i++)
        {
            WriteColor(i < tevConstantRegisters.Length ? tevConstantRegisters[i] : null);
        }

        _writer.WriteUInt32(BuildMaterialFlags(material));
    }

    private void WriteTexMapBlock(BrlytMaterialEntry material, IReadOnlyList<TextureFile> textures)
    {
        TexMap[] texMaps = material.TexMaps ?? Array.Empty<TexMap>();
        int count = Math.Min(texMaps.Length, 15);
        for (int i = 0; i < count; i++)
        {
            WriteTexMap(texMaps[i], textures);
        }
    }

    private void WriteTexMatrixBlock(BrlytMaterialEntry material)
    {
        TexMatrix[] texMatrices = material.TexMatrices ?? Array.Empty<TexMatrix>();
        int count = Math.Min(texMatrices.Length, 15);
        for (int i = 0; i < count; i++)
        {
            WriteTexMatrix(texMatrices[i]);
        }
    }

    private void WriteTexCoordGenBlock(BrlytMaterialEntry material)
    {
        TexCoordGen[] texCoordGens = material.TexCoordGens ?? Array.Empty<TexCoordGen>();
        int count = Math.Min(texCoordGens.Length, 15);
        for (int i = 0; i < count; i++)
        {
            WriteTexCoordGen(texCoordGens[i]);
        }
    }

    private void WriteLegacyMaterialPayload(BrlytMaterialEntry material)
    {
        int tevStageCount = GetEffectiveTevStageCount(material);
        MaterialTextureStage[] legacyStages = material.TextureStages ?? Array.Empty<MaterialTextureStage>();
        for (int i = 0; i < tevStageCount; i++)
        {
            WriteLegacyTextureStage(i < legacyStages.Length ? legacyStages[i] : new MaterialTextureStage());
        }

        int blendRatioCount = Math.Min(material.TexBlendRatios?.Length ?? 0, 3);
        TexBlendRatio[] blendRatios = material.TexBlendRatios ?? Array.Empty<TexBlendRatio>();
        for (int i = 0; i < blendRatioCount; i++)
        {
            WriteTexBlendRatio(blendRatios[i]);
        }

        int indirectCount = GetEffectiveIndirectStageCount(material);
        MaterialWarp[] indirectWarps = material.IndirectWarps ?? Array.Empty<MaterialWarp>();
        for (int i = 0; i < indirectCount; i++)
        {
            WriteMaterialWarp(i < indirectWarps.Length ? indirectWarps[i] : new MaterialWarp());
        }
    }




    private void WriteRevoMaterialPayload(BrlytMaterialEntry material)
    {
        if ((material.ChannelControls?.Length ?? 0) > 0)
        {
            WriteChannelControls(material.ChannelControls!);
        }

        if (material.MaterialColorRegister is not null)
        {
            WriteColor(material.MaterialColorRegister);
        }

        if ((material.SwapTables?.Length ?? 0) > 0)
        {
            WriteSwapTables(material.SwapTables!);
        }

        TexMatrix[] indirectMatrices = material.IndirectMatrices ?? Array.Empty<TexMatrix>();
        if (indirectMatrices.Length > 0)
        {
            for (int i = 0; i < indirectMatrices.Length; i++)
            {
                WriteTexMatrix(indirectMatrices[i]);
            }
        }

        WriteIndirectStageBlock(material);
        WriteTevStageBlock(material);
        WriteRevoTailBlocks(material.AlphaCompare, material.BlendMode);
    }

    private void WriteIndirectStageBlock(BrlytMaterialEntry material)
    {
        int declaredCount = GetEffectiveIndirectStageCount(material);
        Material_RevoIndirectStage[] stages = material.IndirectStages ?? Array.Empty<Material_RevoIndirectStage>();

        for (int i = 0; i < declaredCount; i++)
        {
            WriteIndirectStage(i < stages.Length ? stages[i] : new Material_RevoIndirectStage());
        }
    }

    private void WriteTevStageBlock(BrlytMaterialEntry material)
    {
        int declaredCount = GetEffectiveTevStageCount(material);
        Material_RevoTevStage[] stages = material.TevStages ?? Array.Empty<Material_RevoTevStage>();

        for (int i = 0; i < declaredCount; i++)
        {
            WriteTevStage(i < stages.Length ? stages[i] : new Material_RevoTevStage());
        }
    }






    private void WriteRevoTailBlocks(Material_RevoAlphaCompare? alphaCompare, Material_RevoBlendMode? blendMode)
    {
        if (alphaCompare is not null)
        {
            WriteAlphaCompare(alphaCompare);
        }

        if (blendMode is not null)
        {
            WriteBlendMode(blendMode);
        }
    }

    private void WriteOptionalAlphaCompare(Material_RevoAlphaCompare? alphaCompare)
    {
        // Keep the payload width deterministic in the Revo path: 4-byte record when present,
        // otherwise 4 zero bytes only if another Revo-only tail block will follow later.
        if (alphaCompare is null)
        {
            return;
        }

        WriteAlphaCompare(alphaCompare);
    }

    private void WriteOptionalBlendMode(Material_RevoBlendMode? blendMode)
    {
        if (blendMode is null)
        {
            return;
        }

        WriteBlendMode(blendMode);
    }

    private static int GetEffectiveTevStageCount(BrlytMaterialEntry material)
        => Math.Min(16, material.HasRevoMaterial
            ? (material.TevStageCount > 0 ? material.TevStageCount : (byte)(material.TevStages?.Length ?? 0))
            : (material.TextureStages?.Length ?? 0));

    private static int GetEffectiveIndirectStageCount(BrlytMaterialEntry material)
        => Math.Min(4, material.HasRevoMaterial
            ? (material.IndirectStageCount > 0 ? material.IndirectStageCount : (byte)(material.IndirectStages?.Length ?? 0))
            : (material.IndirectWarps?.Length ?? 0));

    private uint BuildMaterialFlags(BrlytMaterialEntry material)
    {
        int texMapCount = Math.Min(material.TexMaps?.Length ?? 0, 15);
        int texMatrixCount = Math.Min(material.TexMatrices?.Length ?? 0, 15);
        int texCoordGenCount = Math.Min(material.TexCoordGens?.Length ?? 0, 15);

        if (!material.HasRevoMaterial)
        {
            uint legacyFlags = 0;
            legacyFlags |= (uint)(texMapCount & 0x0F);
            legacyFlags |= (uint)((texMatrixCount & 0x0F) << 4);
            legacyFlags |= (uint)((texCoordGenCount & 0x0F) << 8);
            legacyFlags |= (uint)((GetEffectiveTevStageCount(material) & 0x07) << 12);
            // Keep legacy materials from asserting revo-only payload bits.
            // OldCode with detailSetting=false does not advertise swap/indirect revo blocks.
            return legacyFlags;
        }

        int indirectMatrixCount = Math.Min(material.IndirectMatrices?.Length ?? 0, 3);
        int indirectStageCount = GetEffectiveIndirectStageCount(material);
        int effectiveTevStages = Math.Min(GetEffectiveTevStageCount(material), 31);

        // Matches the original NW4R revo packing much more closely than the earlier
        // simplified layout:
        //   00-03 texMap count
        //   04-07 texMatrix count
        //   08-11 texCoordGen count
        //   12    swapTable present
        //   13-14 indirectMatrix count
        //   15-17 indirectStage count
        //   18-22 tevStage count
        //   23    alphaCompare present
        //   24    blendMode present
        //   25    channelControl present
        //   27    matColReg present
        uint revoFlags = 0;
        revoFlags |= (uint)(texMapCount & 0x0F);
        revoFlags |= (uint)((texMatrixCount & 0x0F) << 4);
        revoFlags |= (uint)((texCoordGenCount & 0x0F) << 8);
        revoFlags |= (uint)(((material.SwapTables?.Length ?? 0) > 0 ? 1u : 0u) << 12);
        revoFlags |= (uint)(((indirectMatrixCount & 0x03) | ((indirectStageCount & 0x07) << 2)) << 13);
        revoFlags |= (uint)((effectiveTevStages & 0x1F) << 18);
        revoFlags |= (uint)((material.AlphaCompare is not null ? 1u : 0u) << 23);
        revoFlags |= (uint)((material.BlendMode is not null ? 1u : 0u) << 24);
        revoFlags |= (uint)(((material.ChannelControls?.Length ?? 0) > 0 ? 1u : 0u) << 25);
        revoFlags |= (uint)((material.MaterialColorRegister is not null ? 1u : 0u) << 27);
        return revoFlags;
    }

    private void WritePaneHierarchy(PaneTree? rootPaneTree, BrlytDocumentContext context)
    {
        if (rootPaneTree is null)
        {
            return;
        }

        Pane? rootPane = ResolvePane(rootPaneTree, context);
        if (rootPane is null)
        {
            WriteVirtualRootPaneNode(rootPaneTree, context);
            return;
        }

        WritePaneTreeNode(rootPaneTree, context);
    }

    private void WriteVirtualRootPaneNode(PaneTree tree, BrlytDocumentContext context)
    {
        using (new BrlytSectionScope(_writer, "pan1"))
        {
            WriteVirtualRootPaneCommon(tree.name, context.Layout.screenSetting);
            IncSectionCount();
        }

        PaneTree[] children = tree.paneTree ?? Array.Empty<PaneTree>();
        if (children.Length == 0)
        {
            return;
        }

        using (new BrlytSectionScope(_writer, "pas1"))
        {
            IncSectionCount();
        }

        foreach (var child in children)
        {
            WritePaneTreeNode(child, context);
        }

        using (new BrlytSectionScope(_writer, "pae1"))
        {
            IncSectionCount();
        }
    }

    private void WritePaneTreeNode(PaneTree tree, BrlytDocumentContext context)
    {
        Pane? pane = ResolvePane(tree, context);
        if (pane is null)
        {
            throw new InvalidDataException($"PaneTree node '{tree.name ?? string.Empty}' could not be resolved to a pane.");
        }

        WritePaneNodeSection(pane, context);
        WritePaneUserDataIfPresent(pane.userData);

        PaneTree[] children = tree.paneTree ?? Array.Empty<PaneTree>();
        if (children.Length == 0)
        {
            return;
        }

        using (new BrlytSectionScope(_writer, "pas1"))
        {
            IncSectionCount();
        }

        foreach (var child in children)
        {
            WritePaneTreeNode(child, context);
        }

        using (new BrlytSectionScope(_writer, "pae1"))
        {
            IncSectionCount();
        }
    }

    private void WritePaneNodeSection(Pane pane, BrlytDocumentContext context)
    {
        string magic = pane.kind switch
        {
            PaneKind.Picture => "pic1",
            PaneKind.TextBox => "txt1",
            PaneKind.Window => "wnd1",
            PaneKind.Bounding => "bnd1",
            _ => "pan1",
        };

        using var section = new BrlytSectionScope(_writer, magic);
        WritePaneCommon(pane);

        switch (pane.Item)
        {
            case Picture picture:
                WritePicturePane(picture, context);
                break;
            case TextBox textBox:
                WriteTextBoxPane(textBox, context);
                break;
            case Window window:
                WriteWindowPane(window, context);
                break;
            case Bounding bounding:
                WriteBoundingPane(bounding);
                break;
        }

        IncSectionCount();
    }

    private void WritePaneCommon(Pane pane)
    {
        // Mirrors the fixed 76-byte pane payload populated by bv/<Module>.a(ic*, Pane):
        // flags(1), basePosition(1), alpha(1), reserved(1), name[16], reservedZero[8],
        // translate(12), rotate(12), scale(8), size(8). The original writer zero-fills
        // the 8-byte slot after the pane name; it is not a live preview of userData.
        _writer.WriteByte(BuildPaneFlags(pane));
        _writer.WriteByte(MapPaneBasePosition(pane.basePositionType));
        _writer.WriteByte(pane.alpha);
        _writer.WriteByte(0);
        _writer.WriteFixedAscii(pane.name ?? string.Empty, 16, zeroTerminate: true);
        WriteZeroBytes(8);
        _writer.WriteSingle(pane.translate?.x ?? 0f);
        _writer.WriteSingle(pane.translate?.y ?? 0f);
        _writer.WriteSingle(pane.translate?.z ?? 0f);
        _writer.WriteSingle(pane.rotate?.x ?? 0f);
        _writer.WriteSingle(pane.rotate?.y ?? 0f);
        _writer.WriteSingle(pane.rotate?.z ?? 0f);
        _writer.WriteSingle(pane.scale?.x ?? 1f);
        _writer.WriteSingle(pane.scale?.y ?? 1f);
        _writer.WriteSingle(pane.size?.x ?? 0f);
        _writer.WriteSingle(pane.size?.y ?? 0f);
    }


    private void WriteVirtualRootPaneCommon(string? name, ScreenSetting? screenSetting)
    {
        _writer.WriteByte(0x01);
        _writer.WriteByte(MapVirtualRootPaneBasePosition(screenSetting?.origin));
        _writer.WriteByte(0xFF);
        _writer.WriteByte(0);
        _writer.WriteFixedAscii(name ?? "RootPane", 16, zeroTerminate: true);
        WriteZeroBytes(8);
        _writer.WriteSingle(0f);
        _writer.WriteSingle(0f);
        _writer.WriteSingle(0f);
        _writer.WriteSingle(0f);
        _writer.WriteSingle(0f);
        _writer.WriteSingle(0f);
        _writer.WriteSingle(1f);
        _writer.WriteSingle(1f);
        _writer.WriteSingle(screenSetting?.layoutSize?.x ?? 0f);
        _writer.WriteSingle(screenSetting?.layoutSize?.y ?? 0f);
    }

    private void WritePicturePane(Picture picture, BrlytDocumentContext context)
    {
        // Original writer (bv.a(Picture, Pane, ...)) uses a 20-byte fixed payload after the pane-common block.
        TexCoord[] texCoords = picture.texCoord ?? Array.Empty<TexCoord>();
        int texCoordCount = GetEffectiveTexCoordCount(texCoords);

        WriteColor(picture.vtxColLT);
        WriteColor(picture.vtxColRT);
        WriteColor(picture.vtxColLB);
        WriteColor(picture.vtxColRB);
        _writer.WriteUInt16((ushort)ResolveMaterialIndex(context, picture.material, picture.materialRevo));
        _writer.WriteByte((byte)texCoordCount);
        _writer.WriteByte(0);

        for (int i = 0; i < texCoordCount; i++)
        {
            WriteTexCoord(texCoords[i]);
        }
    }

    private void WriteTextBoxPane(TextBox textBox, BrlytDocumentContext context)
    {
        // Mirrors db.a(TextBox, Pane) + bv.a(TextBox, Pane, ...):
        // - fixed textbox payload = 40 bytes after the 76-byte pane-common block
        // - text bytes are only emitted when the textbox actually has content
        // - allocateStringLength affects stored size metadata, but not forced payload emission for empty strings
        // - text payload is aligned to 4 bytes in the original writer
        if (_flavor == LayoutFlavor.Banner && textBox.textAlignment != TextAlignment.Synchronous)
        {
            throw new InvalidOperationException($"Banner BRLYT only supports synchronous text alignment for textbox '{textBox.font ?? "<unnamed>"}'.");
        }

        string rawText = NormalizeTextForExport(textBox.text ?? string.Empty);
        bool hasText = rawText.Length > 0;
        byte[] textBytes = hasText
            ? Encoding.BigEndianUnicode.GetBytes(rawText + '\0')
            : Array.Empty<byte>();

        ushort allocatedBytes = textBox.allocateStringLengthSpecified
            ? (ushort)Math.Min(65534, (int)((textBox.allocateStringLength == 0 ? 0u : (textBox.allocateStringLength + 1u) * 2u)))
            : (ushort)0;
        ushort storedBytes = textBox.allocateStringLengthSpecified ? allocatedBytes : (ushort)textBytes.Length;
        ushort writtenBytes = hasText
            ? (ushort)Math.Min(textBytes.Length, allocatedBytes > 0 ? allocatedBytes : textBytes.Length)
            : (ushort)0;

        if (writtenBytes > 0 && writtenBytes < textBytes.Length)
        {
            // Match the original converter: when the allocated buffer is smaller than the
            // encoded string, the last UTF-16 code unit is forcibly terminated with 0x0000.
            byte[] truncated = textBytes[..writtenBytes];
            truncated[^2] = 0;
            truncated[^1] = 0;
            textBytes = truncated;
        }

        const uint textOffset = PaneCommonPayloadSize + TextBoxPayloadSize;

        _writer.WriteUInt16((ushort)ResolveMaterialIndex(context, textBox.material, textBox.materialRevo));
        _writer.WriteUInt16((ushort)ResolveFontIndex(textBox.font, context.Fonts));
        _writer.WriteUInt16(writtenBytes);
        _writer.WriteUInt16(storedBytes);
        _writer.WriteByte((byte)MapTextBoxPosition(textBox.positionType));
        _writer.WriteByte((byte)MapTextAlignment(textBox.textAlignment));
        _writer.WriteUInt16(0);
        _writer.WriteUInt32(textOffset);
        WriteColor(textBox.topColor);
        WriteColor(textBox.bottomColor);
        _writer.WriteSingle(textBox.fontSize?.x ?? 0f);
        _writer.WriteSingle(textBox.fontSize?.y ?? 0f);
        _writer.WriteSingle(textBox.charSpace);
        _writer.WriteSingle(textBox.lineSpace);

        WriteTextPayload(textBytes, writtenBytes, storedBytes);
    }

    private void WriteWindowPane(Window window, BrlytDocumentContext context)
    {
        // Mirrors the dedicated RLYT -> BRLYT window path from db.a(Window, Pane) + bv.a(Window, Pane, ...):
        // - fixed window payload = 28 bytes after the 76-byte pane-common block
        // - header layout is: contentInflation(16), frameCount(1), reserved(3), contentOffset(4), frameTableOffset(4)
        // - WindowContent fixed payload = 20 bytes
        // - WindowFrame record = 4 bytes
        // - frame count must be 1, 4 or 8
        // - frames are reordered by WindowFrameType slot before serialization
        TexCoord[] contentTexCoords = window.content?.texCoord ?? Array.Empty<TexCoord>();
        int texCoordCount = GetEffectiveTexCoordCount(contentTexCoords);
        WindowFrame[] orderedFrames = OrderWindowFramesForSerialization(window.frame ?? Array.Empty<WindowFrame>());
        int frameCount = orderedFrames.Length;

        bool hasContent = window.content is not null;
        uint payloadBaseOffset = PaneCommonPayloadSize + WindowPayloadSize;
        uint contentOffset = hasContent ? payloadBaseOffset : 0u;
        uint frameOffsetTableOffset = frameCount > 0
            ? payloadBaseOffset + (hasContent ? WindowContentPayloadSize + (uint)(texCoordCount * 32) : 0u)
            : 0u;
        uint frameRecordOffset = frameOffsetTableOffset + (uint)(frameCount * 4);

        _writer.WriteSingle(window.contentInflation?.l ?? 0f);
        _writer.WriteSingle(window.contentInflation?.r ?? 0f);
        _writer.WriteSingle(window.contentInflation?.t ?? 0f);
        _writer.WriteSingle(window.contentInflation?.b ?? 0f);
        _writer.WriteByte((byte)frameCount);
        _writer.WriteByte(0);
        _writer.WriteByte(0);
        _writer.WriteByte(0);
        _writer.WriteUInt32(contentOffset);
        _writer.WriteUInt32(frameOffsetTableOffset);

        if (window.content is { } content)
        {
            WriteWindowContent(content, (ushort)ResolveMaterialIndex(context, content.material, content.materialRevo), texCoordCount);
            for (int i = 0; i < texCoordCount; i++)
            {
                WriteTexCoord(contentTexCoords[i]);
            }
        }

        uint currentFrameRecordOffset = frameRecordOffset;
        for (int i = 0; i < frameCount; i++)
        {
            _writer.WriteUInt32(currentFrameRecordOffset);
            currentFrameRecordOffset += 4u;
        }

        for (int i = 0; i < frameCount; i++)
        {
            WindowFrame frame = orderedFrames[i];
            _writer.WriteUInt16((ushort)ResolveMaterialIndex(context, frame.material, frame.materialRevo));
            _writer.WriteUInt16(MapTextureFlip(frame.textureFlip));
        }
    }

    private void WriteWindowContent(WindowContent content, ushort materialIndex, int texCoordCount)
    {
        WriteColor(content.vtxColLT);
        WriteColor(content.vtxColRT);
        WriteColor(content.vtxColLB);
        WriteColor(content.vtxColRB);
        _writer.WriteUInt16(materialIndex);
        _writer.WriteByte((byte)texCoordCount);
        _writer.WriteByte(0);
    }

    private void WriteTexCoord(TexCoord coord)
    {
        _writer.WriteSingle(coord.texLT?.s ?? 0f);
        _writer.WriteSingle(coord.texLT?.t ?? 0f);
        _writer.WriteSingle(coord.texRT?.s ?? 0f);
        _writer.WriteSingle(coord.texRT?.t ?? 0f);
        _writer.WriteSingle(coord.texLB?.s ?? 0f);
        _writer.WriteSingle(coord.texLB?.t ?? 0f);
        _writer.WriteSingle(coord.texRB?.s ?? 0f);
        _writer.WriteSingle(coord.texRB?.t ?? 0f);
    }


    private static int GetEffectiveTexCoordCount(TexCoord[] texCoords)
        // GX/NW4R paths typically serialize at most 8 texture coordinate sets per pane/content block.
        => Math.Min(8, texCoords.Length);


    private static WindowFrame[] OrderWindowFramesForSerialization(WindowFrame[] frames)
    {
        int frameCount = frames.Length;
        if (frameCount == 0)
        {
            return Array.Empty<WindowFrame>();
        }

        if (frameCount is not (1 or 4 or 8))
        {
            throw new InvalidOperationException($"window - invalid frame num {frameCount}");
        }

        WindowFrame[] orderedFrames = new WindowFrame[frameCount];
        foreach (WindowFrame frame in frames)
        {
            int slot = MapWindowFrameType(frame.frameType);
            if (slot >= frameCount || orderedFrames[slot] is not null)
            {
                throw new InvalidOperationException("window - invalid frame type");
            }

            orderedFrames[slot] = frame;
        }

        for (int i = 0; i < orderedFrames.Length; i++)
        {
            if (orderedFrames[i] is null)
            {
                throw new InvalidOperationException("window - invalid frame type");
            }
        }

        return orderedFrames;
    }

    private static int MapWindowFrameType(WindowFrameType frameType)
        => frameType switch
        {
            WindowFrameType.CornerLT => 0,
            WindowFrameType.CornerRT => 1,
            WindowFrameType.CornerLB => 2,
            WindowFrameType.CornerRB => 3,
            WindowFrameType.FrameL => 4,
            WindowFrameType.FrameR => 5,
            WindowFrameType.FrameT => 6,
            WindowFrameType.FrameB => 7,
            _ => throw new InvalidOperationException($"invalid frame type. - {frameType}"),
        };

    private void WriteBoundingPane(Bounding bounding)
    {
    }

    private void WritePaneUserDataIfPresent(object[]? userData)
    {
        if (userData is not { Length: >= 2 })
        {
            return;
        }

        if (_flavor == LayoutFlavor.Banner)
        {
            throw new InvalidOperationException("Banner BRLYT does not support extended userData.");
        }

        UserDataPreparedEntry[] entries = PrepareUserDataEntries(userData);
        if (entries.Length == 0)
        {
            return;
        }

        using var numericStream = new MemoryStream();
        using var numericWriter = new BigEndianBinaryOutputWriter(numericStream, leaveOpen: true);
        using var stringStream = new MemoryStream();

        foreach (UserDataPreparedEntry entry in entries)
        {
            switch (entry.ValueType)
            {
                case PaneUserDataValueType.String:
                    entry.ValueOffset = checked((int)stringStream.Position);
                    WriteCStringToStream(stringStream, entry.StringValue ?? string.Empty);
                    break;

                case PaneUserDataValueType.Int32List:
                    entry.ValueOffset = checked((int)numericStream.Position);
                    foreach (int value in entry.IntValues ?? Array.Empty<int>())
                    {
                        numericWriter.WriteInt32(value);
                    }
                    break;

                case PaneUserDataValueType.Float32List:
                    entry.ValueOffset = checked((int)numericStream.Position);
                    foreach (float value in entry.FloatValues ?? Array.Empty<float>())
                    {
                        numericWriter.WriteSingle(value);
                    }
                    break;
            }

            if (!string.IsNullOrEmpty(entry.Name))
            {
                entry.NameOffset = checked((int)stringStream.Position);
                WriteCStringToStream(stringStream, entry.Name);
            }
        }

        AlignStream(stringStream, SectionAlignment);

        int descriptorSize = entries.Length * 12;
        int numericSize = checked((int)numericStream.Length);

        using var section = new BrlytSectionScope(_writer, "usd1");
        _writer.WriteUInt16(ToUInt16Count(entries.Length, "usd1 entry count"));
        _writer.WriteUInt16(0);

        for (int i = 0; i < entries.Length; i++)
        {
            UserDataPreparedEntry entry = entries[i];
            int descriptorRelativeBase = descriptorSize - (i * 12);
            uint nameOffset = entry.NameOffset >= 0
                ? (uint)(descriptorRelativeBase + numericSize + entry.NameOffset)
                : 0u;
            uint valueOffset = entry.ValueType == PaneUserDataValueType.String
                ? (uint)(descriptorRelativeBase + numericSize + entry.ValueOffset)
                : (uint)(descriptorRelativeBase + entry.ValueOffset);

            _writer.WriteUInt32(nameOffset);
            _writer.WriteUInt32(valueOffset);
            _writer.WriteUInt16((ushort)entry.ElementCount);
            _writer.WriteByte((byte)entry.ValueType);
            _writer.WriteByte(0);
        }

        _writer.Write(numericStream.ToArray());
        _writer.Write(stringStream.ToArray());

        // usd1 is intentionally excluded from the BRLYT file header section count.
    }

    private void WriteGroupHierarchy(Group? rootGroup)
    {
        if (rootGroup is null)
        {
            return;
        }

        WriteGroupTreeNode(rootGroup);
    }

    private void WriteGroupTreeNode(Group group)
    {
        WriteGroupSection(group);

        var children = group.group ?? Array.Empty<Group>();
        if (children.Length == 0)
        {
            return;
        }

        using (new BrlytSectionScope(_writer, "grs1"))
        {
            IncSectionCount();
        }

        foreach (var child in children)
        {
            WriteGroupTreeNode(child);
        }

        using (new BrlytSectionScope(_writer, "gre1"))
        {
            IncSectionCount();
        }
    }

    private void WriteGroupSection(Group group)
    {
        using var section = new BrlytSectionScope(_writer, "grp1");
        _writer.WriteFixedAscii(group.name ?? string.Empty, 16, zeroTerminate: true);
        _writer.WriteUInt16(ToUInt16Count(group.paneRef?.Length ?? 0, "grp1 paneRef count"));
        _writer.WriteUInt16(0);

        foreach (var paneRef in group.paneRef ?? Array.Empty<GroupPaneRef>())
        {
            _writer.WriteFixedAscii(paneRef.name ?? string.Empty, 16, zeroTerminate: true);
        }

        IncSectionCount();
    }

    private void WriteStringTable(IEnumerable<string> values, int alignment)
    {
        string[] entries = values.ToArray();
        _writer.WriteUInt16((ushort)entries.Length);
        _writer.WriteUInt16(0);

        long offsetsStart = _writer.Position;
        long[] offsetPatches = new long[entries.Length];
        for (int i = 0; i < entries.Length; i++)
        {
            offsetPatches[i] = _writer.ReserveUInt32();
            _writer.WriteUInt32(0);
        }

        for (int i = 0; i < entries.Length; i++)
        {
            uint relativeOffset = checked((uint)(_writer.Position - offsetsStart));
            _writer.PatchUInt32(offsetPatches[i], relativeOffset);
            WriteCString(entries[i]);
        }

        if (alignment > 1)
        {
            _writer.Align(alignment);
        }
    }

    private void WriteTexMap(TexMap texMap, IReadOnlyList<TextureFile> textures)
    {
        ushort textureIndex = (ushort)ResolveTextureIndex(texMap.imageName, textures);
        byte packed2 = 0;
        byte packed3 = 0;

        packed2 |= (byte)((int)texMap.wrap_s & 0x03);
        packed3 |= (byte)((int)texMap.wrap_t & 0x03);
        packed2 |= (byte)((MapMinFilter(texMap.minFilter) & 0x07) << 2);
        packed3 |= (byte)((MapMagFilter(texMap.magFilter) & 0x01) << 2);

        _writer.WriteUInt16(textureIndex);
        _writer.WriteByte(packed2);
        _writer.WriteByte(packed3);
    }

    private void WriteTexMatrix(TexMatrix texMatrix)
    {
        _writer.WriteSingle(texMatrix.translate?.x ?? 0f);
        _writer.WriteSingle(texMatrix.translate?.y ?? 0f);
        _writer.WriteSingle(texMatrix.rotate);
        _writer.WriteSingle(texMatrix.scale?.x ?? 1f);
        _writer.WriteSingle(texMatrix.scale?.y ?? 1f);
    }

    private void WriteTexCoordGen(TexCoordGen generator)
    {
        _writer.WriteByte(1);
        _writer.WriteByte(MapTexGenSource(generator.srcParam));
        _writer.WriteByte(MapTexGenMatrix(generator.matrix));
        _writer.WriteByte(0);
    }

    private void WriteColorS10_4(ColorS10_4 color)
    {
        _writer.WriteInt16(color.r);
        _writer.WriteInt16(color.g);
        _writer.WriteInt16(color.b);
        _writer.WriteInt16(color.a);
    }

    private void WriteBlackColor(BlackColor? color)
    {
        _writer.WriteByte(color?.r ?? 0);
        _writer.WriteByte(color?.g ?? 0);
        _writer.WriteByte(color?.b ?? 0);
        _writer.WriteByte(color?.a ?? 0);
    }

    private void WriteWhiteColor(WhiteColor? color)
    {
        _writer.WriteByte(color?.r ?? byte.MaxValue);
        _writer.WriteByte(color?.g ?? byte.MaxValue);
        _writer.WriteByte(color?.b ?? byte.MaxValue);
        _writer.WriteByte(color?.a ?? byte.MaxValue);
    }

    private void WriteTextPayload(byte[] textBytes, ushort writtenBytes, ushort storedBytes)
    {
        if (writtenBytes > 0)
        {
            _writer.Write(textBytes.AsSpan(0, writtenBytes));
        }

        // The txt1 header stores both the actual written byte length and the optional
        // allocated length, but the original writer does not emit zero-fill up to the
        // allocated capacity here. It only writes the actual text payload, then aligns.
        _writer.Align(SectionAlignment);
    }

    private void WriteLegacyTextureStage(MaterialTextureStage stage)
    {
        _writer.WriteByte(unchecked((byte)stage.texMap));
        _writer.WriteByte(unchecked((byte)stage.texCoordGen));
        _writer.WriteByte(unchecked((byte)stage.indirectStage));
        _writer.WriteByte(0);
    }

    private void WriteTexBlendRatio(TexBlendRatio ratio)
    {
        _writer.WriteByte(ratio.color);
        _writer.WriteByte(0);
        _writer.WriteByte(0);
        _writer.WriteByte(0);
    }

    private void WriteMaterialWarp(MaterialWarp warp)
    {
        _writer.WriteByte(warp.texMap);
        _writer.WriteByte(warp.texCoordGen);
        _writer.WriteByte((byte)warp.scale_s);
        _writer.WriteByte((byte)warp.scale_t);
        _writer.WriteByte(warp.signedOffsets ? (byte)1 : (byte)0);
        _writer.WriteByte(warp.replaceMode ? (byte)1 : (byte)0);
        _writer.WriteByte(warp.matrix);
        _writer.WriteByte(0);
    }
    private void WriteChannelControls(Material_RevoChannelControl[] channels)
    {
        byte color0Source = 1;
        byte alpha0Source = 1;

        foreach (Material_RevoChannelControl channel in channels)
        {
            byte source = channel.materialSource == ColorSource.Register ? (byte)0 : (byte)1;
            switch (channel.channel)
            {
                case ChannelID.Color0:
                    color0Source = source;
                    break;
                case ChannelID.Alpha0:
                    alpha0Source = source;
                    break;
            }
        }

        _writer.WriteByte(color0Source);
        _writer.WriteByte(alpha0Source);
        _writer.WriteUInt16(0);
    }

    private void WriteSwapTables(Material_RevoSwapTable[] swapTables)
    {
        Span<byte> packed = stackalloc byte[] { 0xE4, 0xC0, 0xD5, 0xEA };

        int count = Math.Min(swapTables.Length, packed.Length);
        for (int i = 0; i < count; i++)
        {
            Material_RevoSwapTable table = swapTables[i];
            packed[i] = (byte)(
                (MapTevColorChannel(table.r) & 0x03) |
                ((MapTevColorChannel(table.g) & 0x03) << 2) |
                ((MapTevColorChannel(table.b) & 0x03) << 4) |
                ((MapTevColorChannel(table.a) & 0x03) << 6));
        }

        _writer.Write(packed);
    }

    private void WriteIndirectStage(Material_RevoIndirectStage stage)
    {
        _writer.WriteByte(stage.texCoordGen);
        _writer.WriteByte(stage.texMap);
        _writer.WriteByte(MapIndirectScale(stage.scale_s));
        _writer.WriteByte(MapIndirectScale(stage.scale_t));
    }

    private void WriteTevStage(Material_RevoTevStage stage)
    {
        // The original bv.a(Material_RevoTevStage) packs a TEV stage into a compact fixed 16-byte record
        // instead of writing the XML sub-objects verbatim. We mirror that compact layout here.
        Span<byte> buffer = stackalloc byte[16];

        byte texMap = stage.texMap < 0 ? (byte)0xFF : unchecked((byte)stage.texMap);
        byte texCoord = stage.texCoordGen < 0 ? (byte)0xFF : unchecked((byte)stage.texCoordGen);

        buffer[0] = texCoord;
        buffer[1] = MapTevChannel(stage.colorChannel);
        buffer[2] = texMap;
        buffer[3] = (byte)(((stage.texColSwap & 0x03) << 3) | ((stage.rasColSwap & 0x03) << 1) | (texMap >> 8));

        WriteTevStageColor(stage.color, buffer);
        WriteTevStageAlpha(stage.alpha, buffer);
        WriteTevStageIndirect(stage.indirect, buffer);

        _writer.Write(buffer);
    }

    private static void WriteTevStageColor(Material_RevoTevStageColor? color, Span<byte> buffer)
    {
        if (color is null)
        {
            buffer[4] = 0x0F;
            buffer[5] = 0x0F;
            buffer[6] = 0x00;
            buffer[7] = 0x0C;
            return;
        }

        buffer[4] = (byte)((MapTevColorArg(color.b) << 4) | MapTevColorArg(color.a));
        buffer[5] = (byte)((MapTevColorArg(color.d) << 4) | MapTevColorArg(color.c));
        buffer[6] = (byte)((MapTevColorOp(color.op) & 0x0F) | ((MapTevBias(color.bias) & 0x03) << 4) | ((MapTevScale(color.scale) & 0x03) << 6));
        buffer[7] = (byte)((MapTevKonstColor(color.konst) << 3) | ((MapTevReg(color.outReg) & 0x03) << 1) | (color.clamp ? 1 : 0));
    }

    private static void WriteTevStageAlpha(Material_RevoTevStageAlpha? alpha, Span<byte> buffer)
    {
        // If there is no alpha stage, write zeros (the default placeholder) and exit.
        if (alpha is null)
        {
            buffer[8] = 0x00;
            buffer[9] = 0x00;
            buffer[10] = 0x00;
            buffer[11] = 0x00;
            return;
        }

        // Otherwise write the actual packed values.
        buffer[8] = (byte)((MapTevAlphaArg(alpha.b) << 4) | MapTevAlphaArg(alpha.a));
        buffer[9] = (byte)((MapTevAlphaArg(alpha.d) << 4) | MapTevAlphaArg(alpha.c));
        buffer[10] = (byte)((MapTevAlphaOp(alpha.op) & 0x0F) | ((MapTevBias(alpha.bias) & 0x03) << 4) | ((MapTevScale(alpha.scale) & 0x03) << 6));
        buffer[11] = (byte)((MapTevKonstAlpha(alpha.konst) << 3) | ((MapTevReg(alpha.outReg) & 0x03) << 1) | (alpha.clamp ? 1 : 0));
    }

    private static void WriteTevStageIndirect(Material_RevoTevStageIndirect? indirect, Span<byte> buffer)
    {
        if (indirect is null)
        {
            return;
        }

        buffer[12] = indirect.indStage;
        buffer[13] = (byte)((MapIndTexMatrix(indirect.matrix) << 3) | MapIndTexBias(indirect.bias));
        buffer[14] = (byte)((MapIndTexWrap(indirect.wrap_t) << 3) | MapIndTexWrap(indirect.wrap_s));
        buffer[15] = (byte)((MapIndTexAlpha(indirect.alpha) << 4) | (indirect.utcLod ? 8 : 0) | (indirect.addPrev ? 4 : 0) | MapIndTexFormat(indirect.format));
    }

    private void WriteAlphaCompare(Material_RevoAlphaCompare? alphaCompare)
    {
        if (alphaCompare is null)
        {
            _writer.WriteByte(0x77);
            _writer.WriteByte(0);
            _writer.WriteByte(0);
            _writer.WriteByte(0);
            return;
        }

        byte compare0 = MapCompare(alphaCompare.comp0);
        byte compare1 = MapCompare(alphaCompare.comp1);
        byte packedCompare = (byte)((compare1 << 4) | compare0);
        _writer.WriteByte(packedCompare);
        _writer.WriteByte(MapAlphaOp(alphaCompare.op));
        _writer.WriteByte(alphaCompare.ref0);
        _writer.WriteByte(alphaCompare.ref1);
    }

    private void WriteBlendMode(Material_RevoBlendMode? blendMode)
    {
        if (blendMode is null)
        {
            _writer.WriteByte(1);
            _writer.WriteByte(4);
            _writer.WriteByte(5);
            _writer.WriteByte(15);
            return;
        }

        _writer.WriteByte(MapBlendMode(blendMode.type));
        _writer.WriteByte(MapBlendFactorSrc(blendMode.srcFactor));
        _writer.WriteByte(MapBlendFactorDst(blendMode.dstFactor));
        _writer.WriteByte(MapLogicOp(blendMode.op));
    }

    private static byte MapMinFilter(TexFilter filter)
        => filter == TexFilter.Linear ? (byte)0 : (byte)Math.Max(0, ((int)filter - (int)TexFilter.Linear));

    private static byte MapMagFilter(TexFilter filter)
        => filter == TexFilter.Linear ? (byte)0 : (byte)Math.Max(0, ((int)filter - (int)TexFilter.Linear));

    private static byte MapTexGenSource(TexGenSrc source)
        => source switch
        {
            TexGenSrc.Tex0 => 4,
            TexGenSrc.Tex1 => 5,
            TexGenSrc.Tex2 => 6,
            TexGenSrc.Tex3 => 7,
            TexGenSrc.Tex4 => 8,
            TexGenSrc.Tex5 => 9,
            TexGenSrc.Tex6 => 10,
            TexGenSrc.Tex7 => 11,
            _ => 0,
        };

    private static byte MapTexGenMatrix(int matrix)
        => (sbyte)(matrix + 1) switch
        {
            0 => 60,
            1 => 30,
            2 => 33,
            3 => 36,
            4 => 39,
            5 => 42,
            6 => 45,
            7 => 48,
            8 => 51,
            _ => 60,
        };

    private static byte MapTevChannel(TevChannelID channel)
        => channel switch
        {
            TevChannelID.Color0a0 => 4,
            TevChannelID.ColorZero => 6,
            TevChannelID.ColorNull => 0xFF,
            _ => 4,
        };

    private static byte MapTevColorArg(TevColorArg arg)
        => arg switch
        {
            TevColorArg.CPrev => 0,
            TevColorArg.APrev => 1,
            TevColorArg.C0 => 2,
            TevColorArg.A0 => 3,
            TevColorArg.C1 => 4,
            TevColorArg.A1 => 5,
            TevColorArg.C2 => 6,
            TevColorArg.A2 => 7,
            TevColorArg.TexC => 8,
            TevColorArg.TexA => 9,
            TevColorArg.RasC => 10,
            TevColorArg.RasA => 11,
            TevColorArg.V1_0 => 12,
            TevColorArg.V0_5 => 13,
            TevColorArg.Konst => 14,
            TevColorArg.V0 => 15,
            _ => 2,
        };

    private static byte MapTevAlphaArg(TevAlphaArg arg)
        => arg switch
        {
            TevAlphaArg.APrev => 0,
            TevAlphaArg.A0 => 1,
            TevAlphaArg.A1 => 2,
            TevAlphaArg.A2 => 3,
            TevAlphaArg.TexA => 4,
            TevAlphaArg.RasA => 5,
            TevAlphaArg.Konst => 6,
            TevAlphaArg.V0 => 7,
            _ => 1,
        };

    private static byte MapTevColorOp(TevOpC op)
        => (byte)op < 2 ? (byte)op : (byte)((byte)op + 6);

    private static byte MapTevAlphaOp(TevOpA op) => (byte)op;

    private static byte MapTevBias(TevBias bias)
        => bias switch
        {
            TevBias.V0 => 0,
            TevBias.P0_5 => 1,
            TevBias.M0_5 => 2,
            _ => 0,
        };

    private static byte MapTevScale(TevScale scale)
        => scale switch
        {
            TevScale.V1 => 0,
            TevScale.V2 => 1,
            TevScale.V4 => 2,
            TevScale.V1_2 => 3,
            _ => 0,
        };

    private static byte MapTevReg(TevRegID reg)
        => reg switch
        {
            TevRegID.Prev => 0,
            TevRegID.Reg0 => 1,
            TevRegID.Reg1 => 2,
            TevRegID.Reg2 => 3,
            _ => 0,
        };

    private static byte MapTevKonstColor(TevKColorSel konst)
        => (byte)konst < 20 ? (byte)((byte)konst + 12) : (byte)((byte)konst - 20);

    private static byte MapTevKonstAlpha(TevKAlphaSel konst)
        => (byte)konst < 16 ? (byte)((byte)konst + 16) : (byte)((byte)konst - 16);

    private static byte MapIndTexFormat(IndTexFormat format) => (byte)format;
    private static byte MapIndTexBias(IndTexBiasSel bias)
        => bias switch
        {
            IndTexBiasSel.U => 4,
            IndTexBiasSel.ST => 3,
            _ => (byte)bias,
        };

    private static byte MapIndTexMatrix(IndTexMtxID matrix)
        => matrix switch
        {
            IndTexMtxID.S0 => 5,
            IndTexMtxID.S1 => 6,
            IndTexMtxID.S2 => 7,
            IndTexMtxID.T0 => 9,
            IndTexMtxID.T1 => 10,
            IndTexMtxID.T2 => 11,
            _ => (byte)matrix,
        };
    private static byte MapIndTexWrap(IndTexWrap wrap) => (byte)wrap;
    private static byte MapIndTexAlpha(IndTexAlphaSel alpha) => (byte)alpha;

    private static byte MapTevColorChannel(TevColorChannel channel)
        => channel switch
        {
            TevColorChannel.Red => 0,
            TevColorChannel.Green => 1,
            TevColorChannel.Blue => 2,
            TevColorChannel.Alpha => 3,
            _ => 0,
        };

    private static byte MapIndirectScale(IndTexScale scale)
        => scale switch
        {
            IndTexScale.V1 => 0,
            IndTexScale.V2 => 1,
            IndTexScale.V4 => 2,
            IndTexScale.V8 => 3,
            IndTexScale.V16 => 4,
            IndTexScale.V32 => 5,
            IndTexScale.V64 => 6,
            IndTexScale.V128 => 7,
            IndTexScale.V256 => 8,
            _ => 0,
        };

    private static byte MapCompare(Compare compare)
        => compare switch
        {
            Compare.Never => 0,
            Compare.Less => 1,
            Compare.Equal => 2,
            Compare.LEqual => 3,
            Compare.Greater => 4,
            Compare.NEqual => 5,
            Compare.GEqual => 6,
            Compare.Always => 7,
            _ => 0,
        };

    private static byte MapAlphaOp(AlphaOp op)
        => op switch
        {
            AlphaOp.And => 0,
            AlphaOp.Or => 1,
            AlphaOp.Xor => 2,
            AlphaOp.Xnor => 3,
            _ => 0,
        };

    private static byte MapBlendMode(BlendMode mode)
        => mode switch
        {
            BlendMode.None => 0,
            BlendMode.Blend => 1,
            BlendMode.Logic => 2,
            BlendMode.Subtract => 3,
            _ => 0,
        };

    private static byte MapBlendFactorSrc(BlendFactorSrc factor)
        => factor switch
        {
            BlendFactorSrc.V0 => 0,
            BlendFactorSrc.V1_0 => 1,
            BlendFactorSrc.DstClr => 2,
            BlendFactorSrc.InvDstClr => 3,
            BlendFactorSrc.SrcAlpha => 4,
            BlendFactorSrc.InvSrcAlpha => 5,
            BlendFactorSrc.DstAlpha => 6,
            BlendFactorSrc.InvDstAlpha => 7,
            _ => 0,
        };

    private static byte MapBlendFactorDst(BlendFactorDst factor)
        => factor switch
        {
            BlendFactorDst.V0 => 0,
            BlendFactorDst.V1_0 => 1,
            BlendFactorDst.SrcClr => 2,
            BlendFactorDst.InvSrcClr => 3,
            BlendFactorDst.SrcAlpha => 4,
            BlendFactorDst.InvSrcAlpha => 5,
            BlendFactorDst.DstAlpha => 6,
            BlendFactorDst.InvDstAlpha => 7,
            _ => 0,
        };

    private static byte MapLogicOp(LogicOp op)
        => op switch
        {
            LogicOp.Clear => 0,
            LogicOp.And => 1,
            LogicOp.RevAnd => 2,
            LogicOp.Copy => 3,
            LogicOp.InvAnd => 4,
            LogicOp.NoOp => 5,
            LogicOp.Xor => 6,
            LogicOp.Or => 7,
            LogicOp.Nor => 8,
            LogicOp.Equiv => 9,
            LogicOp.Inv => 10,
            LogicOp.RevOr => 11,
            LogicOp.InvCopy => 12,
            LogicOp.InvOr => 13,
            LogicOp.Nand => 14,
            LogicOp.Set => 15,
            _ => 15,
        };

    private Pane? ResolvePane(PaneTree tree, BrlytDocumentContext context)
        => string.IsNullOrWhiteSpace(tree.name) ? null : context.FindPane(tree.name!);


    private int ResolveMaterialIndex(BrlytDocumentContext context, Material? material, Material_Revo? materialRevo)
        => context.ResolveMaterialIndex(materialRevo) is var revoIndex && revoIndex != 0
            ? revoIndex
            : context.ResolveMaterialIndex(material);

    private void WriteRgb(byte r, byte g, byte b)
    {
        _writer.WriteByte(r);
        _writer.WriteByte(g);
        _writer.WriteByte(b);
        _writer.WriteByte(0);
    }

    private void WriteColor(Color4? color)
    {
        _writer.WriteByte(color?.r ?? byte.MaxValue);
        _writer.WriteByte(color?.g ?? byte.MaxValue);
        _writer.WriteByte(color?.b ?? byte.MaxValue);
        _writer.WriteByte(color?.a ?? byte.MaxValue);
    }

    private void WriteZeroBytes(int count)
    {
        for (int i = 0; i < count; i++)
        {
            _writer.WriteByte(0);
        }
    }

    private void WriteCString(string text)
    {
        _writer.Write(Encoding.ASCII.GetBytes(text));
        _writer.WriteByte(0);
    }

    private void WriteAlignedCString(string text, int alignment = SectionAlignment)
    {
        WriteCString(text);
        _writer.Align(alignment);
    }

    private byte BuildPaneFlags(Pane pane)
    {
        byte flags = 0;
        if (pane.visible) flags |= 0x01;
        if (pane.influencedAlpha) flags |= 0x02;
        if (pane.locationAdjust) flags |= 0x04;
        return flags;
    }

    private static byte MapPaneBasePosition(Position? position)
    {
        int horizontal = (position?.x ?? HorizontalPosition.Left) switch
        {
            HorizontalPosition.Left => 0,
            HorizontalPosition.Center => 1,
            HorizontalPosition.Right => 2,
            _ => 0,
        };

        int vertical = (position?.y ?? VerticalPosition.Top) switch
        {
            VerticalPosition.Top => 0,
            VerticalPosition.Center => 1,
            VerticalPosition.Bottom => 2,
            _ => 0,
        };

        return (byte)((vertical * 3) + horizontal);
    }

    private static byte MapScreenOriginMode(ScreenOriginType? origin)
        => origin == ScreenOriginType.Normal ? (byte)1 : (byte)0;

    private static byte MapVirtualRootPaneBasePosition(ScreenOriginType? origin)
        => origin == ScreenOriginType.Classic ? (byte)0 : (byte)4;

    private static string GetPaneUserDataPreview(object[]? userData)
    {
        if (userData is not { Length: > 0 })
        {
            return string.Empty;
        }

        if (userData[0] is UserDataString firstString && !string.IsNullOrEmpty(firstString.Value))
        {
            return firstString.Value;
        }

        return string.Empty;
    }

    private UserDataPreparedEntry[] PrepareUserDataEntries(object[] userData)
    {
        var seenNames = new Dictionary<string, string>(StringComparer.InvariantCulture);
        var entries = new List<UserDataPreparedEntry>(userData.Length);

        foreach (object item in userData)
        {
            switch (item)
            {
                case UserDataString text:
                    {
                        string value = text.Value ?? string.Empty;
                        if (value.Trim().Length == 0)
                        {
                            continue;
                        }

                        ValidateUniqueUserDataName(seenNames, text.name ?? string.Empty);
                        entries.Add(UserDataPreparedEntry.ForString(text.name ?? string.Empty, value));
                        break;
                    }

                case UserDataIntList ints:
                    {
                        string raw = ints.Value ?? string.Empty;
                        if (raw.Trim().Length == 0)
                        {
                            continue;
                        }

                        int[] values = SplitUserDataTokens(raw).Select(static token => int.Parse(token, CultureInfo.InvariantCulture)).ToArray();
                        ValidateUniqueUserDataName(seenNames, ints.name ?? string.Empty);
                        entries.Add(UserDataPreparedEntry.ForInts(ints.name ?? string.Empty, values));
                        break;
                    }

                case UserDataFloatList floats:
                    {
                        string raw = floats.Value ?? string.Empty;
                        if (raw.Trim().Length == 0)
                        {
                            continue;
                        }

                        float[] values = SplitUserDataTokens(raw).Select(static token => float.Parse(token, CultureInfo.InvariantCulture)).ToArray();
                        ValidateUniqueUserDataName(seenNames, floats.name ?? string.Empty);
                        entries.Add(UserDataPreparedEntry.ForFloats(floats.name ?? string.Empty, values));
                        break;
                    }
            }
        }

        return entries.ToArray();
    }

    private static void ValidateUniqueUserDataName(Dictionary<string, string> seenNames, string name)
    {
        try
        {
            seenNames.Add(name, name);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidDataException($"Duplicate userData name '{name}' is not allowed.", ex);
        }
    }

    private static IEnumerable<string> SplitUserDataTokens(string value)
        => value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static void WriteCStringToStream(Stream stream, string value)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(value + " ");
        stream.Write(bytes, 0, bytes.Length);
    }

    private static void AlignStream(Stream stream, uint alignment)
    {
        long remainder = stream.Position % alignment;
        if (remainder == 0)
        {
            return;
        }

        int padding = checked((int)(alignment - remainder));
        Span<byte> zeros = stackalloc byte[4];
        stream.Write(zeros[..padding]);
    }

    private static ushort MapTextureFlip(TextureFlip[]? textureFlip)
    {
        TextureFlip value = textureFlip is { Length: > 0 } ? textureFlip[0] : TextureFlip.None;
        return value switch
        {
            TextureFlip.FlipH => 1,
            TextureFlip.FlipV => 2,
            TextureFlip.Rotate90 => 3,
            TextureFlip.Rotate180 => 4,
            TextureFlip.Rotate270 => 5,
            _ => 0,
        };
    }

    private static int MapTextBoxPosition(Position? positionType)
    {
        int horizontal = (positionType?.x ?? HorizontalPosition.Left) switch
        {
            HorizontalPosition.Left => 0,
            HorizontalPosition.Center => 1,
            HorizontalPosition.Right => 2,
            _ => 0,
        };

        int vertical = (positionType?.y ?? VerticalPosition.Top) switch
        {
            VerticalPosition.Top => 0,
            VerticalPosition.Center => 1,
            VerticalPosition.Bottom => 2,
            _ => 0,
        };

        return (vertical * 3) + horizontal;
    }

    private static int MapTextAlignment(TextAlignment alignment)
        => alignment switch
        {
            TextAlignment.Synchronous => 0,
            TextAlignment.Left => 1,
            TextAlignment.Center => 2,
            TextAlignment.Right => 3,
            _ => 0,
        };

    private static IEnumerable<string> SplitCsv(string raw)
        => raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    private int ResolveTextureIndex(string? imageName, IReadOnlyList<TextureFile> textures)
        => textures.Select(static (texture, index) => (texture, index))
            .FirstOrDefault(tuple => string.Equals(tuple.texture.GetName(), imageName, StringComparison.InvariantCultureIgnoreCase)).index;

    private int ResolveFontIndex(string? fontName, IReadOnlyList<FontFile> fonts)
        => fonts.Select(static (font, index) => (font, index))
            .FirstOrDefault(tuple => string.Equals(tuple.font.GetName(), fontName, StringComparison.Ordinal)).index;

    private string NormalizeTextForExport(string text)
    {
        if (_suppressCvtrCharConversion || text.Length == 0)
        {
            return text;
        }

        return DecodeCvtrCharEscapes(text);
    }

    private static string DecodeCvtrCharEscapes(string text)
    {
        var builder = new StringBuilder(text.Length);
        int segmentStart = 0;
        int scanIndex = 0;

        while (scanIndex < text.Length)
        {
            int caretIndex = text.IndexOf('^', scanIndex);
            if (caretIndex < 0)
            {
                builder.Append(text.AsSpan(segmentStart));
                break;
            }

            if (caretIndex > segmentStart)
            {
                builder.Append(text.AsSpan(segmentStart, caretIndex - segmentStart));
            }

            if (caretIndex + 1 >= text.Length)
            {
                break;
            }

            char next = text[caretIndex + 1];
            if (next == '^')
            {
                builder.Append('^');
                scanIndex = caretIndex + 2;
                segmentStart = scanIndex;
                continue;
            }

            if (caretIndex + 2 < text.Length &&
                IsCvtrHexNibble(next) &&
                IsCvtrHexNibble(text[caretIndex + 2]))
            {
                builder.Append((char)Convert.ToInt32(text.Substring(caretIndex + 1, 2), 16));
                scanIndex = caretIndex + 3;
                segmentStart = scanIndex;
                continue;
            }

            scanIndex = caretIndex + 1;
            segmentStart = scanIndex;
        }

        return builder.ToString();
    }

    private static bool IsCvtrHexNibble(char value)
        => (value is >= '0' and <= '9') || (value is >= 'a' and <= 'f') || (value is >= 'A' and <= 'F');

    private static uint ParseVersion(string? xmlVersion)
    {
        if (string.IsNullOrWhiteSpace(xmlVersion))
        {
            return 0x00010000;
        }

        string[] parts = xmlVersion.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        uint major = parts.Length > 0 && uint.TryParse(parts[0], out uint parsedMajor) ? parsedMajor : 1;
        uint minor = parts.Length > 1 && uint.TryParse(parts[1], out uint parsedMinor) ? parsedMinor : 0;
        uint patch = parts.Length > 2 && uint.TryParse(parts[2], out uint parsedPatch) ? parsedPatch : 0;
        return (major << 24) | (minor << 16) | patch;
    }

    private sealed class UserDataPreparedEntry
    {
        private UserDataPreparedEntry(string name, PaneUserDataValueType valueType, int elementCount)
        {
            Name = name;
            ValueType = valueType;
            ElementCount = elementCount;
        }

        public string Name { get; }
        public PaneUserDataValueType ValueType { get; }
        public int ElementCount { get; }
        public string? StringValue { get; private init; }
        public int[]? IntValues { get; private init; }
        public float[]? FloatValues { get; private init; }
        public int NameOffset { get; set; } = -1;
        public int ValueOffset { get; set; }

        public static UserDataPreparedEntry ForString(string name, string value)
            => new(name, PaneUserDataValueType.String, value.Length)
            {
                StringValue = value,
            };

        public static UserDataPreparedEntry ForInts(string name, int[] values)
            => new(name, PaneUserDataValueType.Int32List, values.Length)
            {
                IntValues = values,
            };

        public static UserDataPreparedEntry ForFloats(string name, float[] values)
            => new(name, PaneUserDataValueType.Float32List, values.Length)
            {
                FloatValues = values,
            };
    }
    private static ushort ToUInt16Count(int count, string fieldName)
    {
        if ((uint)count > ushort.MaxValue)
        {
            throw new InvalidDataException($"{fieldName} exceeds UInt16 range: {count}.");
        }

        return (ushort)count;
    }

}
