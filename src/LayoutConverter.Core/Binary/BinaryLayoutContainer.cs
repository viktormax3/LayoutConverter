namespace LayoutConverter.Core.Binary;

public sealed record BinaryLayoutContainer(
    string Magic,
    ushort ByteOrderMark,
    ushort Version,
    uint FileSize,
    ushort HeaderSize,
    ushort SectionCount,
    IReadOnlyList<BinaryLayoutSection> Sections);

public sealed record BinaryLayoutSection(
    string Magic,
    uint Size,
    long Offset,
    long PayloadOffset,
    long PayloadSize);
