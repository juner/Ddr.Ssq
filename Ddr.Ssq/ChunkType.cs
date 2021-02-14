using System;
using System.Runtime.Serialization;
using Ddr.Ssq.Internal;

namespace Ddr.Ssq
{
    public enum ChunkType:short
    {
        [EnumMember(Value = "End of File.")]
        EndOfFile = 0,
        [EnumMember(Value = "Tempo/TfPS Config.")]
        TempoTFPSConfig = 1,
        [EnumMember(Value = "Begin/Finish Config.")]
        BiginFinishConfig = 2,
        [EnumMember(Value = "Step Data.")]
        StepData = 3,
    }
    public static class ChunktypeExtensions
    {
        public static string ToMemberName(this ChunkType Type)
            => Type.GetAttribute<EnumMemberAttribute>(ThrowNotFoundFiled: false)?.Value ?? "Unknown Data.";
    }
}
