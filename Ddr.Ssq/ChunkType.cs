using System.Runtime.Serialization;
using Ddr.Ssq.Internal;

namespace Ddr.Ssq;

/// <summary>
/// Chunk Type
/// </summary>
public enum ChunkType : short
{
    /// <summary>
    /// End of File.
    /// </summary>
    [EnumMember(Value = "End of File.")]
    EndOfFile = 0,
    /// <summary>
    /// Tempo/TfPS Config.
    /// </summary>
    [EnumMember(Value = "Tempo/TfPS Config.")]
    TempoTFPSConfig = 1,
    /// <summary>
    /// Begin/Finish Config.
    /// </summary>
    [EnumMember(Value = "Begin/Finish Config.")]
    BiginFinishConfig = 2,
    /// <summary>
    /// Step Data.
    /// </summary>
    [EnumMember(Value = "Step Data.")]
    StepData = 3,
}
/// <summary>
/// Chunk Type Extensions
/// </summary>
public static class ChunkTypeExtensions
{
    /// <summary>
    /// get <see cref="EnumMemberAttribute"/> of <see cref="ChunkType"/>
    /// </summary>
    /// <param name="Type"></param>
    /// <returns></returns>
    public static string ToMemberName(this ChunkType Type)
        => Type.GetAttribute<EnumMemberAttribute>(ThrowNotFoundFiled: false)?.Value ?? "Unknown Data.";
}
