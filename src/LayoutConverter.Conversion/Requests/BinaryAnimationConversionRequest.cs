using LayoutConverter.Conversion.Infrastructure;
using LayoutConverter.Conversion.Options;

namespace LayoutConverter.Conversion.Requests;

public sealed class BinaryAnimationConversionRequest : ConversionRequest
{
    public BinaryAnimationConversionRequest(
        string sourcePath,
        string destinationPath,
        ExecutionOptions execution,
        XmlLoadOptions xmlLoad)
        : base(ConverterFileType.BinaryAnimation, sourcePath, destinationPath, execution, xmlLoad)
    {
    }
}
