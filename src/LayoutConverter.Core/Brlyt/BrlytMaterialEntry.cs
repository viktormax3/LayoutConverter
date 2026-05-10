using System;
using LayoutConverter.Core.Schema.Rlyt;

namespace LayoutConverter.Core.Brlyt;

public sealed class BrlytMaterialEntry
{
    public string Name { get; init; } = string.Empty;
    public bool HasRevoMaterial { get; init; }
    public Material_RevoChannelControl[]? ChannelControls { get; init; }
    public Color4? MaterialColorRegister { get; init; }
    public ColorS10_4[] TevColorRegisters { get; init; } = Array.Empty<ColorS10_4>();
    public Color4[]? TevConstantRegisters { get; init; }
    public TexMap[] TexMaps { get; init; } = Array.Empty<TexMap>();
    public TexMatrix[] TexMatrices { get; init; } = Array.Empty<TexMatrix>();
    public TexCoordGen[] TexCoordGens { get; init; } = Array.Empty<TexCoordGen>();
    public MaterialTextureStage[] TextureStages { get; init; } = Array.Empty<MaterialTextureStage>();
    public TexBlendRatio[] TexBlendRatios { get; init; } = Array.Empty<TexBlendRatio>();
    public MaterialWarp[] IndirectWarps { get; init; } = Array.Empty<MaterialWarp>();
    public Material_RevoSwapTable[]? SwapTables { get; init; }
    public TexMatrix[] IndirectMatrices { get; init; } = Array.Empty<TexMatrix>();
    public Material_RevoIndirectStage[] IndirectStages { get; init; } = Array.Empty<Material_RevoIndirectStage>();
    public Material_RevoTevStage[] TevStages { get; init; } = Array.Empty<Material_RevoTevStage>();
    public Material_RevoAlphaCompare? AlphaCompare { get; init; }
    public Material_RevoBlendMode? BlendMode { get; init; }
    public byte TevStageCount { get; init; }
    public byte IndirectStageCount { get; init; }
    public DisplayFace DisplayFace { get; init; } = DisplayFace.Both;

    public static BrlytMaterialEntry From(
        Material? material,
        Material_Revo? materialRevo,
        bool supportsTexturePayload,
        bool useRevoMaterial)
    {
        if (useRevoMaterial && HasEffectiveRevoPayload(materialRevo))
        {
            return FromRevo(materialRevo!, supportsTexturePayload);
        }

        return FromLegacy(material, supportsTexturePayload);
    }

    private static BrlytMaterialEntry FromLegacy(Material? material, bool supportsTexturePayload)
    {
        Material source = material ?? new Material();
        string name = source.name ?? string.Empty;

        ColorS10_4[] tevColorRegisters =
        [
            ToColorRegister(source.blackColor),
            ToColorRegister(source.whiteColor),
            new ColorS10_4(255, 255, 255, 255),
        ];

        TexBlendRatio[] texBlendRatios = Resize(source.texBlendRatio, 8);
        Color4[] tevConstantRegisters =
        [
            new Color4(255, 255, 255, 255),
            new Color4(255, 255, 255, 255),
            new Color4(255, 255, 255, 255),
            new Color4(255, 255, 255, 255),
        ];

        for (int i = 0; i < texBlendRatios.Length; i++)
        {
            int registerIndex = tevConstantRegisters.Length - 1 - (i / 4);
            int componentIndex = i % 4;
            byte ratioValue = texBlendRatios[i]?.color ?? 0;
            Color4 target = tevConstantRegisters[registerIndex];

            switch (componentIndex)
            {
                case 0:
                    target.a = ratioValue;
                    break;
                case 1:
                    target.b = ratioValue;
                    break;
                case 2:
                    target.g = ratioValue;
                    break;
                case 3:
                    target.r = ratioValue;
                    break;
            }

            tevConstantRegisters[registerIndex] = target;
        }

        return new BrlytMaterialEntry
        {
            Name = name,
            HasRevoMaterial = false,
            TevColorRegisters = tevColorRegisters,
            TevConstantRegisters = tevConstantRegisters,
            TexMaps = supportsTexturePayload ? CloneArray(source.texMap) : Array.Empty<TexMap>(),
            TexMatrices = supportsTexturePayload ? CloneArray(source.texMatrix) : Array.Empty<TexMatrix>(),
            TexCoordGens = supportsTexturePayload ? CloneArray(source.texCoordGen) : Array.Empty<TexCoordGen>(),
            // The compact material route does not serialize stage payload blocks.
            TextureStages = Array.Empty<MaterialTextureStage>(),
            TexBlendRatios = Array.Empty<TexBlendRatio>(),
            IndirectWarps = Array.Empty<MaterialWarp>(),
            TevStageCount = 0,
            IndirectStageCount = 0,
        };
    }

    private static BrlytMaterialEntry FromRevo(Material_Revo materialRevo, bool supportsTexturePayload)
    {
        Material_RevoIndirectStage[] indirectStages = Array.Empty<Material_RevoIndirectStage>();
        TexMatrix[] indirectMatrices = Array.Empty<TexMatrix>();
        Material_RevoTevStage[] tevStages = Array.Empty<Material_RevoTevStage>();

        if (supportsTexturePayload)
        {
            int indirectStageCount = Math.Min(materialRevo.indirectStageNum, (byte)4);
            indirectStages = indirectStageCount > 0
                ? Resize(materialRevo.indirectStage, indirectStageCount)
                : Array.Empty<Material_RevoIndirectStage>();
            indirectMatrices = indirectStages.Length > 0
                ? Resize(materialRevo.indirectMatrix, 3)
                : Array.Empty<TexMatrix>();

            int tevStageCount = Math.Min(materialRevo.tevStageNum, (byte)16);
            tevStages = tevStageCount > 0
                ? Resize(materialRevo.tevStage, tevStageCount)
                : Array.Empty<Material_RevoTevStage>();
        }

        return new BrlytMaterialEntry
        {
            Name = materialRevo.name ?? string.Empty,
            HasRevoMaterial = true,
            ChannelControls = supportsTexturePayload ? NormalizeChannelControls(materialRevo.channelControl) : null,
            TevColorRegisters = Resize(materialRevo.tevColReg, 3),
            TevConstantRegisters = NormalizeConstantRegisters(materialRevo.tevConstReg),
            TexMaps = supportsTexturePayload ? CloneArray(materialRevo.texMap) : Array.Empty<TexMap>(),
            TexMatrices = supportsTexturePayload ? CloneArray(materialRevo.texMatrix) : Array.Empty<TexMatrix>(),
            TexCoordGens = supportsTexturePayload ? CloneArray(materialRevo.texCoordGen) : Array.Empty<TexCoordGen>(),
            SwapTables = supportsTexturePayload ? Resize(materialRevo.swapTable, 4) : null,
            IndirectMatrices = indirectMatrices,
            IndirectStages = indirectStages,
            TevStages = tevStages,
            AlphaCompare = supportsTexturePayload ? NormalizeAlphaCompare(materialRevo.alphaCompare) : null,
            BlendMode = supportsTexturePayload ? NormalizeBlendMode(materialRevo.blendMode) : null,
            TevStageCount = (byte)tevStages.Length,
            IndirectStageCount = (byte)indirectStages.Length,
            DisplayFace = materialRevo.displayFace,
            MaterialColorRegister = materialRevo.matColReg,
        };
    }

    private static ColorS10_4 ToColorRegister(BlackColor? color)
        => color is null
            ? new ColorS10_4(0, 0, 0, 0)
            : new ColorS10_4(color.r, color.g, color.b, color.a);

    private static ColorS10_4 ToColorRegister(WhiteColor? color)
        => color is null
            ? new ColorS10_4(255, 255, 255, 255)
            : new ColorS10_4(color.r, color.g, color.b, color.a);

    private static ColorS10_4 ToColorRegister(Color4? color)
        => color is null
            ? new ColorS10_4()
            : new ColorS10_4(color.r, color.g, color.b, color.a);

    private static T[] CloneArray<T>(T[]? source)
        => source?.ToArray() ?? Array.Empty<T>();

    private static T[] Resize<T>(T[]? source, int maxCount)
    {
        if (source is null || source.Length == 0 || maxCount <= 0)
        {
            return Array.Empty<T>();
        }

        int count = Math.Min(source.Length, maxCount);
        T[] resized = new T[count];
        Array.Copy(source, resized, count);
        return resized;
    }

    private static Material_RevoSwapTable[]? NormalizeSwapTables(Material_RevoSwapTable[]? source)
    {
        Material_RevoSwapTable[] resized = Resize(source, 4);
        if (resized.Length == 0)
        {
            return null;
        }

        bool allDefault = true;
        for (int i = 0; i < resized.Length; i++)
        {
            Material_RevoSwapTable table = resized[i];
            if (table.r != TevColorChannel.Red
                || table.g != TevColorChannel.Green
                || table.b != TevColorChannel.Blue
                || table.a != TevColorChannel.Alpha)
            {
                allDefault = false;
                break;
            }
        }

        return allDefault ? null : resized;
    }

    private static Material_RevoAlphaCompare? NormalizeAlphaCompare(Material_RevoAlphaCompare? source)
    {
        return source;
    }

    private static Material_RevoBlendMode? NormalizeBlendMode(Material_RevoBlendMode? source)
    {
        return source;
    }

    private static Material_RevoChannelControl[]? NormalizeChannelControls(Material_RevoChannelControl[]? source)
    {
        return source is null ? null : Resize(source, 2);
    }

    private static Color4[]? NormalizeConstantRegisters(Color4[]? source)
    {
        return source is null ? null : Resize(source, 4);
    }

    private static Color4? NormalizeMatColReg(Color4? source)
    {
        return source;
    }
    private static bool HasEffectiveRevoPayload(Material_Revo? materialRevo)
    {
        return materialRevo is not null;
    }

    private static TexMatrix[] CreateIdentityIndirectMatrices()
        =>
        [
            new TexMatrix { scale = new Vec2(1f, 1f), translate = new Vec2(0f, 0f), rotate = 0f },
            new TexMatrix { scale = new Vec2(1f, 1f), translate = new Vec2(0f, 0f), rotate = 0f },
            new TexMatrix { scale = new Vec2(1f, 1f), translate = new Vec2(0f, 0f), rotate = 0f },
        ];
}
