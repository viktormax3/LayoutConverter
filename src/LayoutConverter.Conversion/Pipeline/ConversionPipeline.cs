using LayoutConverter.Conversion.Infrastructure;
using LayoutConverter.Conversion.Options;
using LayoutConverter.Conversion.Requests;
using LayoutConverter.Conversion.Routing;
using LayoutConverter.Conversion.Validation;

namespace LayoutConverter.Conversion.Pipeline;

public sealed class ConversionPipeline
{
    private readonly ConversionRouteRegistry _routeRegistry = new();
    private readonly ConversionRequestValidator _validator = new();

    public ConversionExitCode Run(ConverterOptions options, TextWriter log)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(log);

        if (options.ShowHelp || options.InputPaths.Count == 0)
        {
            WriteUsage(log);
            return options.ShowHelp ? ConversionExitCode.Success : ConversionExitCode.InvalidArguments;
        }

        foreach (var inputArgument in options.InputPaths)
        {
            string[] inputs;
            bool sourceWasDirectory;
            try
            {
                inputs = ConverterPathHelper.ResolveInputPaths(inputArgument, out sourceWasDirectory);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                log.WriteLine(ex.Message);
                return ConversionExitCode.InputAccessFailure;
            }

            foreach (var inputPath in inputs)
            {
                var fileType = ConverterPathHelper.DetectFileType(inputPath);
                var request = ConversionRequestFactory.Create(inputPath, options);
                if (request is null)
                {
                    if (sourceWasDirectory)
                    {
                        continue;
                    }

                    log.WriteLine(fileType == ConverterFileType.Unknown
                        ? $"Unknown file type: {inputPath}"
                        : $"Unable to create a conversion request for: {inputPath}");

                    return fileType == ConverterFileType.Unknown
                        ? ConversionExitCode.UnknownFileType
                        : ConversionExitCode.InvalidArguments;
                }

                foreach (var warning in _validator.GetWarnings(request, options))
                {
                    log.WriteLine($"Warning: {warning}");
                }

                var handler = _routeRegistry.Resolve(request);
                if (handler is null)
                {
                    log.WriteLine($"No route handler is registered for request type: {request.GetType().Name}");
                    return ConversionExitCode.NotYetImplemented;
                }

                var exitCode = handler.Execute(request, log);
                if (exitCode != ConversionExitCode.Success)
                {
                    return exitCode;
                }
            }
        }

        return ConversionExitCode.Success;
    }

    private static void WriteUsage(TextWriter log)
    {
        log.WriteLine("layout-converter [options] SOURCE... DESTDIR");
        log.WriteLine("layout-converter [options] --output <destdir> SOURCE...");
        log.WriteLine("  -h, --help                 Show help");
        log.WriteLine("  -- Common options --");
        log.WriteLine("  -u                         Update-only mode");
        log.WriteLine("  -i                         Disable cvtrchar conversion");
        log.WriteLine("      --no-check-version     Skip version guard");
        log.WriteLine("      --xsd-validate         Enable XML/XSD validation");
        log.WriteLine("      --logfile <path>       Redirect log output");
        log.WriteLine("  -- Layout route (.rlyt -> .brlyt) --");
        log.WriteLine("      --banner               Use banner-oriented BRLYT profile");
        log.WriteLine("  -- Animation routes (.rlan/.rlpa/.rlvi/.rlvc/.rlmc/.rlts/.rltp) --");
        log.WriteLine("  -g                         Split animation outputs by tag");
        log.WriteLine("      --no-taginfo           Omit tag info block");
        log.WriteLine("      --omit-samekey         Drop duplicate keys after first tag");
        log.WriteLine("      --omit-samekey-all     Drop duplicate keys across all tags");
        log.WriteLine("      --bake-infinity        Bake infinity area key");
        log.WriteLine("      --cvtr-ref-tex-only    Export only referenced textures");
        log.WriteLine("  -- Reverse/inspection routes --");
        log.WriteLine("      .tpl                    Decode supported TPL textures to TGA");
        log.WriteLine("      .brlan                  Reconstruct RLAN XML");
        log.WriteLine("      .brlyt                  Reconstruct RLYT XML");
    }
}
