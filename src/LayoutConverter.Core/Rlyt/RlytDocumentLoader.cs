using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using LayoutConverter.Core.Schema.Rlyt;

namespace LayoutConverter.Core.Rlyt;

public sealed class RlytDocumentLoader
{
    private static readonly XmlSerializer Serializer = new(typeof(Document));
    private const int SupportedMajorVersion = 1;
    private const int SupportedMaxMinorVersion = 2;

    public Document Load(
        string path,
        bool skipVersionCheck = false,
        bool enableXsdValidation = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        using var stream = File.OpenRead(path);
        using var reader = XmlReader.Create(stream, CreateReaderSettings(path, enableXsdValidation));
        var document = (Document)Serializer.Deserialize(reader)!;

        if (!skipVersionCheck)
        {
            ValidateVersion(document.version);
        }

        return document;
    }

    public RlytAnalysisReport Analyze(
        string path,
        bool skipVersionCheck = false,
        bool enableXsdValidation = false)
    {
        var document = Load(path, skipVersionCheck, enableXsdValidation);
        var layout = document.body?.rlyt
            ?? throw new InvalidOperationException("The XML document does not contain an RLYT body.");

        var paneNames = new List<string>();
        var materialNames = new List<string>();

        int totalPaneCount = 0;
        int picturePaneCount = 0;
        int nullPaneCount = 0;
        int materialCount = 0;
        int textureMapCount = 0;
        int groupCount = CountGroups(layout.groupSet?.group);

        foreach (var pane in layout.paneSet ?? Array.Empty<Pane>())
        {
            CountPane(pane, paneNames, materialNames, ref totalPaneCount, ref picturePaneCount, ref nullPaneCount, ref materialCount, ref textureMapCount);
        }

        return new RlytAnalysisReport(
            SourcePath: path,
            Version: document.version ?? "unknown",
            TotalPaneCount: totalPaneCount,
            PicturePaneCount: picturePaneCount,
            NullPaneCount: nullPaneCount,
            MaterialCount: materialCount,
            TextureMapCount: textureMapCount,
            GroupCount: groupCount,
            PaneNames: paneNames,
            MaterialNames: materialNames);
    }

    private static void CountPane(
        Pane pane,
        List<string> paneNames,
        List<string> materialNames,
        ref int totalPaneCount,
        ref int picturePaneCount,
        ref int nullPaneCount,
        ref int materialCount,
        ref int textureMapCount)
    {
        totalPaneCount++;

        if (!string.IsNullOrWhiteSpace(pane.name))
        {
            paneNames.Add(pane.name);
        }

        switch (pane.kind)
        {
            case PaneKind.Picture:
                picturePaneCount++;
                if (pane.Item is Picture picture && picture.material is { } pictureMaterial)
                {
                    AddMaterial(pictureMaterial, materialNames, ref materialCount, ref textureMapCount);
                }
                break;

            case PaneKind.TextBox:
                if (pane.Item is TextBox textBox && textBox.material is { } textMaterial)
                {
                    AddMaterial(textMaterial, materialNames, ref materialCount, ref textureMapCount);
                }
                break;

            case PaneKind.Window:
                if (pane.Item is Window window && window.content?.material is { } windowMaterial)
                {
                    AddMaterial(windowMaterial, materialNames, ref materialCount, ref textureMapCount);
                }
                break;

            case PaneKind.Null:
                nullPaneCount++;
                break;
        }
    }

    private static void AddMaterial(
        Material material,
        List<string> materialNames,
        ref int materialCount,
        ref int textureMapCount)
    {
        materialCount++;

        if (!string.IsNullOrWhiteSpace(material.name))
        {
            materialNames.Add(material.name);
        }

        textureMapCount += material.texMap?.Length ?? 0;
    }

    private static int CountGroups(Group? rootGroup)
    {
        if (rootGroup is null)
        {
            return 0;
        }

        int count = 1;
        foreach (var child in rootGroup.group ?? Array.Empty<Group>())
        {
            count += CountGroups(child);
        }

        return count;
    }

    private static XmlReaderSettings CreateReaderSettings(string path, bool enableXsdValidation)
    {
        var settings = new XmlReaderSettings
        {
            CloseInput = false,
            ValidationType = enableXsdValidation ? ValidationType.Schema : ValidationType.None,
        };

        if (!enableXsdValidation)
        {
            return settings;
        }

        string xsdPath = GetRlytSchemaPath();
        settings.Schemas = new XmlSchemaSet();
        settings.Schemas.Add(null, xsdPath);
        settings.ValidationEventHandler += (_, args) =>
            throw new InvalidDataException($"XML schema validation failed for '{path}': {args.Message}", args.Exception);

        return settings;
    }

    private static string GetRlytSchemaPath()
    {
        string schemaRoot = Environment.ExpandEnvironmentVariables(
            @"%NW4R_ROOT%\XMLSchemata\LayoutEditorFileFormat\ver_0_4");
        return Path.Combine(schemaRoot, "rlyt.xsd");
    }

    private static void ValidateVersion(string? version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            ThrowUnsupportedVersion(version ?? string.Empty);
            return;
        }

        string[] parts = version.Split('.');
        if (parts.Length != 3 ||
            !int.TryParse(parts[0], out int major) ||
            !int.TryParse(parts[1], out int minor) ||
            major != SupportedMajorVersion ||
            minor > SupportedMaxMinorVersion)
        {
            ThrowUnsupportedVersion(version);
        }
    }

    private static void ThrowUnsupportedVersion(string version)
    {
        throw new InvalidDataException(
            $"Unsupported version of layout file. - '{version}'{Environment.NewLine}" +
            $"Supported versions are '{SupportedMajorVersion}.{SupportedMaxMinorVersion}.*'.");
    }
}
