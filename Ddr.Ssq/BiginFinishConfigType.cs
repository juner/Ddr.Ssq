using System.Runtime.Serialization;
using Ddr.Ssq.Internal;

namespace Ddr.Ssq
{
    public enum BiginFinishConfigType : short
    {
        [EnumMember(Value = "Start  GameMode  ?")]
        StartGameMode = 0x401,
        [EnumMember(Value = "Start  Music     ?")]
        StartMusic = 0x102,
        [EnumMember(Value = "Delay  TimeOffset?")]
        DelayTimeOffset = 0x202,
        [EnumMember(Value = "Unknown(5th-)    ?")]
        Unknown_5th = 0x502,
        [EnumMember(Value = "Finish GameMode")]
        FinishGameMode = 0x302,
        [EnumMember(Value = "Buffer length    ?")]
        BufferLength = 0x402,
    }
    public static class BiginFinishConfigTypeExtensions
    {
        public static string ToMemberName(this BiginFinishConfigType Type)
            => Type.GetAttribute<EnumMemberAttribute>(ThrowNotFoundFiled: false)?.Value ?? "Unkown";
    }
}
