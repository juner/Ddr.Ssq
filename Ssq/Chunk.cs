using System;

namespace Ssq
{
    public class Chunk
    {
        public int Offset { get; set; }
        public ChunkHeader Header { get; set; }
        public uint[] TimeOffsets { get; set; } = Array.Empty<uint>();
        public int[] Tempo_TFPS_Config { get; set; } = Array.Empty<int>();
        public short[] Bigin_Finish_Config { get; set; } = Array.Empty<short>();
        public byte[] StepData { get; set; } = Array.Empty<byte>();
        public byte[] OtherData { get; set; } = Array.Empty<byte>();
    }
}
