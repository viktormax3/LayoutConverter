using LayoutConverter.Conversion.Requests;

namespace LayoutConverter.Conversion.Routing;

public sealed class ConversionRouteRegistry
{
    private readonly IReadOnlyList<IConversionRouteHandler> _handlers;

    public ConversionRouteRegistry()
        : this(new IConversionRouteHandler[]
        {
            new LayoutConversionRouteHandler(),
            new AnimationConversionRouteHandler(),
            new BinaryLayoutConversionRouteHandler(),
            new BinaryAnimationConversionRouteHandler(),
            new TextureConversionRouteHandler(),
            new BinaryInspectionRouteHandler(),
        })
    {
    }

    public ConversionRouteRegistry(IReadOnlyList<IConversionRouteHandler> handlers)
    {
        _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
    }

    public IConversionRouteHandler? Resolve(ConversionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _handlers.FirstOrDefault(handler => handler.CanHandle(request));
    }
}
