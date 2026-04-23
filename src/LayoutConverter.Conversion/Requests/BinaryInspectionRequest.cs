using LayoutConverter.Conversion.Infrastructure;
using LayoutConverter.Conversion.Options;

namespace LayoutConverter.Conversion.Requests;

public sealed class BinaryInspectionRequest : ConversionRequest
{
    public BinaryInspectionRequest(
        ConverterFileType fileType,
        string sourcePath,
        string destinationPath,
        ExecutionOptions execution,
        XmlLoadOptions xmlLoad)
        : base(fileType, sourcePath, destinationPath, execution, xmlLoad)
    {
    }
}
