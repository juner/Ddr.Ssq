using System;

namespace Ddr.Ssq
{
    public class Chunk
    {
        public long Offset { get; set; }
        public ChunkHeader Header { get; set; }
        public IBody Body { get; set; } = default!;
    }
    public static class ChunkExtensions
    {
        public static void Deconstruct(this Chunk Chunk, out long Offset, out ChunkHeader Header, out IBody Body)
            => (Offset, Header, Body) = (Chunk.Offset, Chunk.Header, Chunk.Body);
        public static void Deconstruct(this Chunk Chunk, out ChunkHeader Header, out IBody Body)
            => (Header, Body) = (Chunk.Header, Chunk.Body);
    }
}
