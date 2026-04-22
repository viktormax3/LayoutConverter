using LayoutConverter.Conversion.Infrastructure;
using LayoutConverter.Conversion.Options;

namespace LayoutConverter.Conversion.Requests;

public abstract class ConversionRequest
{
    protected ConversionRequest(
        ConverterFileType fileType,
        string sourcePath,
        string destinationPath,
        ExecutionOptions execution,
        XmlLoadOptions xmlLoad)
    {
        FileType = fileType;
        SourcePath = sourcePath;
        DestinationPath = destinationPath;
        Execution = execution;
        XmlLoad = xmlLoad;
    }

    public ConverterFileType FileType { get; }
    public string SourcePath { get; }
    public string DestinationPath { get; }
    public ExecutionOptions Execution { get; }
    public XmlLoadOptions XmlLoad { get; }
}
