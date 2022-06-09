using System.Runtime.Serialization;
using Ddr.Ssq.Internal;

namespace Ddr.Ssq;

/// <summary>
/// Begin/Finish Config Type
/// </summary>
public enum BiginFinishConfigType : short
{
    /// <summary>
    /// Start Game Mode
    /// </summary>
    [EnumMember(Value = "Start  GameMode  ?")]
    StartGameMode = 0x401,
    /// <summary>
    /// Start Music
    /// </summary>
    [EnumMember(Value = "Start  Music     ?")]
    StartMusic = 0x102,
    /// <summary>
    /// Delay Time Offset
    /// </summary>
    [EnumMember(Value = "Delay  TimeOffset?")]
    DelayTimeOffset = 0x202,
    /// <summary>
    /// Unkown(5th-)
    /// </summary>
    [EnumMember(Value = "Unknown(5th-)    ?")]
    Unknown_5th = 0x502,
    /// <summary>
    /// Finish Game Mode
    /// </summary>
    [EnumMember(Value = "Finish GameMode")]
    FinishGameMode = 0x302,
    /// <summary>
    /// Buffer Length
    /// </summary>
    [EnumMember(Value = "Buffer length    ?")]
    BufferLength = 0x402,
}
/// <summary>
/// Begin/Finish Config Type Extensions
/// </summary>
public static class BiginFinishConfigTypeExtensions
{
    /// <summary>
    /// get <see cref="EnumMemberAttribute.Value"/> of <paramref name="Type"/>
    /// </summary>
    /// <param name="Type"></param>
    /// <returns></returns>
    public static string ToMemberName(this BiginFinishConfigType Type)
        => Type.GetAttribute<EnumMemberAttribute>(ThrowNotFoundFiled: false)?.Value ?? "Unkown";
}
