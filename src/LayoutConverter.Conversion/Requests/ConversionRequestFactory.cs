using LayoutConverter.Conversion.Infrastructure;
using LayoutConverter.Conversion.Options;

namespace LayoutConverter.Conversion.Requests;

public static class ConversionRequestFactory
{
    public static ConversionRequest? Create(string inputPath, ConverterOptions options)
    {
        ArgumentNullException.ThrowIfNull(inputPath);
        ArgumentNullException.ThrowIfNull(options);

        var fileType = ConverterPathHelper.DetectFileType(inputPath);
        if (fileType == ConverterFileType.Unknown)
        {
            return null;
        }

        var outputDirectory = string.IsNullOrWhiteSpace(options.OutputPath)
            ? Path.GetDirectoryName(Path.GetFullPath(inputPath))!
            : Path.GetFullPath(options.OutputPath);

        Directory.CreateDirectory(outputDirectory);

        return fileType switch
        {
            ConverterFileType.Layout => new LayoutConversionRequest(
                inputPath,
                outputDirectory,
                ConverterPathHelper.BuildBinaryOutputFilePath(
                    ConverterPathHelper.EnsureSectionOutputDirectory(outputDirectory, 0x626C7974),
                    inputPath),
                options.Execution,
                options.XmlLoad,
                options.Layout),

            ConverterFileType.Animation
                or ConverterFileType.PaletteAnimation
                or ConverterFileType.VisibilityAnimation
                or ConverterFileType.VertexColorAnimation
                or ConverterFileType.MaterialColorAnimation
                or ConverterFileType.TextureSrtAnimation
                or ConverterFileType.TexturePatternAnimation => new AnimationConversionRequest(
                    fileType,
                    inputPath,
                    ConverterPathHelper.BuildBinaryOutputFilePath(outputDirectory, inputPath),
                    options.Execution,
                    options.XmlLoad,
                    options.Animation),

            _ => null,
        };
    }
}
