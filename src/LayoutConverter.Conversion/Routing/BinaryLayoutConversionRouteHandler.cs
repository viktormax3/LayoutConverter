using System.Xml.Serialization;
using LayoutConverter.Conversion.Infrastructure;
using LayoutConverter.Conversion.Options;
using LayoutConverter.Conversion.Requests;
using LayoutConverter.Conversion.Textures;
using LayoutConverter.Core.Brlyt;
using LayoutConverter.Core.Schema.Rlyt;

namespace LayoutConverter.Conversion.Routing;

public sealed class BinaryLayoutConversionRouteHandler : IConversionRouteHandler
{
    private static readonly XmlSerializer Serializer = new(typeof(Document));

    public bool CanHandle(ConversionRequest request)
        => request is BinaryLayoutConversionRequest;

    public ConversionExitCode Execute(ConversionRequest request, TextWriter log)
    {
        var layoutRequest = (BinaryLayoutConversionRequest)request;
        try
        {
            var document = BrlytBinaryReader.ReadDocument(layoutRequest.SourcePath);
            ExportCompanionTextures(layoutRequest.SourcePath, layoutRequest.DestinationPath, document, log);
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(layoutRequest.DestinationPath))!);
            using var output = File.Create(layoutRequest.DestinationPath);
            Serializer.Serialize(output, document);
            log.WriteLine($"Wrote {layoutRequest.DestinationPath}");
            return ConversionExitCode.Success;
        }
        catch (Exception ex) when (ex is InvalidDataException or EndOfStreamException or IOException)
        {
            log.WriteLine(ex.Message);
            return ConversionExitCode.InputAccessFailure;
        }
    }

    private static void ExportCompanionTextures(string sourcePath, string destinationPath, Document document, TextWriter log)
    {
        TextureFile[] textures = document.body?.rlyt?.textureFile ?? Array.Empty<TextureFile>();
        if (textures.Length == 0)
        {
            return;
        }

        string layoutDirectory = Path.GetDirectoryName(Path.GetFullPath(destinationPath))!;
        string outputRootDirectory = Path.GetDirectoryName(layoutDirectory)!;
        string textureOutputDirectory = ConverterPathHelper.EnsureNativeSectionOutputDirectory(outputRootDirectory, "texture");

        foreach (TextureFile texture in textures)
        {
            string sourceTexturePath = ResolveCompanionTexturePath(sourcePath, texture.imagePath);
            if (!File.Exists(sourceTexturePath))
            {
                log.WriteLine($"Warning: Companion texture file not found, keeping original reference: {sourceTexturePath}");
                continue;
            }

            if (!TplTextureInfoReader.TryReadInfo(sourceTexturePath, out TplTextureInfo info))
            {
                log.WriteLine($"Warning: Could not inspect TPL texture format, keeping original reference: {sourceTexturePath}");
                continue;
            }

            texture.format = info.Format;
            if (!info.CanDecodeToTga)
            {
                log.WriteLine($"Warning: TPL reverse decode is not implemented for {info.Format}, keeping original reference: {sourceTexturePath}");
                continue;
            }

            string tgaFileName = Path.ChangeExtension(Path.GetFileName(sourceTexturePath), ".tga");
            string destinationTexturePath = Path.Combine(textureOutputDirectory, tgaFileName);
            try
            {
                TplToTgaConverter.Convert(sourceTexturePath, destinationTexturePath);
                texture.imagePath = $@"..\texture\{tgaFileName}";
            }
            catch (Exception ex) when (ex is InvalidDataException or NotSupportedException or IOException)
            {
                log.WriteLine($"Warning: Could not decode companion TPL to TGA, keeping original reference: {sourceTexturePath} ({ex.Message})");
            }
        }
    }

    private static string ResolveCompanionTexturePath(string layoutSourcePath, string originalTexturePath)
    {
        string fullLayoutSourcePath = Path.GetFullPath(layoutSourcePath);
        string sourceDirectory = Path.GetDirectoryName(fullLayoutSourcePath)!;
        string textureFileName = Path.ChangeExtension(Path.GetFileName(originalTexturePath), ".tpl");

        string[] candidates =
        [
            Path.Combine(sourceDirectory, textureFileName),
            Path.Combine(sourceDirectory, "timg", textureFileName),
            Path.Combine(Path.GetDirectoryName(sourceDirectory) ?? sourceDirectory, "timg", textureFileName),
        ];

        foreach (string candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return candidates[0];
    }
}
