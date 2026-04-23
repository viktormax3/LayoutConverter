using LayoutConverter.Conversion.Options;
using LayoutConverter.Conversion.Requests;
using LayoutConverter.Conversion.Textures;

namespace LayoutConverter.Conversion.Routing;

public sealed class TextureConversionRouteHandler : IConversionRouteHandler
{
    public bool CanHandle(ConversionRequest request)
        => request is TextureConversionRequest;

    public ConversionExitCode Execute(ConversionRequest request, TextWriter log)
    {
        var textureRequest = (TextureConversionRequest)request;
        try
        {
            TplToTgaConverter.Convert(textureRequest.SourcePath, textureRequest.DestinationPath);
            log.WriteLine($"Wrote {textureRequest.DestinationPath}");
            return ConversionExitCode.Success;
        }
        catch (NotSupportedException ex)
        {
            log.WriteLine(ex.Message);
            return ConversionExitCode.NotYetImplemented;
        }
        catch (InvalidDataException ex)
        {
            log.WriteLine(ex.Message);
            return ConversionExitCode.InputAccessFailure;
        }
        catch (IOException ex)
        {
            log.WriteLine(ex.Message);
            return ConversionExitCode.InputAccessFailure;
        }
    }
}
