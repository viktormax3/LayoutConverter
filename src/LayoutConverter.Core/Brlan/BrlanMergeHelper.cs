using LayoutConverter.Core.Schema.Rlan;

namespace LayoutConverter.Core.Brlan;

public static class BrlanMergeHelper
{
    public static Document MergeSequential(string layoutName, IReadOnlyList<string> splitBrlanPaths)
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
        int maxPieceDuration = 0;

        var sortedPaths = SortSplitPaths(layoutName, splitBrlanPaths).ToArray();
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

            int pieceDuration = subRlans.Max(static rlan => rlan.endFrame - rlan.startFrame);
            int? trimAfterFrame = pathIndex < sortedPaths.Length - 1 ? pieceDuration : null;
            maxPieceDuration = Math.Max(maxPieceDuration, pieceDuration);

            masterTags.Add(new AnimTag
            {
                name = tagName,
                fileName = tagName,
                startFrame = currentOffset,
                endFrame = currentOffset + pieceDuration,
                animLoop = AnimLoopType.OneTime,
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
                        var shiftedTarget = ShiftTargetKeys(target, currentOffset, trimAfterFrame);
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
                                shiftedTarget.key ?? Array.Empty<Hermite>());
                        }
                        else
                        {
                            mergedTargets.Add(shiftedTarget);
                        }
                    }

                    masterContent.Items = mergedTargets.ToArray();
                }
            }

            currentOffset += pieceDuration;
        }

        masterDoc.body.animTag = masterTags.ToArray();
        masterDoc.body.rlan = mergedAnimations.Values
            .OrderBy(static animation => GetAnimationTypeOrder(animation.Type))
            .Select(animation => new RLAN
            {
                animType = animation.Type,
                startFrame = 0,
                endFrame = maxPieceDuration,
                convertStartFrame = 0,
                convertEndFrame = maxPieceDuration,
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

    private static Hermite[] MergeKeyframes(Hermite[] existing, Hermite[] incoming)
        => existing
            .Concat(incoming)
            .OrderBy(static key => key.frame)
            .ToArray();

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
