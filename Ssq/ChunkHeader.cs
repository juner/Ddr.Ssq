using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ssq
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public struct ChunkHeader
    {
        public int Length;
        [MarshalAs(UnmanagedType.I2)]
        public ChunkType Type;
        public short Param;
        public int Entry;
        public PlayDifficulty PlayDifficulty
        {
            readonly get => (PlayDifficulty)(short)((Param & 0xff00) >> 8);
            set => Param = (short)((((short)value) << 8) | (Param & 0xff));
        }
        public PlayStyle PlayStyle
        {
            readonly get => (PlayStyle)(short)(Param & 0xff);
            set => Param = (short)((Param & 0xff00) | (((short)value) & 0xff));
        }

        internal readonly string GetDebuggerDisplay()
        {
            IEnumerable<string?> members = new[] {
                Length<=0 ? null: $"{nameof(Length)}:{Length}",
                $"{nameof(Type)}:{Type.ToMemberName()}({Type:d})",
                $"{nameof(Param)}:0x{Param:X4}",
                $"{nameof(Entry)}:0x{Entry:X8}",
            };
            if (Type is ChunkType.StepData)
                members = members
                    .Append($"{nameof(PlayDifficulty)}:{PlayDifficulty.ToMemberName()}(0x{(short)PlayDifficulty:X4})")
                    .Append($"{nameof(PlayStyle)}:{PlayStyle.ToMemberName()}(0x{(short)PlayStyle:X4})");
            return $"{nameof(ChunkHeader)}{{{string.Join(", ", members)}}}";
        }
    }
}
