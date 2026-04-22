using LayoutConverter.Conversion.Infrastructure;
using LayoutConverter.Core.Brlyt;

namespace LayoutConverter.Conversion.Options;

public sealed class ConverterOptions
{
    public required IReadOnlyList<string> InputPaths { get; init; }
    public string OutputPath { get; init; } = string.Empty;
    public bool ShowHelp { get; init; }
    public required ExecutionOptions Execution { get; init; }
    public required XmlLoadOptions XmlLoad { get; init; }
    public required LayoutRouteOptions Layout { get; init; }
    public required AnimationRouteOptions Animation { get; init; }

    public static ConverterOptions Parse(string[] args)
    {
        var inputs = new List<string>();
        string output = string.Empty;
        bool showHelp = false;
        bool update = false;
        bool splitByTag = false;
        bool includeTagInfo = true;
        bool omitSameKey = false;
        bool omitSameKeyAll = false;
        bool bakeInfinity = false;
        bool skipVersionCheck = false;
        bool exportReferencedTexturesOnly = false;
        bool validateXsd = false;
        bool suppressCvtrChar = false;
        string? logFile = null;
        LayoutFlavor flavor = LayoutFlavor.Default;

        for (int index = 0; index < args.Length; index++)
        {
            string arg = args[index];
            if (!arg.StartsWith('-') && !arg.StartsWith('/'))
            {
                inputs.Add(arg);
                continue;
            }

            string normalized = arg.TrimStart('-', '/');
            switch (normalized.ToLowerInvariant())
            {
                case "h":
                case "help":
                    showHelp = true;
                    break;
                case "u":
                    update = true;
                    break;
                case "g":
                    splitByTag = true;
                    break;
                case "i":
                case "no-convert-cvtrchar":
                    suppressCvtrChar = true;
                    break;
                case "no-taginfo":
                    includeTagInfo = false;
                    break;
                case "omit-samekey":
                    omitSameKey = true;
                    break;
                case "omit-samekey-all":
                    omitSameKeyAll = true;
                    break;
                case "bake-infinity":
                    bakeInfinity = true;
                    break;
                case "no-check-version":
                    skipVersionCheck = true;
                    break;
                case "cvtr-ref-tex-only":
                    exportReferencedTexturesOnly = true;
                    break;
                case "xsd-validate":
                    validateXsd = true;
                    break;
                case "banner":
                    flavor = LayoutFlavor.Banner;
                    break;
                case "logfile":
                    if (index + 1 >= args.Length)
                    {
                        throw new ArgumentException("Missing value after --logfile.");
                    }

                    logFile = args[++index];
                    break;
                case "o":
                case "out":
                case "output":
                    if (index + 1 >= args.Length)
                    {
                        throw new ArgumentException("Missing value after output option.");
                    }

                    output = args[++index];
                    break;
                default:
                    throw new ArgumentException($"Unknown option: {arg}");
            }
        }

        if (!showHelp && string.IsNullOrWhiteSpace(output))
        {
            if (inputs.Count < 2)
            {
                throw new ArgumentException("Expected SOURCE... DESTDIR when --output is not specified.");
            }

            output = inputs[^1];
            inputs.RemoveAt(inputs.Count - 1);
        }

        return new ConverterOptions
        {
            InputPaths = inputs,
            OutputPath = output,
            ShowHelp = showHelp,
            Execution = new ExecutionOptions
            {
                OnlyUpdateChangedFiles = update,
                LogFilePath = logFile,
            },
            XmlLoad = new XmlLoadOptions
            {
                SkipVersionCheck = skipVersionCheck,
                EnableXsdValidation = validateXsd,
                SuppressCvtrCharConversion = suppressCvtrChar,
            },
            Layout = new LayoutRouteOptions
            {
                Flavor = flavor,
                ExportReferencedTexturesOnly = exportReferencedTexturesOnly,
            },
            Animation = new AnimationRouteOptions
            {
                SplitOutputsByTag = splitByTag,
                IncludeTagInfo = includeTagInfo,
                OmitSameKeyAfterFirstTag = omitSameKey,
                OmitSameKeyForAllTags = omitSameKeyAll,
                BakeInfinityAreaKey = bakeInfinity,
                ExportReferencedTexturesOnly = exportReferencedTexturesOnly,
                UseBannerVersion = flavor == LayoutFlavor.Banner,
            },
        };
    }

    public IEnumerable<string> GetRouteWarnings(ConverterFileType fileType)
    {
        yield break;
    }
}

public sealed class ExecutionOptions
{
    public bool OnlyUpdateChangedFiles { get; init; }
    public string? LogFilePath { get; init; }
}

public sealed class XmlLoadOptions
{
    public bool SkipVersionCheck { get; init; }
    public bool EnableXsdValidation { get; init; }
    public bool SuppressCvtrCharConversion { get; init; }
}

public sealed class LayoutRouteOptions
{
    public LayoutFlavor Flavor { get; init; } = LayoutFlavor.Default;
    public bool ExportReferencedTexturesOnly { get; init; }
}

public sealed class AnimationRouteOptions
{
    public bool SplitOutputsByTag { get; init; }
    public bool IncludeTagInfo { get; init; } = true;
    public bool OmitSameKeyAfterFirstTag { get; init; }
    public bool OmitSameKeyForAllTags { get; init; }
    public bool BakeInfinityAreaKey { get; init; }
    public bool ExportReferencedTexturesOnly { get; init; }
    public bool UseBannerVersion { get; init; }

    public bool HasAnyExplicitSetting()
    {
        return SplitOutputsByTag
            || !IncludeTagInfo
            || OmitSameKeyAfterFirstTag
            || OmitSameKeyForAllTags
            || BakeInfinityAreaKey
            || UseBannerVersion;
    }
}
