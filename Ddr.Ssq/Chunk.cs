using System;

namespace Ddr.Ssq
{
    public class Chunk
    {
        public long Offset { get; set; }
        public ChunkHeader Header { get; set; }
        public IBody Body { get; set; } = default!;
    }
}
