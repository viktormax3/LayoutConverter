using LayoutConverter.Conversion.Infrastructure;
using LayoutConverter.Conversion.Options;

namespace LayoutConverter.Conversion.Requests;

public sealed class AnimationConversionRequest : ConversionRequest
{
    public AnimationConversionRequest(
        ConverterFileType fileType,
        string sourcePath,
        string destinationPath,
        ExecutionOptions execution,
        XmlLoadOptions xmlLoad,
        AnimationRouteOptions animation)
        : base(fileType, sourcePath, destinationPath, execution, xmlLoad)
    {
        Animation = animation;
    }

    public AnimationRouteOptions Animation { get; }
}
