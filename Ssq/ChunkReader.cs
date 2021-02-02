using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Ssq
{
    public class ChunkReader : IDisposable
    {
        Stream Stream;
        readonly bool LeaveOpen;
        public ChunkReader()
        public ChunkReader(Stream Stream, bool LeaveOpen)
            => this.Stream = Stream;
        public IEnumerable<Chunk> ReadToEnd()
        {
            var Length = Stream.Length;
            while (Stream.Position < Length)
            {

            }
        }
        public Chunk Read()
        {
            var Chunk = new Chunk();
            ReadHeader(Chunk);
        }
        public ChunkHeader ReadHeader()
        {
            var headerSize = Marshal.SizeOf<ChunkHeader>();
            var buffer = ArrayPool<byte>.Shared.Rent(headerSize);
            var span = buffer.AsSpan();
            var readed = Stream.Read(span);
            if (readed != headerSize)
                throw new InvalidOperationException();
            return MemoryMarshal.Read<ChunkHeader>(span);
        }
        public void ReadHeader(Chunk Chunk)
        {
            var Offset = Stream.Position;
            var Header = ReadHeader();
            Chunk.Offset = Offset;
            Chunk.Header = Header;
        }
        public void ReadBody(Chunk Chunk)
        {
            if (Chunk.Header is { Type: ChunkType.EndOfFile } or { Length: 0 })
                return;
        }
        public void Dispose()
        {
            if (!LeaveOpen)
                Stream.Dispose();
        }
    }
}
