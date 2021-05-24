using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ddr.Ssq
{
    /// <summary>
    /// <see cref="ChunkHeader.Param"/> alternate <see cref="ChunkType.StepData"/> version.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public struct StepPlayType
    {
        /// <summary>
        /// StepData Play Style
        /// </summary>
        public PlayStyle Style;
        /// <summary>
        /// StepData Play Difficulty
        /// </summary>
        public PlayDifficulty Difficulty;
        StepPlayType(short Param) => ParamToValue(Param, out Style, out Difficulty);
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="Style"></param>
        /// <param name="Difficulty"></param>
        public StepPlayType(PlayStyle Style, PlayDifficulty Difficulty) => (this.Style, this.Difficulty) = (Style, Difficulty);
        /// <summary>
        /// Deconstruct <see cref="StepPlayType"/> -&gt; <see cref="ValueTuple{PlayStyle, PlayDifficulty}"/>
        /// </summary>
        /// <param name="Style"></param>
        /// <param name="Difficulty"></param>
        public readonly void Deconstruct(out PlayStyle Style, out PlayDifficulty Difficulty) => (Style, Difficulty) = (this.Style, this.Difficulty);
        /// <summary>
        /// to Param(<see cref="short"/>)
        /// </summary>
        /// <returns></returns>
        public readonly short ToParam() => ValueToParam(Style, Difficulty);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ParamToValue(short Param, out PlayStyle Style, out PlayDifficulty Difficulty)
            => (Style, Difficulty) = ((PlayStyle)(short)(Param & 0xff), (PlayDifficulty)(short)((Param & 0xff00) >> 8));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static short ValueToParam(PlayStyle Style, PlayDifficulty Difficulty)
            => (short)((((short)Difficulty) << 8) | ((byte)Style));
        /// <summary>
        /// from Param(<see cref="short"/>)
        /// </summary>
        /// <param name="Param"></param>
        /// <returns></returns>
        public static StepPlayType FromParam(short Param) => new(Param);
        /// <summary>
        /// <see cref="StepPlayType"/> to <see cref="short"/> 
        /// </summary>
        /// <param name="PlayType"></param>
        public static explicit operator short(in StepPlayType PlayType) => PlayType.ToParam();
        /// <summary>
        /// <see cref="short"/> to <see cref="StepPlayType"/>
        /// </summary>
        /// <param name="Param"></param>
        public static explicit operator StepPlayType(short Param) => FromParam(Param);
        string GetDebuggerDisplay() => $"{Style}(0x{Style:x}),{Difficulty}(0x{Difficulty:x})";
        /// <inheritdoc/>
        public override string ToString()
            => nameof(StepPlayType) + "{"
            + string.Join(", ", new string?[] {
                Style is 0 ? null : $"{nameof(Style)}:{Style}",
                Difficulty is 0 ? null : $"{nameof(Difficulty)}:{Difficulty}",
            }.OfType<string>())
            + "}";
    }
}
