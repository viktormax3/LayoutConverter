using LayoutConverter.Conversion.Options;
using LayoutConverter.Conversion.Requests;

namespace LayoutConverter.Conversion.Routing;

public interface IConversionRouteHandler
{
    bool CanHandle(ConversionRequest request);
    ConversionExitCode Execute(ConversionRequest request, TextWriter log);
}
