using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ddr.Ssq
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public struct ChunkHeader
    {
        [FieldOffset(0)]
        public int Length;
        public uint LongLength { readonly get => unchecked((uint)Length); set => Length = unchecked((int)value); }
        [FieldOffset(4)]
        [MarshalAs(UnmanagedType.I2)]
        public ChunkType Type;
        [FieldOffset(6)]
        public short Param;
        [FieldOffset(6)]
        public StepPlayType Play;
        [FieldOffset(8)]
        public int Entry;
        public ChunkHeader(int Length = 0, ChunkType Type = ChunkType.EndOfFile, short Param = 0, int Entry = 0)
        {
            Play = default;
            (this.Length, this.Type, this.Param, this.Entry) = (Length, Type, Param, Entry);
        }
        public static ChunkHeader CreateStepData(int Length = 0, StepPlayType Play = default, int Entry = 0)
            => new(Length, ChunkType.StepData, (short)Play, Entry);
        internal readonly string GetDebuggerDisplay()
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
            return $"{nameof(ChunkHeader)}{{{string.Join(", ", members)}}}";
        }
    }
}
