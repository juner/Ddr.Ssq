using System;

namespace Ssq
{
    public class Chunk
    {
        public long Offset { get; set; }
        public ChunkHeader Header { get; set; }
        public int[] TimeOffsets { get; set; } = Array.Empty<int>();
        public int[] Tempo_TFPS_Config { get; set; } = Array.Empty<int>();
        public BiginFinishConfigType[] Bigin_Finish_Config { get; set; } = Array.Empty<BiginFinishConfigType>();
        public StepType[] StepData { get; set; } = Array.Empty<StepType>();
        public byte[] OtherData { get; set; } = Array.Empty<byte>();
    }
}
