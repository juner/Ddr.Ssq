using System.Runtime.Serialization;
using Ddr.Ssq.Internal;

namespace Ddr.Ssq;

/// <summary>
/// Play Difficulty
/// </summary>
public enum PlayDifficulty : byte
{
    /// <summary>
    /// Basic
    /// </summary>
    [EnumMember(Value = "Basic")]
    Basic = 0x01,
    /// <summary>
    /// Standard
    /// </summary>
    [EnumMember(Value = "Standard")]
    Standard = 0x02,
    /// <summary>
    /// Heavy
    /// </summary>
    [EnumMember(Value = "Heavy")]
    Heavy = 0x03,
    /// <summary>
    /// Beginner
    /// </summary>
    [EnumMember(Value = "Beginner")]
    Beginner = 0x04,
    /// <summary>
    /// Challenge
    /// </summary>
    [EnumMember(Value = "Challenge")]
    Challenge = 0x06,
    /// <summary>
    /// Battle
    /// </summary>
    [EnumMember(Value = "Battle")]
    Battle = 0x10,
}
/// <summary>
/// Play Difficulty Extensions
/// </summary>
public static class PlayDifficultyExtensions
{
    /// <summary>
    /// get <see cref="EnumMemberAttribute"/> of <see cref="PlayDifficulty"/>
    /// </summary>
    /// <param name="PlayDifficulty"></param>
    /// <returns></returns>
    public static string ToMemberName(this PlayDifficulty PlayDifficulty)
        => PlayDifficulty.GetAttribute<EnumMemberAttribute>(ThrowNotFoundFiled: false)?.Value ?? "Unkown";
}
