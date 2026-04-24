using LayoutConverter.Conversion.Requests;
using LayoutConverter.Core.Brlyt;
using LayoutConverter.Core.Rlyt;

namespace LayoutConverter.Conversion.Export;

public sealed class LayoutBinaryExportCoordinator
{
    private readonly RlytDocumentLoader _loader = new();
    private readonly LayoutResourceExportCoordinator _resourceExporter = new();

    public string BuildAnalysisSummary(LayoutConversionRequest request)
    {
        var report = _loader.Analyze(
            request.SourcePath,
            request.XmlLoad.SkipVersionCheck,
            request.XmlLoad.EnableXsdValidation);

        return string.Join(Environment.NewLine, new[]
        {
            $"Source: {report.SourcePath}",
            $"Version: {report.Version}",
            $"Panes: {report.TotalPaneCount} (Picture={report.PicturePaneCount}, Null={report.NullPaneCount})",
            $"Materials: {report.MaterialCount}",
            $"Texture maps: {report.TextureMapCount}",
            $"Groups: {report.GroupCount}",
        });
    }

    public void ExportLayoutBinary(LayoutConversionRequest request, TextWriter? log = null)
    {
        ArgumentNullException.ThrowIfNull(request);

        var document = _loader.Load(
            request.SourcePath,
            request.XmlLoad.SkipVersionCheck,
            request.XmlLoad.EnableXsdValidation);
        var context = BrlytDocumentContext.FromDocument(document);

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(request.DestinationPath))!);
        using (var output = File.Create(request.DestinationPath))
        {
            var writer = new BrlytBinaryWriter(
                output,
                request.Layout.Flavor,
                request.XmlLoad.SuppressCvtrCharConversion);
            writer.WriteDocument(context);
        }

        _resourceExporter.ExportResources(request, context, log);
    }
}
