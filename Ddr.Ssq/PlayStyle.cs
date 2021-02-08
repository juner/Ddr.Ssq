using System.Runtime.Serialization;
using Ddr.Ssq.Internal;

namespace Ddr.Ssq
{
    public enum PlayStyle : short
    {
        [EnumMember(Value = "Single")]
        Single = 0x14,
        [EnumMember(Value = "Solo")]
        Solo = 0x16,
        [EnumMember(Value = "Double")]
        Double = 0x18,
        [EnumMember(Value = "Battle")]
        Battle = 0x24,
    }
    public static class PlayStyleExtensions
    {
        public static string ToMemberName(this PlayStyle PlayStyle)
            => PlayStyle.GetAttribute<EnumMemberAttribute>(ThrowNotFoundFiled: false)?.Value ?? "Unkown";
    }
}
