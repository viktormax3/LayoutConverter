
using System.Text.RegularExpressions;

namespace LayoutConverter.Conversion.Infrastructure;

public static class ConverterPathHelper
{
    private static readonly Regex ValidResourceNamePattern = new(
        @"[^0-9A-Za-z !#$%&'()+.=\[\]^_@`{}~-]",
        RegexOptions.Compiled);

    private static readonly Dictionary<string, ConverterFileType> FileTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".rlyt"] = ConverterFileType.Layout,
        [".rlan"] = ConverterFileType.Animation,
        [".rlpa"] = ConverterFileType.PaletteAnimation,
        [".rlvi"] = ConverterFileType.VisibilityAnimation,
        [".rlvc"] = ConverterFileType.VertexColorAnimation,
        [".rlmc"] = ConverterFileType.MaterialColorAnimation,
        [".rlts"] = ConverterFileType.TextureSrtAnimation,
        [".rltp"] = ConverterFileType.TexturePatternAnimation,
        [".tpl"] = ConverterFileType.Texture,
        [".brlyt"] = ConverterFileType.BinaryLayout,
        [".brlan"] = ConverterFileType.BinaryAnimation,
    };

    private static readonly Dictionary<uint, string> SectionDirectories = new()
    {
        [0x626C7974] = "blyt",
        [0x74696D67] = "timg",
        [0x666F6E74] = "font",
        [0x666E7461] = "fnta",
        [0x616E696D] = "anim",
    };

    public static ConverterFileType DetectFileType(string path)
    {
        var extension = Path.GetExtension(path);
        return extension is not null && FileTypes.TryGetValue(extension, out var fileType)
            ? fileType
            : ConverterFileType.Unknown;
    }

    public static string[] ResolveInputPaths(string inputArgument, out bool sourceWasDirectory)
    {
        ArgumentException.ThrowIfNullOrEmpty(inputArgument);

        if (Directory.Exists(inputArgument))
        {
            sourceWasDirectory = true;
            return Directory.GetFiles(inputArgument);
        }

        if (File.Exists(inputArgument))
        {
            sourceWasDirectory = false;
            return new[] { inputArgument };
        }

        throw new FileNotFoundException($"Input file or directory not found: {inputArgument}");
    }

    public static string BuildBinaryOutputFilePath(
        string outputDirectory,
        string sourceFilePath,
        string? outputPrefix = null,
        string? outputSuffix = null)
    {
        var prefixPart = string.IsNullOrWhiteSpace(outputPrefix) ? string.Empty : outputPrefix + "_";
        var suffixPart = string.IsNullOrWhiteSpace(outputSuffix) ? string.Empty : "_" + outputSuffix;
        var binaryExtension = Path.GetExtension(sourceFilePath).Insert(1, "b");

        return Path.Combine(
            outputDirectory,
            prefixPart + Path.GetFileNameWithoutExtension(sourceFilePath) + suffixPart + binaryExtension);
    }

    public static string BuildTextureOutputFilePath(string outputDirectory, string sourceFilePath)
        => Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(sourceFilePath) + ".tga");

    public static string BuildBinaryInspectionOutputFilePath(string outputDirectory, string sourceFilePath)
        => Path.Combine(outputDirectory, Path.GetFileName(sourceFilePath) + ".sections.txt");

    public static string BuildRlanOutputFilePath(string outputDirectory, string sourceFilePath)
        => Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(sourceFilePath) + ".rlan");

    public static string BuildRlytOutputFilePath(string outputDirectory, string sourceFilePath)
        => Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(sourceFilePath) + ".rlyt");

    public static bool IsValidResourceName(string value)
        => !ValidResourceNamePattern.IsMatch(value);

    public static string CreateFreshTempOutputDirectory(string executableName)
    {
        var tempDirectoryPath = Path.Combine(Path.GetTempPath(), executableName + "_temp");
        var directory = new DirectoryInfo(tempDirectoryPath);
        if (directory.Exists)
        {
            directory.Delete(recursive: true);
        }

        directory.Create();
        return directory.FullName;
    }

    public static string EnsureSectionOutputDirectory(string rootOutputDirectory, uint sectionMagic)
    {
        var sectionDirectoryName = SectionDirectories.TryGetValue(sectionMagic, out var name)
            ? name
            : sectionMagic.ToString("X8");

        var sectionDirectoryPath = Path.Combine(rootOutputDirectory, sectionDirectoryName);
        Directory.CreateDirectory(sectionDirectoryPath);
        return sectionDirectoryPath;
    }
}
