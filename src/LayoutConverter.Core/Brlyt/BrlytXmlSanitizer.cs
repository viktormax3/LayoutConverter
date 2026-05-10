using LayoutConverter.Core.Schema.Rlyt;
using LayoutConverter.Core.Models;

namespace LayoutConverter.Core.Brlyt;

public static class BrlytXmlSanitizer
{
    public static void Sanitize(Document document)
    {
        if (document.body?.rlyt?.paneSet == null) return;

        foreach (var pane in document.body.rlyt.paneSet)
        {
            SanitizePane(pane);
        }
    }

    private static void SanitizePane(Pane pane)
    {
        pane.comment ??= string.Empty;
        if (pane.userData == null || pane.userData.Length == 0)
        {
            pane.userData = new object[] { new UserDataString { name = "__BasicUserDataString", Value = string.Empty } };
        }

        switch (pane.Item)
        {
            case Picture pic:
                pic.material ??= new Material { name = pane.name };
                pic.materialRevo ??= new Material_Revo { name = pane.name };
                FillLegacyDefaults(pic.material, pic.materialRevo);
                FillRevoDefaults(pic.materialRevo, pic.material);
                break;
            case TextBox txt:
                txt.material ??= new Material { name = pane.name };
                txt.materialRevo ??= new Material_Revo { name = pane.name };
                FillLegacyDefaults(txt.material, txt.materialRevo);
                FillRevoDefaults(txt.materialRevo, txt.material);
                break;
            case Window wnd:
                if (wnd.content != null)
                {
                    wnd.content.material ??= new Material { name = pane.name + "_Content" };
                    wnd.content.materialRevo ??= new Material_Revo { name = pane.name + "_Content" };
                    FillLegacyDefaults(wnd.content.material, wnd.content.materialRevo);
                    FillRevoDefaults(wnd.content.materialRevo, wnd.content.material);
                }
                if (wnd.frame != null)
                {
                    foreach (var frame in wnd.frame)
                    {
                        frame.material ??= new Material { name = pane.name + "_Frame" };
                        frame.materialRevo ??= new Material_Revo { name = pane.name + "_Frame" };
                        FillLegacyDefaults(frame.material, frame.materialRevo);
                        FillRevoDefaults(frame.materialRevo, frame.material);
                    }
                }
                break;
        }
    }

    private static void FillLegacyDefaults(Material mat, Material_Revo revo)
    {
        mat.blackColor ??= new BlackColor { r = 0, g = 0, b = 0 };
        mat.whiteColor ??= new WhiteColor { r = 255, g = 255, b = 255 };
        
        if (mat.texMap == null && revo.texMap != null) mat.texMap = revo.texMap;
        if (mat.texMatrix == null && revo.texMatrix != null) mat.texMatrix = revo.texMatrix;
        if (mat.texCoordGen == null && revo.texCoordGen != null) mat.texCoordGen = revo.texCoordGen;

        int requiredStages = Math.Max(1, (int)revo.tevStageNum);
        if (mat.textureStage == null || mat.textureStage.Length < requiredStages)
        {
            var newStages = new MaterialTextureStage[requiredStages];
            int existing = mat.textureStage?.Length ?? 0;
            for (int i = 0; i < requiredStages; i++)
            {
                newStages[i] = i < existing ? mat.textureStage[i] : new MaterialTextureStage { texMap = (sbyte)i, texCoordGen = (sbyte)i };
            }
            mat.textureStage = newStages;
        }
        
        if (mat.texBlendRatio == null || mat.texBlendRatio.Length == 0)
        {
            mat.texBlendRatio = new[] { new TexBlendRatio { color = 255 } };
        }
    }

    private static void FillRevoDefaults(Material_Revo material, Material legacy)
    {
        material.channelControl ??= new[]
        {
            new Material_RevoChannelControl { channel = ChannelID.Color0, materialSource = ColorSource.Vertex },
            new Material_RevoChannelControl { channel = ChannelID.Alpha0, materialSource = ColorSource.Vertex }
        };

        material.matColReg ??= new Color4(255, 255, 255, 255);

        if (material.tevColReg == null || material.tevColReg.Length == 0)
        {
            material.tevColReg = new[]
            {
                new ColorS10_4((short)(legacy.blackColor?.r ?? 0), (short)(legacy.blackColor?.g ?? 0), (short)(legacy.blackColor?.b ?? 0), 0),
                new ColorS10_4((short)(legacy.whiteColor?.r ?? 255), (short)(legacy.whiteColor?.g ?? 255), (short)(legacy.whiteColor?.b ?? 255), 255),
                new ColorS10_4(255, 255, 255, 255)
            };
        }

        material.tevConstReg ??= new[]
        {
            new Color4(255, 255, 255, 255),
            new Color4(255, 255, 255, 255),
            new Color4(255, 255, 255, 255),
            new Color4(255, 255, 255, 255)
        };

        if (material.texMap == null && legacy.texMap != null) material.texMap = legacy.texMap;
        if (material.texMatrix == null && legacy.texMatrix != null) material.texMatrix = legacy.texMatrix;
        if (material.texCoordGen == null && legacy.texCoordGen != null) material.texCoordGen = legacy.texCoordGen;

        material.swapTable ??= new[]
        {
            new Material_RevoSwapTable { r = TevColorChannel.Red, g = TevColorChannel.Green, b = TevColorChannel.Blue, a = TevColorChannel.Alpha },
            new Material_RevoSwapTable { r = TevColorChannel.Red, g = TevColorChannel.Green, b = TevColorChannel.Blue, a = TevColorChannel.Alpha },
            new Material_RevoSwapTable { r = TevColorChannel.Red, g = TevColorChannel.Green, b = TevColorChannel.Blue, a = TevColorChannel.Alpha },
            new Material_RevoSwapTable { r = TevColorChannel.Red, g = TevColorChannel.Green, b = TevColorChannel.Blue, a = TevColorChannel.Alpha }
        };

        material.indirectMatrix ??= new[]
        {
            new TexMatrix { rotate = 0, scale = new Vec2(1, 1), translate = new Vec2(0, 0) },
            new TexMatrix { rotate = 0, scale = new Vec2(1, 1), translate = new Vec2(0, 0) },
            new TexMatrix { rotate = 0, scale = new Vec2(1, 1), translate = new Vec2(0, 0) }
        };

        material.indirectStage ??= new[]
        {
            new Material_RevoIndirectStage { texMap = 0, texCoordGen = 0, scale_s = IndTexScale.V1, scale_t = IndTexScale.V1 },
            new Material_RevoIndirectStage { texMap = 0, texCoordGen = 0, scale_s = IndTexScale.V1, scale_t = IndTexScale.V1 },
            new Material_RevoIndirectStage { texMap = 0, texCoordGen = 0, scale_s = IndTexScale.V1, scale_t = IndTexScale.V1 },
            new Material_RevoIndirectStage { texMap = 0, texCoordGen = 0, scale_s = IndTexScale.V1, scale_t = IndTexScale.V1 }
        };

        int requiredStages = GetRequiredTevStageCount(material, legacy);
        if (material.tevStage == null || material.tevStage.Length < requiredStages)
        {
            var oldStages = material.tevStage ?? Array.Empty<Material_RevoTevStage>();
            var newStages = new Material_RevoTevStage[requiredStages];
            Array.Copy(oldStages, newStages, oldStages.Length);
            for (int i = oldStages.Length; i < requiredStages; i++)
            {
                newStages[i] = CreateDefaultTevStage(i, material, legacy, requiredStages);
            }
            material.tevStage = newStages;
        }
        material.tevStageNum = (byte)material.tevStage.Length;

        // Sanitize every tevStage to ensure color, alpha, and indirect nodes are present
        foreach (var stage in material.tevStage)
        {
            stage.color ??= new Material_RevoTevStageColor { a = TevColorArg.RasC, b = TevColorArg.V0, c = TevColorArg.V0, d = TevColorArg.V0, konst = TevKColorSel.K0, op = TevOpC.Add, bias = TevBias.V0, scale = TevScale.V1, clamp = true, outReg = TevRegID.Prev };
            stage.alpha ??= new Material_RevoTevStageAlpha { a = TevAlphaArg.RasA, b = TevAlphaArg.V0, c = TevAlphaArg.V0, d = TevAlphaArg.V0, konst = TevKAlphaSel.K0_a, op = TevOpA.Add, bias = TevBias.V0, scale = TevScale.V1, clamp = true, outReg = TevRegID.Prev };
            stage.indirect ??= new Material_RevoTevStageIndirect { indStage = 0, format = IndTexFormat.V8, bias = IndTexBiasSel.None, matrix = IndTexMtxID.Off, wrap_s = IndTexWrap.Off, wrap_t = IndTexWrap.Off, addPrev = false, utcLod = false, alpha = IndTexAlphaSel.Off };
        }

        material.alphaCompare ??= new Material_RevoAlphaCompare
        {
            comp0 = Compare.Always, ref0 = 0, op = AlphaOp.And, comp1 = Compare.Always, ref1 = 0
        };

        material.blendMode ??= new Material_RevoBlendMode
        {
            type = BlendMode.Blend, srcFactor = BlendFactorSrc.SrcAlpha, dstFactor = BlendFactorDst.InvSrcAlpha, op = LogicOp.Copy
        };
    }

    private static int GetRequiredTevStageCount(Material_Revo material, Material legacy)
    {
        if (material.tevStageNum != 1 || material.tevStage != null)
        {
            return Math.Max(1, (int)material.tevStageNum);
        }

        return UsesTwoStageLegacyRevoDefault(legacy) ? 2 : 1;
    }

    private static bool UsesTwoStageLegacyRevoDefault(Material legacy)
        => legacy.name?.Contains("JPN", StringComparison.Ordinal) == true
            && legacy.blackColor != null
            && legacy.whiteColor != null
            && legacy.blackColor.r == legacy.whiteColor.r
            && legacy.blackColor.g == legacy.whiteColor.g
            && legacy.blackColor.b == legacy.whiteColor.b
            && legacy.blackColor.r == 0
            && legacy.blackColor.g == 0
            && legacy.blackColor.b == 0;

    private static Material_RevoTevStage CreateDefaultTevStage(int index, Material_Revo material, Material legacy, int stageCount)
    {
        if (stageCount > 1 && index == 0)
        {
            return new Material_RevoTevStage
            {
                colorChannel = TevChannelID.ColorNull,
                texMap = 0,
                texCoordGen = 0,
                rasColSwap = 0,
                texColSwap = 0,
                color = new Material_RevoTevStageColor { a = TevColorArg.C0, b = TevColorArg.C1, c = TevColorArg.TexC, d = TevColorArg.V0, konst = TevKColorSel.K3_a, op = TevOpC.Add, bias = TevBias.V0, scale = TevScale.V1, clamp = false, outReg = TevRegID.Prev },
                alpha = new Material_RevoTevStageAlpha { a = TevAlphaArg.A0, b = TevAlphaArg.A1, c = TevAlphaArg.TexA, d = TevAlphaArg.V0, konst = TevKAlphaSel.K3_a, op = TevOpA.Add, bias = TevBias.V0, scale = TevScale.V1, clamp = false, outReg = TevRegID.Prev },
                indirect = new Material_RevoTevStageIndirect { indStage = 0, format = IndTexFormat.V8, bias = IndTexBiasSel.None, matrix = IndTexMtxID.Off, wrap_s = IndTexWrap.Off, wrap_t = IndTexWrap.Off, addPrev = false, utcLod = false, alpha = IndTexAlphaSel.Off }
            };
        }

        return new Material_RevoTevStage
        {
            colorChannel = TevChannelID.Color0a0,
            texMap = -1,
            texCoordGen = -1,
            rasColSwap = 0,
            texColSwap = 0,
            color = new Material_RevoTevStageColor { a = TevColorArg.RasC, b = TevColorArg.V0, c = TevColorArg.V0, d = TevColorArg.V0, konst = TevKColorSel.K0, op = TevOpC.Add, bias = TevBias.V0, scale = TevScale.V1, clamp = true, outReg = TevRegID.Prev },
            alpha = new Material_RevoTevStageAlpha { a = TevAlphaArg.RasA, b = TevAlphaArg.V0, c = TevAlphaArg.V0, d = TevAlphaArg.V0, konst = TevKAlphaSel.K0_a, op = TevOpA.Add, bias = TevBias.V0, scale = TevScale.V1, clamp = true, outReg = TevRegID.Prev },
            indirect = new Material_RevoTevStageIndirect { indStage = 0, format = IndTexFormat.V8, bias = IndTexBiasSel.None, matrix = IndTexMtxID.Off, wrap_s = IndTexWrap.Off, wrap_t = IndTexWrap.Off, addPrev = false, utcLod = false, alpha = IndTexAlphaSel.Off }
        };
    }
}
