using System.Xml.Serialization;
using LayoutConverter.Core.Schema.Rlan;

namespace LayoutConverter.Core.Rlan;

public sealed class RlanDocumentLoader
{
    private static readonly XmlSerializer Serializer = new(typeof(Document));

    public Document Load(string path, bool skipVersionCheck = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        using var stream = File.OpenRead(path);
        var document = (Document?)Serializer.Deserialize(stream)
            ?? throw new InvalidOperationException("Unable to deserialize RLAN XML document.");

        if (!skipVersionCheck && !IsSupportedVersion(document.version))
        {
            throw new InvalidOperationException($"Unsupported RLAN XML version '{document.version}'. Expected 1.2.*.");
        }

        if (document.body?.rlan is null || document.body.rlan.Length == 0)
        {
            throw new InvalidOperationException("The XML document does not contain RLAN animation data.");
        }

        return document;
    }

    private static bool IsSupportedVersion(string? version)
        => version is not null && version.StartsWith("1.2.", StringComparison.Ordinal);
}
