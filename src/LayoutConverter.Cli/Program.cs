using LayoutConverter.Conversion.Options;
using LayoutConverter.Conversion.Pipeline;

try
{
    var options = ConverterOptions.Parse(args);
    using var writer = BuildWriter(options.Execution.LogFilePath);
    var pipeline = new ConversionPipeline();
    var exitCode = pipeline.Run(options, writer);
    return (int)exitCode;
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine(ex.Message);
    return (int)ConversionExitCode.InvalidArguments;
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex);
    return (int)ConversionExitCode.UnexpectedFailure;
}

static TextWriter BuildWriter(string? logFilePath)
{
    if (string.IsNullOrWhiteSpace(logFilePath))
    {
        return Console.Out;
    }

    var fullPath = Path.GetFullPath(logFilePath);
    Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
    return TextWriter.Synchronized(new StreamWriter(fullPath, append: false));
}
