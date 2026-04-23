using LayoutConverter.Conversion.Infrastructure;
using LayoutConverter.Conversion.Options;

namespace LayoutConverter.Conversion.Requests;

public sealed class TextureConversionRequest : ConversionRequest
{
    public TextureConversionRequest(
        string sourcePath,
        string destinationPath,
        ExecutionOptions execution,
        XmlLoadOptions xmlLoad)
        : base(ConverterFileType.Texture, sourcePath, destinationPath, execution, xmlLoad)
    {
    }
}
