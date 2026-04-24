using System.Text.Json;
using LayoutConverter.Core.Schema.Rlyt;

namespace LayoutConverter.Core.Brlyt;

public sealed class BrlytDocumentContext
{
    private readonly Dictionary<string, Pane> _panesByName;
    private readonly Dictionary<string, int> _materialIndicesByName;

    private BrlytDocumentContext(
        Document sourceDocument,
        RLYT layout,
        IReadOnlyList<Pane> panes,
        IReadOnlyList<BrlytMaterialEntry> materials,
        IReadOnlyList<TextureFile> allTextures,
        IReadOnlyList<TextureFile> textures,
        IReadOnlyList<FontFile> fonts,
        PaneTree? rootPaneTree,
        Group? rootGroup)
    {
        SourceDocument = sourceDocument;
        Layout = layout;
        Panes = panes;
        Materials = materials;
        AllTextures = allTextures;
        Textures = textures;
        Fonts = fonts;
        RootPaneTree = rootPaneTree;
        RootGroup = rootGroup;

        _panesByName = panes
            .Where(static pane => !string.IsNullOrWhiteSpace(pane.name))
            .GroupBy(static pane => pane.name, StringComparer.Ordinal)
            .ToDictionary(static g => g.Key!, static g => g.First(), StringComparer.Ordinal);

        _materialIndicesByName = materials
            .Select(static (material, index) => (material, index))
            .Where(static tuple => !string.IsNullOrWhiteSpace(tuple.material.Name))
            .GroupBy(static tuple => tuple.material.Name, StringComparer.Ordinal)
            .ToDictionary(static g => g.Key, static g => g.First().index, StringComparer.Ordinal);
    }

    public Document SourceDocument { get; }
    public RLYT Layout { get; }
    public IReadOnlyList<Pane> Panes { get; }
    public IReadOnlyList<BrlytMaterialEntry> Materials { get; }
    public IReadOnlyList<TextureFile> AllTextures { get; }
    public IReadOnlyList<TextureFile> Textures { get; }
    public IReadOnlyList<FontFile> Fonts { get; }
    public PaneTree? RootPaneTree { get; }
    public Group? RootGroup { get; }

    public Pane? FindPane(string name)
        => _panesByName.GetValueOrDefault(name);

    public int ResolveMaterialIndex(Material? material)
        => ResolveMaterialIndex(material?.name);

    public int ResolveMaterialIndex(Material_Revo? material)
        => ResolveMaterialIndex(material?.name);

    public int ResolveMaterialIndex(string? materialName)
    {
        if (string.IsNullOrWhiteSpace(materialName))
        {
            return 0;
        }

        if (_materialIndicesByName.TryGetValue(materialName, out int index))
        {
            return index;
        }

        return TryParseLogicalIndex(materialName, "Material", out index)
            ? index
            : 0;
    }

    public static BrlytDocumentContext FromDocument(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var layout = document.body?.rlyt
            ?? throw new InvalidOperationException("The XML document does not contain an RLYT body.");

        var panes = layout.paneSet?.ToArray() ?? Array.Empty<Pane>();
        var materials = CollectDistinctMaterials(panes);
        var allTextures = CollectSortedTextures(layout.textureFile);
        var textures = CollectUsedTextures(materials, allTextures);
        var fonts = CollectUsedFonts(panes, CollectSortedFonts(layout.fontFile));

        return new BrlytDocumentContext(
            sourceDocument: document,
            layout: layout,
            panes: panes,
            materials: materials,
            allTextures: allTextures,
            textures: textures,
            fonts: fonts,
            rootPaneTree: layout.paneHierarchy?.paneTree,
            rootGroup: layout.groupSet?.group);
    }


    private static IReadOnlyList<TextureFile> CollectSortedTextures(IEnumerable<TextureFile>? textures)
    {
        var sorted = new SortedDictionary<string, TextureFile>(StringComparer.InvariantCultureIgnoreCase);

        foreach (var texture in textures ?? Array.Empty<TextureFile>())
        {
            string name = texture.GetName();
            sorted.Add(name, texture);
        }

        return sorted.Values.ToArray();
    }

    private static IReadOnlyList<TextureFile> CollectUsedTextures(
        IReadOnlyList<BrlytMaterialEntry> materials,
        IReadOnlyList<TextureFile> sortedTextures)
    {
        var allTexturesByName = sortedTextures
            .ToDictionary(static texture => texture.GetName(), StringComparer.InvariantCultureIgnoreCase);

        var usedTextures = new SortedDictionary<string, TextureFile>(StringComparer.InvariantCultureIgnoreCase);

        foreach (var material in materials)
        {
            foreach (var texMap in material.TexMaps)
            {
                if (string.IsNullOrWhiteSpace(texMap.imageName))
                {
                    continue;
                }

                if (usedTextures.ContainsKey(texMap.imageName))
                {
                    continue;
                }

                if (!allTexturesByName.TryGetValue(texMap.imageName, out var texture))
                {
                    throw new InvalidOperationException($"Texture '{texMap.imageName}' referenced by material '{material.Name}' was not found.");
                }

                usedTextures.Add(texMap.imageName, texture);
            }
        }

        return usedTextures.Values.ToArray();
    }

    private static IReadOnlyList<FontFile> CollectSortedFonts(IEnumerable<FontFile>? fonts)
    {
        var sorted = new SortedDictionary<string, FontFile>(StringComparer.InvariantCultureIgnoreCase);

        foreach (var font in fonts ?? Array.Empty<FontFile>())
        {
            string name = font.GetName();
            sorted.Add(name, font);
        }

        return sorted.Values.ToArray();
    }

    private static IReadOnlyList<FontFile> CollectUsedFonts(
        IReadOnlyList<Pane> panes,
        IReadOnlyList<FontFile> sortedFonts)
    {
        var allFontsByName = sortedFonts
            .ToDictionary(static font => font.GetName(), StringComparer.InvariantCultureIgnoreCase);

        var usedFonts = new SortedDictionary<string, FontFile>(StringComparer.InvariantCultureIgnoreCase);

        foreach (var pane in panes)
        {
            if (pane.kind != PaneKind.TextBox || pane.Item is not TextBox textBox)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(textBox.font))
            {
                continue;
            }

            if (!allFontsByName.TryGetValue(textBox.font, out var font))
            {
                if (!IsLogicalFontName(textBox.font) || sortedFonts.Count == 0)
                {
                    throw new InvalidOperationException($"Font '{textBox.font}' referenced by textbox '{pane.name ?? string.Empty}' was not found.");
                }

                font = sortedFonts[0];
            }

            string key = font.GetName();
            if (!usedFonts.ContainsKey(key))
            {
                usedFonts.Add(key, font);
            }
        }

        return usedFonts.Values.ToArray();
    }

    private static bool IsLogicalFontName(string fontName)
    {
        const string prefix = "Font";

        if (!fontName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) || fontName.Length == prefix.Length)
        {
            return false;
        }

        for (int i = prefix.Length; i < fontName.Length; i++)
        {
            if (!char.IsDigit(fontName[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static readonly JsonSerializerOptions MaterialJsonOptions = new()
    {
        WriteIndented = false,
    };

    private static IReadOnlyList<BrlytMaterialEntry> CollectDistinctMaterials(IEnumerable<Pane> panes)
    {
        var materials = new List<BrlytMaterialEntry>();
        var materialsByName = new Dictionary<string, BrlytMaterialEntry>(StringComparer.Ordinal);

        foreach (var pane in panes)
        {
            foreach (var entry in ExtractMaterials(pane))
            {
                string key = entry.Name ?? string.Empty;

                if (materialsByName.TryGetValue(key, out var existing))
                {
                    if (!MaterialContentEquals(existing, entry))
                    {
                        throw new InvalidOperationException($"Duplicate material name '{key}' has different content.");
                    }

                    continue;
                }

                materialsByName.Add(key, entry);
                materials.Add(entry);
            }
        }

        return materials;
    }

    private static bool MaterialContentEquals(BrlytMaterialEntry left, BrlytMaterialEntry right)
        => string.Equals(
            JsonSerializer.Serialize(left, MaterialJsonOptions),
            JsonSerializer.Serialize(right, MaterialJsonOptions),
            StringComparison.Ordinal);

    private static IEnumerable<BrlytMaterialEntry> ExtractMaterials(Pane pane)
    {
        switch (pane.Item)
        {
            case Picture picture:
                if (picture.material is not null || picture.materialRevo is not null)
                {
                    yield return BrlytMaterialEntry.From(picture.material, picture.materialRevo, supportsTexturePayload: true, useRevoMaterial: picture.detailSetting);
                }
                break;

            case TextBox textBox:
                if (textBox.material is not null || textBox.materialRevo is not null)
                {
                    yield return BrlytMaterialEntry.From(textBox.material, textBox.materialRevo, supportsTexturePayload: false, useRevoMaterial: false);
                }
                break;

            case Window window:
                if (window.content?.material is not null || window.content?.materialRevo is not null)
                {
                    yield return BrlytMaterialEntry.From(window.content?.material, window.content?.materialRevo, supportsTexturePayload: true, useRevoMaterial: window.content?.detailSetting ?? false);
                }

                foreach (var frame in window.frame ?? Array.Empty<WindowFrame>())
                {
                    if (frame.material is null && frame.materialRevo is null)
                    {
                        continue;
                    }

                    yield return BrlytMaterialEntry.From(frame.material, frame.materialRevo, supportsTexturePayload: true, useRevoMaterial: frame.detailSetting);
                }
                break;
        }
    }

    private static bool TryParseLogicalIndex(string value, string prefix, out int index)
    {
        index = 0;

        if (!value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) || value.Length == prefix.Length)
        {
            return false;
        }

        for (int i = prefix.Length; i < value.Length; i++)
        {
            if (!char.IsDigit(value[i]))
            {
                return false;
            }
        }

        return int.TryParse(value[prefix.Length..], out index);
    }
}
