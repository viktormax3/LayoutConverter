using LayoutConverter.Core.Schema.Rlan;

namespace LayoutConverter.Core.Brlan;

public static class BrlanMergeHelper
{
    public static Document MergeSequential(string layoutName, IReadOnlyList<string> splitBrlanPaths)
        => Merge(layoutName, splitBrlanPaths, sequentialFrames: true);

    public static Document Merge(string layoutName, IReadOnlyList<string> splitBrlanPaths, bool sequentialFrames)
    {
        var masterDoc = new Document
        {
            head = new Head
            {
                create = new HeadCreate { user = "MergeHelper", date = DateTime.Now },
                generator = new HeadGenerator { name = "LayoutConverter", version = "1.0.0" },
            },
            body = new DocumentBody(),
        };

        var masterTags = new List<AnimTag>();
        var mergedAnimations = new Dictionary<AnimationType, MergedAnimation>();

        int currentOffset = 0;
        int masterEndFrame = 0;

        var sortedPaths = sequentialFrames
            ? SortSplitPaths(layoutName, splitBrlanPaths).ToArray()
            : splitBrlanPaths.ToArray();
        for (int pathIndex = 0; pathIndex < sortedPaths.Length; pathIndex++)
        {
            var path = sortedPaths[pathIndex];
            string tagName = ExtractTagName(layoutName, path);
            var subDoc = BrlanBinaryReader.ReadDocument(path);
            var subRlans = subDoc.body?.rlan ?? Array.Empty<RLAN>();
            if (subRlans.Length == 0)
            {
                continue;
            }

            int pieceStartFrame = sequentialFrames ? 0 : subRlans.Min(static rlan => rlan.startFrame);
            int pieceEndFrame = subRlans.Max(static rlan => rlan.endFrame);
            int pieceDuration = pieceEndFrame - pieceStartFrame;
            int frameOffset = sequentialFrames ? currentOffset : 0;
            int? trimAfterFrame = sequentialFrames && pathIndex < sortedPaths.Length - 1 ? pieceDuration : null;

            masterTags.Add(new AnimTag
            {
                name = tagName,
                fileName = tagName,
                startFrame = frameOffset + pieceStartFrame,
                endFrame = frameOffset + pieceEndFrame,
                animLoop = subRlans.Any(static rlan => rlan.animLoop == AnimLoopType.Loop)
                    ? AnimLoopType.Loop
                    : AnimLoopType.OneTime,
                outputPaneSRT = HasAnimationType(subRlans, AnimationType.PainSRT),
                outputVisibility = HasAnimationType(subRlans, AnimationType.Visibility),
                outputVertexColor = HasAnimationType(subRlans, AnimationType.VertexColor),
                outputMaterialColor = HasAnimationType(subRlans, AnimationType.MaterialColor),
                outputTextureSRT = HasAnimationType(subRlans, AnimationType.TextureSRT),
                outputTexturePattern = HasAnimationType(subRlans, AnimationType.TexturePattern),
                outputIndTextureSRT = HasAnimationType(subRlans, AnimationType.IndTextureSRT),
            });

            foreach (var subRlan in subRlans)
            {
                if (!mergedAnimations.TryGetValue(subRlan.animType, out var mergedAnimation))
                {
                    mergedAnimation = new MergedAnimation(subRlan.animType);
                    mergedAnimations.Add(subRlan.animType, mergedAnimation);
                }

                foreach (var content in subRlan.animContent ?? Array.Empty<AnimContent>())
                {
                    string contentName = content.name ?? string.Empty;
                    if (!mergedAnimation.Contents.TryGetValue(contentName, out var masterContent))
                    {
                        masterContent = new AnimContent { name = contentName, Items = Array.Empty<AnimTarget>() };
                        mergedAnimation.Contents.Add(contentName, masterContent);
                    }

                    var mergedTargets = new List<AnimTarget>(masterContent.Items ?? Array.Empty<AnimTarget>());
                    var occurrenceByTarget = new Dictionary<(AnimTargetType Target, byte Id), int>();
                    foreach (var target in content.Items ?? Array.Empty<AnimTarget>())
                    {
                        var shiftedTarget = ShiftTargetKeys(target, frameOffset, trimAfterFrame);
                        var targetKey = (shiftedTarget.target, shiftedTarget.id);
                        occurrenceByTarget.TryGetValue(targetKey, out int occurrence);
                        occurrenceByTarget[targetKey] = occurrence + 1;

                        var existingTarget = mergedTargets
                            .Where(t => t.target == shiftedTarget.target && t.id == shiftedTarget.id)
                            .Skip(occurrence)
                            .FirstOrDefault();

                        if (existingTarget != null)
                        {
                            existingTarget.key = MergeKeyframes(
                                existingTarget.key ?? Array.Empty<Hermite>(),
                                shiftedTarget.key ?? Array.Empty<Hermite>(),
                                sequentialFrames);
                        }
                        else
                        {
                            mergedTargets.Add(shiftedTarget);
                        }
                    }

                    masterContent.Items = mergedTargets.ToArray();
                }
            }

            masterEndFrame = Math.Max(masterEndFrame, frameOffset + pieceEndFrame);
            if (sequentialFrames)
            {
                currentOffset += pieceDuration;
            }
        }

        masterDoc.body.animTag = masterTags.ToArray();
        masterDoc.body.rlan = mergedAnimations.Values
            .OrderBy(static animation => GetAnimationTypeOrder(animation.Type))
            .Select(animation => new RLAN
            {
                animType = animation.Type,
                startFrame = 0,
                endFrame = masterEndFrame,
                convertStartFrame = 0,
                convertEndFrame = masterEndFrame,
                animContent = animation.Contents.Values.ToArray(),
            })
            .ToArray();

        return masterDoc;
    }

    private static IEnumerable<string> SortSplitPaths(string layoutName, IReadOnlyList<string> splitBrlanPaths)
        => splitBrlanPaths
            .Select((path, index) => new
            {
                Path = path,
                Index = index,
                TagName = ExtractTagName(layoutName, path),
            })
            .OrderBy(static item => GetTagOrder(item.TagName))
            .ThenBy(static item => item.TagName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(static item => item.Index)
            .Select(static item => item.Path);

    private static string ExtractTagName(string layoutName, string path)
    {
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(path);
        return fileNameWithoutExt.StartsWith(layoutName + "_", StringComparison.OrdinalIgnoreCase)
            ? fileNameWithoutExt.Substring(layoutName.Length + 1)
            : fileNameWithoutExt;
    }

    private static int GetTagOrder(string tagName)
    {
        if (tagName.Equals("Start", StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        if (tagName.Equals("Loop", StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        if (tagName.Equals("End", StringComparison.OrdinalIgnoreCase))
        {
            return 2;
        }

        return 10;
    }

    private static int GetAnimationTypeOrder(AnimationType type)
        => type switch
        {
            AnimationType.PainSRT => 0,
            AnimationType.VertexColor => 1,
            AnimationType.MaterialColor => 2,
            AnimationType.TextureSRT => 3,
            AnimationType.TexturePattern => 4,
            AnimationType.IndTextureSRT => 5,
            AnimationType.Visibility => 6,
            _ => 100,
        };

    private static bool HasAnimationType(IEnumerable<RLAN> rlans, AnimationType type)
        => rlans.Any(rlan => rlan.animType == type);

    private static AnimTarget ShiftTargetKeys(AnimTarget target, int offset, int? trimAfterFrame)
    {
        var sourceKeys = target.key ?? Array.Empty<Hermite>();
        if (trimAfterFrame is int maxFrame)
        {
            sourceKeys = sourceKeys
                .Where(key => key.frame <= maxFrame)
                .ToArray();
        }

        var newTarget = target.Duplicate(sourceKeys);

        if (offset != 0 && newTarget.key != null)
        {
            var newKeys = new Hermite[newTarget.key.Length];
            for (int i = 0; i < newTarget.key.Length; i++)
            {
                var key = newTarget.key[i];
                newKeys[i] = key.Duplicate(key.frame + offset);
            }

            newTarget.key = newKeys;
        }

        return newTarget;
    }

    private static Hermite[] MergeKeyframes(Hermite[] existing, Hermite[] incoming, bool dropDuplicateBoundaryKeys)
        => existing
            .Concat(dropDuplicateBoundaryKeys ? DropDuplicateBoundaryKey(existing, incoming) : incoming)
            .OrderBy(static key => key.frame)
            .ToArray();

    private static IEnumerable<Hermite> DropDuplicateBoundaryKey(Hermite[] existing, Hermite[] incoming)
    {
        if (existing.Length == 0 || incoming.Length == 0)
        {
            return incoming;
        }

        var lastExisting = existing[^1];
        var firstIncoming = incoming[0];
        if (SameBinaryKey(lastExisting, firstIncoming))
        {
            return incoming.Skip(1);
        }

        return incoming;
    }

    private static bool SameBinaryKey(Hermite left, Hermite right)
        => left.frame == right.frame
            && left.value == right.value
            && left.slope == right.slope;

    private sealed class MergedAnimation
    {
        public MergedAnimation(AnimationType type)
        {
            Type = type;
        }

        public AnimationType Type { get; }
        public Dictionary<string, AnimContent> Contents { get; } = new(StringComparer.Ordinal);
    }
}
