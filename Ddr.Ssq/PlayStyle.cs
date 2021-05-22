using System.Runtime.Serialization;
using Ddr.Ssq.Internal;

namespace Ddr.Ssq
{
    /// <summary>
    /// Play Style
    /// </summary>
    public enum PlayStyle : byte
    {
        /// <summary>
        /// Single
        /// </summary>
        [EnumMember(Value = "Single")]
        Single = 0x14,
        /// <summary>
        /// Solo
        /// </summary>
        [EnumMember(Value = "Solo")]
        Solo = 0x16,
        /// <summary>
        /// Double
        /// </summary>
        [EnumMember(Value = "Double")]
        Double = 0x18,
        /// <summary>
        /// Battle
        /// </summary>
        [EnumMember(Value = "Battle")]
        Battle = 0x24,
    }
    /// <summary>
    /// Play Style Extensions
    /// </summary>
    public static class PlayStyleExtensions
    {
        /// <summary>
        /// get <see cref="EnumMemberAttribute"/> of <see cref="PlayStyle"/>
        /// </summary>
        /// <param name="PlayStyle"></param>
        /// <returns></returns>
        public static string ToMemberName(this PlayStyle PlayStyle)
            => PlayStyle.GetAttribute<EnumMemberAttribute>(ThrowNotFoundFiled: false)?.Value ?? "Unkown";
    }
}
