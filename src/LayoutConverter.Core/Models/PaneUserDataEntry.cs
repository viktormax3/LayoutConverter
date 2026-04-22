
namespace LayoutConverter.Core.Models;

public sealed class PaneUserDataEntry
{
    public required string Name { get; init; }
    public required PaneUserDataValueType ValueType { get; init; }
    public required string RawValue { get; init; }
    public int NameOffset { get; set; }
    public int ValueOffset { get; set; }
    public int ElementCountOrLength { get; set; }
}
