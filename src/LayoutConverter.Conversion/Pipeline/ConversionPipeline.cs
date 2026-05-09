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

        if (options.Animation.MergeAnimations)
        {
            var mergeResult = ExecuteAnimationMerge(options, log);
            if (mergeResult != ConversionExitCode.Success)
            {
                return mergeResult;
            }
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

                // Skip individual BRLANs if they were already processed by the merge handler
                if (options.Animation.MergeAnimations && fileType == ConverterFileType.BinaryAnimation)
                {
                    continue;
                }

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
        log.WriteLine("      --merge-anim            Merge multiple splitted BRLANs into one RLAN master");
        log.WriteLine("      .brlyt                  Reconstruct RLYT XML");
    }

    private ConversionExitCode ExecuteAnimationMerge(ConverterOptions options, TextWriter log)
    {
        try
        {
            var inputs = new System.Collections.Generic.List<string>();
            foreach (var p in options.InputPaths)
            {
                if (Directory.Exists(p)) 
                    inputs.AddRange(Directory.GetFiles(p, "*.brlan", System.IO.SearchOption.AllDirectories));
                else 
                    inputs.Add(p);
            }

            if (inputs.Count == 0)
            {
                log.WriteLine("Error: No BRLAN files provided for merge.");
                return ConversionExitCode.InvalidArguments;
            }

            string layoutName;
            string outFilePath;

            // Detect if OutputPath is a directory (no extension or already exists as directory)
            bool isOutputDir = string.IsNullOrEmpty(Path.GetExtension(options.OutputPath)) || Directory.Exists(options.OutputPath);

            if (isOutputDir)
            {
                var firstInputName = Path.GetFileNameWithoutExtension(inputs[0]);
                var underscoreIndex = firstInputName.IndexOf('_');
                layoutName = underscoreIndex > 0 ? firstInputName.Substring(0, underscoreIndex) : firstInputName;
                outFilePath = Path.Combine(options.OutputPath, "Layout", layoutName + ".rlan");
            }
            else
            {
                layoutName = Path.GetFileNameWithoutExtension(options.OutputPath);
                outFilePath = options.OutputPath;
            }

            if (string.IsNullOrEmpty(layoutName))
            {
                log.WriteLine("Error: Could not derive layout name for merge.");
                return ConversionExitCode.InvalidArguments;
            }

            var mergedDoc = LayoutConverter.Core.Brlan.BrlanMergeHelper.MergeSequential(layoutName, inputs);
            
            var fullOutPath = Path.GetFullPath(outFilePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullOutPath)!);
            
            using var output = File.Create(fullOutPath);
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(LayoutConverter.Core.Schema.Rlan.Document));
            serializer.Serialize(output, mergedDoc);

            log.WriteLine($"Merged {inputs.Count} animation(s) into {fullOutPath}");
            return ConversionExitCode.Success;
        }
        catch (Exception ex)
        {
            log.WriteLine($"Merge failed: {ex.Message}");
            return ConversionExitCode.UnexpectedFailure;
        }
    }
}
