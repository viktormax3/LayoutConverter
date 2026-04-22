using LayoutConverter.Core.IO;

namespace LayoutConverter.Core.Brlyt;

internal sealed class BrlytSectionScope : IDisposable
{
    public const int DefaultAlignment = 4;

    private readonly BigEndianBinaryOutputWriter _writer;
    private readonly long _sectionStart;
    private readonly long _sectionSizePatchOffset;
    private readonly int _alignment;

    public BrlytSectionScope(BigEndianBinaryOutputWriter writer, string fourCc, int alignment = DefaultAlignment)
    {
        _writer = writer;
        _alignment = alignment;
        _sectionStart = writer.Position;
        writer.WriteFixedAscii(fourCc, 4);
        _sectionSizePatchOffset = writer.ReserveUInt32();
    }

    public uint ContentStartPositionRelativeToSection => checked((uint)(_writer.Position - _sectionStart));

    public void Dispose()
    {
        _writer.Align(_alignment);
        var sectionSize = checked((uint)(_writer.Position - _sectionStart));
        _writer.PatchUInt32(_sectionSizePatchOffset, sectionSize);
    }
}
