using LayoutConverter.Conversion.Infrastructure;
using LayoutConverter.Conversion.Options;

namespace LayoutConverter.Conversion.Requests;

public sealed class LayoutConversionRequest : ConversionRequest
{
    public LayoutConversionRequest(
        string sourcePath,
        string outputRootPath,
        string destinationPath,
        ExecutionOptions execution,
        XmlLoadOptions xmlLoad,
        LayoutRouteOptions layout)
        : base(ConverterFileType.Layout, sourcePath, destinationPath, execution, xmlLoad)
    {
        OutputRootPath = outputRootPath;
        Layout = layout;
    }

    public string OutputRootPath { get; }
    public LayoutRouteOptions Layout { get; }
}
