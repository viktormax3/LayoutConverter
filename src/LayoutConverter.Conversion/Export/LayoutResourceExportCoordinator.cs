using LayoutConverter.Conversion.Infrastructure;
using LayoutConverter.Conversion.Requests;
using LayoutConverter.Conversion.Textures;
using LayoutConverter.Core.Brlyt;
using LayoutConverter.Core.Schema.Rlyt;

namespace LayoutConverter.Conversion.Export;

public sealed class LayoutResourceExportCoordinator
{
    public void ExportResources(LayoutConversionRequest request, BrlytDocumentContext context)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        ExportTextures(request, context);
        ExportFonts(request, context);
    }

    public void ExportTexturesByName(
        string outputRootPath,
        string layoutSourcePath,
        BrlytDocumentContext context,
        IEnumerable<string> textureNames)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputRootPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(layoutSourcePath);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(textureNames);

        var selectedNames = new HashSet<string>(textureNames, StringComparer.InvariantCultureIgnoreCase);
        if (selectedNames.Count == 0)
        {
            return;
        }

        var textures = context.AllTextures
            .Where(texture => selectedNames.Contains(texture.GetName()))
            .ToArray();

        if (textures.Length != selectedNames.Count)
        {
            var available = context.AllTextures
                .Select(static texture => texture.GetName())
                .ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            string missing = string.Join(", ", selectedNames.Where(name => !available.Contains(name)).Order(StringComparer.InvariantCultureIgnoreCase));
            throw new InvalidOperationException($"Animation references texture resource(s) not present in the companion RLYT: {missing}");
        }

        ExportTextures(outputRootPath, layoutSourcePath, textures);
    }

    private static void ExportTextures(LayoutConversionRequest request, BrlytDocumentContext context)
    {
        var textures = request.Layout.ExportReferencedTexturesOnly
            ? context.Textures
            : context.AllTextures;

        if (textures.Count == 0)
        {
            return;
        }

        ExportTextures(request.OutputRootPath, request.SourcePath, textures);
    }

    private static void ExportTextures(
        string outputRootPath,
        string layoutSourcePath,
        IReadOnlyList<TextureFile> textures)
    {
        if (textures.Count == 0)
        {
            return;
        }

        var textureOutputDirectory = ConverterPathHelper.EnsureSectionOutputDirectory(outputRootPath, 0x74696D67);

        foreach (var texture in textures)
        {
            if (string.IsNullOrWhiteSpace(texture.imagePath))
            {
                throw new InvalidOperationException("A textureFile entry is missing its imagePath attribute.");
            }

            var sourcePath = ResolveResourcePath(layoutSourcePath, texture.imagePath);
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException($"Texture source file not found: {sourcePath}", sourcePath);
            }

            var destinationPath = Path.Combine(textureOutputDirectory, texture.GetConvertedFileName());
            if (Path.GetExtension(sourcePath).Equals(".tpl", StringComparison.OrdinalIgnoreCase))
            {
                CopyResource(sourcePath, destinationPath);
                continue;
            }

            if (Path.GetExtension(sourcePath).Equals(".tga", StringComparison.OrdinalIgnoreCase))
            {
                TgaToTplConverter.Convert(sourcePath, destinationPath, texture.format);
                continue;
            }

            throw new NotSupportedException($"Unsupported texture source format '{Path.GetExtension(sourcePath)}'. Source: {sourcePath}");
        }
    }

    private static void ExportFonts(LayoutConversionRequest request, BrlytDocumentContext context)
    {
        if (context.Fonts.Count == 0)
        {
            return;
        }

        foreach (var font in context.Fonts)
        {
            if (string.IsNullOrWhiteSpace(font.path))
            {
                throw new InvalidOperationException("A fontFile entry is missing its path attribute.");
            }

            var sourcePath = ResolveResourcePath(request.SourcePath, font.path);
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException($"Font source file not found: {sourcePath}", sourcePath);
            }

            var sectionMagic = Path.GetExtension(sourcePath).Equals(".brfna", StringComparison.OrdinalIgnoreCase)
                ? 0x666E7461U
                : 0x666F6E74U;

            var fontOutputDirectory = ConverterPathHelper.EnsureSectionOutputDirectory(request.OutputRootPath, sectionMagic);
            var destinationPath = Path.Combine(fontOutputDirectory, font.GetFileName());
            CopyResource(sourcePath, destinationPath);
        }
    }

    private static string ResolveResourcePath(string layoutSourcePath, string resourcePath)
    {
        if (Path.IsPathFullyQualified(resourcePath))
        {
            return Path.GetFullPath(resourcePath);
        }

        var baseDirectory = Path.GetDirectoryName(Path.GetFullPath(layoutSourcePath))
            ?? Directory.GetCurrentDirectory();
        return Path.GetFullPath(Path.Combine(baseDirectory, resourcePath));
    }

    private static void CopyResource(string sourcePath, string destinationPath)
    {
        if (File.Exists(destinationPath))
        {
            var attributes = File.GetAttributes(destinationPath);
            File.SetAttributes(destinationPath, attributes & ~(FileAttributes.Hidden | FileAttributes.ReadOnly));
        }

        File.Copy(sourcePath, destinationPath, overwrite: true);
    }
}
