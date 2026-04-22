using LayoutConverter.Conversion.Export;
using LayoutConverter.Conversion.Options;
using LayoutConverter.Conversion.Requests;

namespace LayoutConverter.Conversion.Routing;

public sealed class LayoutConversionRouteHandler : IConversionRouteHandler
{
    private readonly LayoutBinaryExportCoordinator _layoutExportCoordinator = new();

    public bool CanHandle(ConversionRequest request)
    {
        return request is LayoutConversionRequest;
    }

    public ConversionExitCode Execute(ConversionRequest request, TextWriter log)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(log);

        if (request is not LayoutConversionRequest layoutRequest)
        {
            throw new ArgumentException($"Expected {nameof(LayoutConversionRequest)} but received {request.GetType().Name}.", nameof(request));
        }

        log.WriteLine(_layoutExportCoordinator.BuildAnalysisSummary(layoutRequest));
        _layoutExportCoordinator.ExportLayoutBinary(layoutRequest);
        log.WriteLine($"BRLYT written to: {layoutRequest.DestinationPath}");
        return ConversionExitCode.Success;
    }
}
