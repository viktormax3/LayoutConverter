using LayoutConverter.Conversion.Options;
using LayoutConverter.Conversion.Export;
using LayoutConverter.Conversion.Requests;

namespace LayoutConverter.Conversion.Routing;

public sealed class AnimationConversionRouteHandler : IConversionRouteHandler
{
    private readonly AnimationBinaryExportCoordinator _animationExportCoordinator = new();

    public bool CanHandle(ConversionRequest request)
    {
        return request is AnimationConversionRequest;
    }

    public ConversionExitCode Execute(ConversionRequest request, TextWriter log)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(log);

        if (request is not AnimationConversionRequest animationRequest)
        {
            throw new ArgumentException($"Expected {nameof(AnimationConversionRequest)} but received {request.GetType().Name}.", nameof(request));
        }

        var outputs = _animationExportCoordinator.ExportAnimationBinary(animationRequest);
        foreach (var output in outputs)
        {
            log.WriteLine($"BRLAN written to: {output}");
        }

        return ConversionExitCode.Success;
    }
}
