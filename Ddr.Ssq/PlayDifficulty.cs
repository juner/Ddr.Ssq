using System.Runtime.Serialization;
using Ddr.Ssq.Internal;

namespace Ddr.Ssq
{
    public enum PlayDifficulty : byte
    {
        [EnumMember(Value = "Basic")]
        Basic = 0x01,
        [EnumMember(Value = "Standard")]
        Standard = 0x02,
        [EnumMember(Value = "Heavy")]
        Heavy = 0x03,
        [EnumMember(Value = "Beginner")]
        Beginner = 0x04,
        [EnumMember(Value = "Challenge")]
        Challenge = 0x06,
        [EnumMember(Value = "Battle")]
        Battle = 0x10,
    }
    public static class PlayDifficultyExtensions
    {
        public static string ToMemberName(this PlayDifficulty PlayDifficulty)
            => PlayDifficulty.GetAttribute<EnumMemberAttribute>(ThrowNotFoundFiled: false)?.Value ?? "Unkown";
    }
}
