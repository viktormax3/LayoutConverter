using System.Buffers.Binary;
using System.Text;

namespace LayoutConverter.Core.IO;

public sealed class BigEndianBinaryInputReader : IDisposable
{
    private readonly Stream _stream;
    private readonly bool _leaveOpen;

    public BigEndianBinaryInputReader(Stream stream, bool leaveOpen = false)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _leaveOpen = leaveOpen;
    }

    public Stream BaseStream => _stream;
    public long Position
    {
        get => _stream.Position;
        set => _stream.Position = value;
    }

    public long Length => _stream.Length;

    public byte ReadByte()
    {
        int value = _stream.ReadByte();
        if (value < 0)
        {
            throw new EndOfStreamException();
        }

        return (byte)value;
    }

    public ushort ReadUInt16()
    {
        Span<byte> buffer = stackalloc byte[2];
        ReadExactly(buffer);
        return BinaryPrimitives.ReadUInt16BigEndian(buffer);
    }

    public short ReadInt16()
    {
        Span<byte> buffer = stackalloc byte[2];
        ReadExactly(buffer);
        return BinaryPrimitives.ReadInt16BigEndian(buffer);
    }

    public uint ReadUInt32()
    {
        Span<byte> buffer = stackalloc byte[4];
        ReadExactly(buffer);
        return BinaryPrimitives.ReadUInt32BigEndian(buffer);
    }

    public int ReadInt32()
    {
        Span<byte> buffer = stackalloc byte[4];
        ReadExactly(buffer);
        return BinaryPrimitives.ReadInt32BigEndian(buffer);
    }

    public float ReadSingle()
        => BitConverter.Int32BitsToSingle(ReadInt32());

    public string ReadFixedAscii(int length)
    {
        var buffer = new byte[length];
        ReadExactly(buffer);
        int end = Array.IndexOf(buffer, (byte)0);
        if (end < 0)
        {
            end = buffer.Length;
        }

        return Encoding.ASCII.GetString(buffer, 0, end);
    }

    public byte[] ReadBytes(int count)
    {
        var buffer = new byte[count];
        ReadExactly(buffer);
        return buffer;
    }

    public void ReadExactly(Span<byte> buffer)
        => _stream.ReadExactly(buffer);

    public void Dispose()
    {
        if (!_leaveOpen)
        {
            _stream.Dispose();
        }
    }
}
