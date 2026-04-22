using System.Buffers.Binary;
using System.Text;

namespace LayoutConverter.Core.IO;

public sealed class BigEndianBinaryOutputWriter : IDisposable
{
    private readonly Stream _stream;
    private readonly bool _leaveOpen;

    public BigEndianBinaryOutputWriter(Stream stream, bool leaveOpen = false)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _leaveOpen = leaveOpen;
    }

    public Stream BaseStream => _stream;
    public long Position => _stream.Position;

    public void Write(byte value) => _stream.WriteByte(value);
    public void WriteByte(byte value) => _stream.WriteByte(value);
    public void Write(ReadOnlySpan<byte> bytes) => _stream.Write(bytes);

    public void WriteUInt16(ushort value)
    {
        Span<byte> buffer = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
        _stream.Write(buffer);
    }

    public void WriteInt16(short value)
    {
        Span<byte> buffer = stackalloc byte[2];
        BinaryPrimitives.WriteInt16BigEndian(buffer, value);
        _stream.Write(buffer);
    }

    public void WriteUInt32(uint value)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(buffer, value);
        _stream.Write(buffer);
    }

    public void WriteInt32(int value)
    {
        Span<byte> buffer = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(buffer, value);
        _stream.Write(buffer);
    }

    public void WriteSingle(float value)
    {
        WriteInt32(BitConverter.SingleToInt32Bits(value));
    }

    public void WriteFixedAscii(string text, int length, bool zeroTerminate = false)
    {
        var bytes = Encoding.ASCII.GetBytes(text);
        if (bytes.Length > length)
        {
            throw new ArgumentOutOfRangeException(nameof(text), $"ASCII string exceeds fixed length {length}.");
        }

        _stream.Write(bytes);

        var written = bytes.Length;
        if (zeroTerminate && written < length)
        {
            _stream.WriteByte(0);
            written++;
        }

        while (written < length)
        {
            _stream.WriteByte(0);
            written++;
        }
    }

    public long ReserveUInt16()
    {
        long offset = _stream.Position;
        WriteUInt16(0);
        return offset;
    }

    public long ReserveUInt32()
    {
        long offset = _stream.Position;
        WriteUInt32(0);
        return offset;
    }

    public void PatchUInt16(long absoluteOffset, ushort value)
    {
        long previous = _stream.Position;
        _stream.Position = absoluteOffset;
        WriteUInt16(value);
        _stream.Position = previous;
    }

    public void PatchUInt32(long absoluteOffset, uint value)
    {
        long previous = _stream.Position;
        _stream.Position = absoluteOffset;
        WriteUInt32(value);
        _stream.Position = previous;
    }

    public void Align(int alignment, byte padding = 0)
    {
        if (alignment <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(alignment));
        }

        while ((_stream.Position % alignment) != 0)
        {
            _stream.WriteByte(padding);
        }
    }

    public void Dispose()
    {
        if (!_leaveOpen)
        {
            _stream.Dispose();
        }
    }
}
