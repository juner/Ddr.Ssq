using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ddr.Ssq;

/// <summary>
/// Chunk Header
/// </summary>
[StructLayout(LayoutKind.Explicit, Pack = 1)]
[DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
public struct ChunkHeader
{
    /// <summary>
    /// Chunk Length (Header + Body)
    /// </summary>
    [FieldOffset(0)]
    public int Length;
    /// <summary>
    /// Chunk Length uint version.
    /// </summary>
    public uint LongLength { readonly get => unchecked((uint)Length); set => Length = unchecked((int)value); }
    /// <summary>
    /// Chunk Type
    /// </summary>
    [FieldOffset(4)]
    [MarshalAs(UnmanagedType.I2)]
    public ChunkType Type;
    /// <summary>
    /// Param
    /// </summary>
    [FieldOffset(6)]
    public short Param;
    /// <summary>
    /// Param by StepData Type.
    /// </summary>
    [FieldOffset(6)]
    public StepPlayType Play;
    /// <summary>
    /// Entry Count
    /// </summary>
    [FieldOffset(8)]
    public int Entry;
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="Length"></param>
    /// <param name="Type"></param>
    /// <param name="Param"></param>
    /// <param name="Entry"></param>
    public ChunkHeader(int Length = 0, ChunkType Type = ChunkType.EndOfFile, short Param = 0, int Entry = 0)
    {
        Play = default;
        (this.Length, this.Type, this.Param, this.Entry) = (Length, Type, Param, Entry);
    }
    /// <summary>
    /// create stepdata constructor
    /// </summary>
    /// <param name="Length"></param>
    /// <param name="Play"></param>
    /// <param name="Entry"></param>
    /// <returns></returns>
    public static ChunkHeader CreateStepData(int Length = 0, StepPlayType Play = default, int Entry = 0)
        => new(Length, ChunkType.StepData, (short)Play, Entry);
    readonly IEnumerable<string> GetInnerDisplay()
    {
        IEnumerable<string?> members = new[] {
                Length == 0 ? null : Length>0 ? $"{nameof(Length)}:{Length}" : $"{nameof(LongLength)}:{LongLength}",
                $"{nameof(Type)}:{Type.ToMemberName()}({Type:d})",
                $"{nameof(Param)}:0x{Param:X4}",
                $"{nameof(Entry)}:0x{Entry:X8}",
            };
        if (Type is ChunkType.StepData)
            members = members
                .Append($"{nameof(Play)}.{nameof(Play.Difficulty)}:{Play.Difficulty.ToMemberName()}(0x{(short)Play.Difficulty:X2})")
                .Append($"{nameof(Play)}.{nameof(Play.Style)}:{Play.Style.ToMemberName()}(0x{(short)Play.Style:X2})");
        return members.OfType<string>();
    }
    internal readonly string GetDebuggerDisplay()
        => string.Join(", ", GetInnerDisplay());
    ///<inheritdoc/>
    public override readonly string ToString()
        => $"{nameof(ChunkHeader)}{{{string.Join(", ", GetInnerDisplay())}}}";
}
