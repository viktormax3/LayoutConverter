using System.Xml.Serialization;
using LayoutConverter.Conversion.Options;
using LayoutConverter.Conversion.Requests;
using LayoutConverter.Core.Brlan;
using LayoutConverter.Core.Schema.Rlan;

namespace LayoutConverter.Conversion.Routing;

public sealed class BinaryAnimationConversionRouteHandler : IConversionRouteHandler
{
    private static readonly XmlSerializer Serializer = new(typeof(Document));

    public bool CanHandle(ConversionRequest request)
        => request is BinaryAnimationConversionRequest;

    public ConversionExitCode Execute(ConversionRequest request, TextWriter log)
    {
        var animationRequest = (BinaryAnimationConversionRequest)request;
        try
        {
            var document = BrlanBinaryReader.ReadDocument(animationRequest.SourcePath);
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(animationRequest.DestinationPath))!);
            using var output = File.Create(animationRequest.DestinationPath);
            Serializer.Serialize(output, document);
            log.WriteLine($"Wrote {animationRequest.DestinationPath}");
            return ConversionExitCode.Success;
        }
        catch (Exception ex) when (ex is InvalidDataException or EndOfStreamException or IOException)
        {
            log.WriteLine(ex.Message);
            return ConversionExitCode.InputAccessFailure;
        }
    }
}
