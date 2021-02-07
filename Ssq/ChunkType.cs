using System;
using System.Runtime.Serialization;
using Ssq.Internal;

namespace Ssq
{
    public enum ChunkType:short
    {
        [EnumMember(Value = "End of File.")]
        EndOfFile = 0,
        [EnumMember(Value = "Tempo/TfPS Config.")]
        Tempo_TFPS_Config = 1,
        [EnumMember(Value = "Begin/Finish Config.")]
        Bigin_Finish_Config = 2,
        [EnumMember(Value = "Step Data.")]
        StepData = 3,
    }
    public static class ChunktypeExtensions
    {
        public static string ToMemberName(this ChunkType Type)
            => Type.GetAttribute<EnumMemberAttribute>(ThrowNotFoundFiled: false)?.Value ?? "Unkown";
    }
}
