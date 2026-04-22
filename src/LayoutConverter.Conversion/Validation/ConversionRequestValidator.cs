using LayoutConverter.Conversion.Infrastructure;
using LayoutConverter.Conversion.Options;
using LayoutConverter.Conversion.Requests;

namespace LayoutConverter.Conversion.Validation;

public sealed class ConversionRequestValidator
{
    public IReadOnlyList<string> GetWarnings(ConversionRequest request, ConverterOptions options)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(options);

        var warnings = new List<string>();
        warnings.AddRange(options.GetRouteWarnings(request.FileType));

        if (request is AnimationConversionRequest animationRequest)
        {
            warnings.AddRange(GetAnimationWarnings(animationRequest));
        }

        return warnings;
    }

    private static IEnumerable<string> GetAnimationWarnings(AnimationConversionRequest request)
    {
        if (!request.Animation.IncludeTagInfo && !request.Animation.SplitOutputsByTag)
        {
            yield return "--no-taginfo only has an effect when animation output is split by tag with -g.";
        }

    }
}
