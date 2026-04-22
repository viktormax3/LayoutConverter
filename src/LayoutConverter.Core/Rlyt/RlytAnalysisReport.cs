
namespace LayoutConverter.Core.Rlyt;

public sealed record RlytAnalysisReport(
    string SourcePath,
    string Version,
    int TotalPaneCount,
    int PicturePaneCount,
    int NullPaneCount,
    int MaterialCount,
    int TextureMapCount,
    int GroupCount,
    IReadOnlyList<string> PaneNames,
    IReadOnlyList<string> MaterialNames);
