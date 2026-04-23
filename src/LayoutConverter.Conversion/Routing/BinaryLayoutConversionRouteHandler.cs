using System.Xml.Serialization;
using LayoutConverter.Conversion.Options;
using LayoutConverter.Conversion.Requests;
using LayoutConverter.Core.Brlyt;
using LayoutConverter.Core.Schema.Rlyt;

namespace LayoutConverter.Conversion.Routing;

public sealed class BinaryLayoutConversionRouteHandler : IConversionRouteHandler
{
    private static readonly XmlSerializer Serializer = new(typeof(Document));

    public bool CanHandle(ConversionRequest request)
        => request is BinaryLayoutConversionRequest;

    public ConversionExitCode Execute(ConversionRequest request, TextWriter log)
    {
        var layoutRequest = (BinaryLayoutConversionRequest)request;
        try
        {
            var document = BrlytBinaryReader.ReadDocument(layoutRequest.SourcePath);
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(layoutRequest.DestinationPath))!);
            using var output = File.Create(layoutRequest.DestinationPath);
            Serializer.Serialize(output, document);
            log.WriteLine($"Wrote {layoutRequest.DestinationPath}");
            return ConversionExitCode.Success;
        }
        catch (Exception ex) when (ex is InvalidDataException or EndOfStreamException or IOException)
        {
            log.WriteLine(ex.Message);
            return ConversionExitCode.InputAccessFailure;
        }
    }
}
