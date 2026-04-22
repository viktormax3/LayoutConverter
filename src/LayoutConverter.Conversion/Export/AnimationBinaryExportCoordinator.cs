using LayoutConverter.Conversion.Infrastructure;
using LayoutConverter.Conversion.Options;
using LayoutConverter.Conversion.Requests;
using LayoutConverter.Core.Brlan;
using LayoutConverter.Core.Brlyt;
using LayoutConverter.Core.Rlan;
using LayoutConverter.Core.Rlyt;
using LayoutConverter.Core.Schema.Rlan;

namespace LayoutConverter.Conversion.Export;

public sealed class AnimationBinaryExportCoordinator
{
    private readonly RlanDocumentLoader _loader = new();
    private readonly RlytDocumentLoader _layoutLoader = new();
    private readonly LayoutResourceExportCoordinator _resourceExporter = new();

    public IReadOnlyList<string> ExportAnimationBinary(AnimationConversionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var document = _loader.Load(request.SourcePath, request.XmlLoad.SkipVersionCheck);
        var outputRootPath = GetOutputRootPath(request);
        var outputDirectory = ConverterPathHelper.EnsureSectionOutputDirectory(outputRootPath, 0x616E696D);
        var outputs = new List<string>();
        var texturePatternResources = new SortedDictionary<string, RefRes>(StringComparer.InvariantCultureIgnoreCase);
        var animShare = BuildAnimShare(document, request.Animation);

        var tags = BuildTags(document, request.Animation.SplitOutputsByTag);
        for (int tagIndex = 0; tagIndex < tags.Count; tagIndex++)
        {
            var tag = tags[tagIndex];
            var contents = BuildContents(document, tag, tagIndex, request.Animation);
            if (contents.Count == 0)
            {
                continue;
            }

            var refResources = BuildRefResources(contents);
            foreach (var resource in refResources)
            {
                texturePatternResources.TryAdd(resource.name, resource);
            }

            var outputPath = Path.Combine(
                outputDirectory,
                BuildOutputFileName(request.SourcePath, tag));

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            using var output = File.Create(outputPath);
            var writer = new BrlanBinaryWriter(output);
            writer.Write(
                document,
                tag,
                contents,
                refResources,
                new BrlanBinaryWriteOptions
                {
                    TagIndex = tagIndex,
                    IncludeTagInfo = request.Animation.SplitOutputsByTag
                        && request.Animation.IncludeTagInfo
                        && !request.Animation.UseBannerVersion,
                    AnimShare = animShare,
                    UseBannerVersion = request.Animation.UseBannerVersion,
                });
            outputs.Add(outputPath);
        }

        if (request.Animation.ExportReferencedTexturesOnly && texturePatternResources.Count > 0)
        {
            ExportTexturePatternResources(request, outputRootPath, texturePatternResources.Values);
        }

        return outputs;
    }

    private void ExportTexturePatternResources(
        AnimationConversionRequest request,
        string outputRootPath,
        IEnumerable<RefRes> refResources)
    {
        var companionLayoutPath = Path.ChangeExtension(request.SourcePath, ".rlyt");
        if (!File.Exists(companionLayoutPath))
        {
            throw new FileNotFoundException(
                $"Animation texture export requires a companion RLYT next to the RLAN: {companionLayoutPath}",
                companionLayoutPath);
        }

        var layoutDocument = _layoutLoader.Load(
            companionLayoutPath,
            request.XmlLoad.SkipVersionCheck,
            request.XmlLoad.EnableXsdValidation);
        var layoutContext = BrlytDocumentContext.FromDocument(layoutDocument);
        _resourceExporter.ExportTexturesByName(
            outputRootPath,
            companionLayoutPath,
            layoutContext,
            refResources.Select(static resource => resource.name));
    }

    private static string GetOutputRootPath(AnimationConversionRequest request)
    {
        var destinationDirectory = Path.GetDirectoryName(Path.GetFullPath(request.DestinationPath))
            ?? Directory.GetCurrentDirectory();
        return destinationDirectory;
    }

    private static string BuildOutputFileName(string sourcePath, AnimTag tag)
    {
        string sourceName = Path.GetFileNameWithoutExtension(sourcePath);
        string suffix = tag.GetFileName();
        return string.IsNullOrWhiteSpace(suffix)
            ? $"{sourceName}.brlan"
            : $"{sourceName}_{suffix}.brlan";
    }

    private static IReadOnlyList<AnimTag> BuildTags(Document document, bool splitOutputsByTag)
    {
        if (splitOutputsByTag)
        {
            var tags = document.body?.animTag?
                .Where(static tag => tag.outputEnabled)
                .ToArray();

            if (tags is { Length: > 0 })
            {
                foreach (var tag in tags)
                {
                    NormalizeTag(tag);
                }

                return tags;
            }
        }

        var firstRlan = document.body?.rlan?.FirstOrDefault()
            ?? throw new InvalidOperationException("The XML document does not contain RLAN animation data.");

        var synthetic = new AnimTag
        {
            startFrame = firstRlan.convertStartFrame,
            endFrame = firstRlan.convertEndFrame,
            animLoop = firstRlan.animLoop,
            group = Array.Empty<GroupRef>(),
        };
        NormalizeTag(synthetic);
        return new[] { synthetic };
    }

    private static AnimShare? BuildAnimShare(Document document, AnimationRouteOptions options)
    {
        var shares = document.body?.animShare;
        if (shares is not { Length: > 0 })
        {
            return null;
        }

        if (options.UseBannerVersion)
        {
            throw new InvalidOperationException("Animation sharing cannot be configured for Icon/Banner formats.");
        }

        return shares[0];
    }

    private static void NormalizeTag(AnimTag tag)
    {
        tag.group ??= Array.Empty<GroupRef>();
        tag.FrameSize = tag.animLoop == AnimLoopType.Loop
            ? tag.endFrame - tag.startFrame
            : tag.endFrame - tag.startFrame + 1;
    }

    private static IReadOnlyList<BrlanAnimationContent> BuildContents(
        Document document,
        AnimTag tag,
        int tagIndex,
        AnimationRouteOptions options)
    {
        var byNameAndKind = new Dictionary<(string Name, BrlanAnimationContentKind Kind), MutableContent>(StringComparerTuple.Ordinal);

        foreach (var rlan in document.body?.rlan ?? Array.Empty<RLAN>())
        {
            if (!ShouldOutputAnimationType(rlan.animType, tag))
            {
                continue;
            }

            foreach (var animContent in rlan.animContent ?? Array.Empty<AnimContent>())
            {
                var groupIndex = GetGroupIndex(rlan.animType, out var kind);
                var key = (animContent.name ?? string.Empty, kind);
                if (!byNameAndKind.TryGetValue(key, out var content))
                {
                    content = new MutableContent(key.Item1, kind);
                    byNameAndKind.Add(key, content);
                }

                var targets = new List<AnimTarget>();
                foreach (var target in animContent.Items ?? Array.Empty<AnimTarget>())
                {
                    var sourceTarget = options.BakeInfinityAreaKey
                        ? BakeInfinityAreaKeys(target, tag.startFrame, tag.endFrame)
                        : target;
                    var trimmed = TrimTargetToTag(sourceTarget, tag.startFrame, tag.endFrame);
                    if ((trimmed.key?.Length ?? 0) == 0)
                    {
                        continue;
                    }

                    bool omitSameKey = options.SplitOutputsByTag
                        && trimmed.key!.Length == 1
                        && (options.OmitSameKeyForAllTags || (options.OmitSameKeyAfterFirstTag && tagIndex > 0));
                    if (omitSameKey)
                    {
                        continue;
                    }

                    targets.Add(trimmed);
                }

                if (targets.Count > 0)
                {
                    content.Groups[groupIndex] = new BrlanAnimationGroup(rlan.animType, targets);
                }
            }
        }

        return byNameAndKind.Values
            .OrderBy(static content => content.Kind)
            .Select(static content => new BrlanAnimationContent(content.Name, content.Kind, content.Groups.Where(static group => group is not null).Cast<BrlanAnimationGroup>().ToArray()))
            .Where(static content => content.Groups.Count > 0)
            .ToArray();
    }

    private static bool ShouldOutputAnimationType(AnimationType type, AnimTag tag)
        => type switch
        {
            AnimationType.PainSRT => tag.outputPaneSRT,
            AnimationType.Visibility => tag.outputVisibility,
            AnimationType.VertexColor => tag.outputVertexColor,
            AnimationType.MaterialColor => tag.outputMaterialColor,
            AnimationType.TextureSRT => tag.outputTextureSRT,
            AnimationType.TexturePattern => tag.outputTexturePattern,
            AnimationType.IndTextureSRT => tag.outputIndTextureSRT,
            _ => false,
        };

    private static IReadOnlyList<RefRes> BuildRefResources(IReadOnlyList<BrlanAnimationContent> contents)
    {
        var resources = new SortedDictionary<string, RefRes>(StringComparer.InvariantCultureIgnoreCase);

        foreach (var content in contents)
        {
            foreach (var group in content.Groups)
            {
                if (group.AnimationType != AnimationType.TexturePattern)
                {
                    continue;
                }

                foreach (var target in group.Targets)
                {
                    foreach (var key in target.key ?? Array.Empty<Hermite>())
                    {
                        int refIndex = checked((int)key.value);
                        var targetResources = target.refRes ?? Array.Empty<RefRes>();
                        if ((uint)refIndex >= (uint)targetResources.Length)
                        {
                            throw new InvalidOperationException($"TexturePattern key references refRes index {refIndex}, but the target only has {targetResources.Length} resources.");
                        }

                        var resource = targetResources[refIndex];
                        resources.TryAdd(resource.name, resource);
                    }
                }
            }
        }

        return resources.Values.ToArray();
    }

    private static int GetGroupIndex(AnimationType type, out BrlanAnimationContentKind kind)
    {
        switch (type)
        {
            case AnimationType.PainSRT:
                kind = BrlanAnimationContentKind.Pane;
                return 0;
            case AnimationType.Visibility:
                kind = BrlanAnimationContentKind.Pane;
                return 1;
            case AnimationType.VertexColor:
                kind = BrlanAnimationContentKind.Pane;
                return 2;
            case AnimationType.MaterialColor:
                kind = BrlanAnimationContentKind.Material;
                return 0;
            case AnimationType.TextureSRT:
                kind = BrlanAnimationContentKind.Material;
                return 1;
            case AnimationType.TexturePattern:
                kind = BrlanAnimationContentKind.Material;
                return 2;
            case AnimationType.IndTextureSRT:
                kind = BrlanAnimationContentKind.Material;
                return 3;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private static AnimTarget TrimTargetToTag(AnimTarget target, int startFrame, int endFrame)
    {
        var keys = target.key ?? Array.Empty<Hermite>();
        if (keys.Length == 0)
        {
            return target.Duplicate(Array.Empty<Hermite>());
        }

        int first = 0;
        while (first < keys.Length && keys[first].frame <= startFrame)
        {
            first++;
        }

        if (first > 0)
        {
            first--;
        }

        int last = keys.Length - 1;
        while (last >= 0 && keys[last].frame >= endFrame)
        {
            last--;
        }

        if (last < keys.Length - 1)
        {
            last++;
        }

        int previous = last - 1;
        while (previous >= first)
        {
            bool canCollapseTrailingKey = keys[previous].frame >= startFrame || keys[last].frame > endFrame;
            if (!canCollapseTrailingKey || !IsSameStationaryValue(keys[last], keys[previous]))
            {
                break;
            }

            last = previous;
            previous--;
        }

        int originalFirst = first;
        int next = first + 1;
        while (next <= last && IsSameStationaryValue(keys[first], keys[next]))
        {
            first = next;
            next++;
        }

        if (originalFirst < first && first + 1 <= last && keys[first].frame == keys[first + 1].frame)
        {
            first--;
        }

        if (last < first)
        {
            return target.Duplicate(Array.Empty<Hermite>());
        }

        var selected = keys
            .Skip(first)
            .Take(last - first + 1)
            .ToArray();

        return target.Duplicate(selected);
    }

    private static AnimTarget BakeInfinityAreaKeys(AnimTarget target, int startFrame, int endFrame)
    {
        var keys = target.key ?? Array.Empty<Hermite>();
        if (keys.Length == 0)
        {
            return target.Duplicate(Array.Empty<Hermite>());
        }

        float firstFrame = keys[0].frame;
        float lastFrame = keys[^1].frame;
        float cycleLength = lastFrame - firstFrame;
        int preCycles = 0;
        int postCycles = 0;

        if (startFrame < firstFrame
            && target.preInfinityType == InfinityType.Cycle
            && cycleLength > 0f)
        {
            preCycles = (int)Math.Ceiling((firstFrame - startFrame) / cycleLength);
        }

        if (lastFrame < endFrame
            && target.postInfinityType == InfinityType.Cycle
            && cycleLength > 0f)
        {
            postCycles = (int)Math.Ceiling((endFrame - lastFrame) / cycleLength);
        }

        var baked = new List<Hermite>(keys.Length * (preCycles + 1 + postCycles));
        for (int cycle = -preCycles; cycle <= postCycles; cycle++)
        {
            AppendCycleKeys(baked, keys, cycle * cycleLength);
        }

        return target.Duplicate(baked.ToArray());
    }

    private static void AppendCycleKeys(List<Hermite> output, Hermite[] keys, float frameOffset)
    {
        if (keys.Length == 0)
        {
            return;
        }

        float firstFrame = keys[0].frame + frameOffset;
        for (int index = output.Count - 1; index >= 0; index--)
        {
            if (output[index].frame == firstFrame)
            {
                output[index] = output[index].Duplicate(output[index].frame - 0.001f);
                break;
            }
        }

        foreach (var key in keys)
        {
            output.Add(frameOffset == 0f ? key : key.Duplicate(key.frame + frameOffset));
        }
    }

    private static bool IsSameStationaryValue(Hermite left, Hermite right)
        => left.value == right.value
            && left.slope == 0f
            && right.slope == 0f;

    private sealed class MutableContent
    {
        public MutableContent(string name, BrlanAnimationContentKind kind)
        {
            Name = name;
            Kind = kind;
            Groups = new BrlanAnimationGroup?[kind == BrlanAnimationContentKind.Pane ? 3 : 4];
        }

        public string Name { get; }
        public BrlanAnimationContentKind Kind { get; }
        public BrlanAnimationGroup?[] Groups { get; }
    }

    private sealed class StringComparerTuple : IEqualityComparer<(string Name, BrlanAnimationContentKind Kind)>
    {
        public static readonly StringComparerTuple Ordinal = new();

        public bool Equals((string Name, BrlanAnimationContentKind Kind) x, (string Name, BrlanAnimationContentKind Kind) y)
            => x.Kind == y.Kind && string.Equals(x.Name, y.Name, StringComparison.Ordinal);

        public int GetHashCode((string Name, BrlanAnimationContentKind Kind) obj)
            => HashCode.Combine(StringComparer.Ordinal.GetHashCode(obj.Name), obj.Kind);
    }
}
