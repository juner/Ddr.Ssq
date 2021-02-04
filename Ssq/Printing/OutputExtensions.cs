using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ssq.Printing
{
    public static class OutputExtensions
    {
        /// <summary>
        /// Write Text Chunk Summary.
        /// see <a href="https://github.com/pumpCurry/ssqcheck/blob/bb5a9a8181beae7af681612c5f85152f2548cfaa/ssqcheck.php#L68-L109">pumpCurry/ssqcheck/ssqcheck.php</a>
        /// </summary>
        /// <param name="Writer"></param>
        /// <param name="Chunks"></param>
        public static void WriteChunckSummary(this TextWriter Writer, IEnumerable<Chunk> Chunks)
        {
            const string BORDER = "+----+------+--------------+-------------------------+-----------------------------------+------------------+";
            const string HEADER = "|#   |Addr: |Length:( HEX )|Chunk type:              |Values                             |Entry             |";
            Writer.WriteLine($"{BORDER}{Environment.NewLine}{HEADER}{Environment.NewLine}{BORDER}");
            foreach (var (Chunk, Index) in Chunks.Select((v, i) => (v, i)))
            {
                Writer.Write($"[{Index + 1:d3}]");
                Writer.Write($"[{Chunk.Offset:X2}][{Chunk.Header.Length:d6} (0x{Chunk.Header.Length:X5})]");
                Writer.Write($"[{Chunk.Header.Type:X2}:{Chunk.Header.Type.ToMemberName().PadLeft(20)}]");
                Writer.Write(Chunk.Header.Type switch
                {
                    ChunkType.Tempo_TFPS_Config
                        => $"[TfPS  : ({Chunk.Header.Param:X4}) {Chunk.Header.Param:D4}] {string.Empty.PadLeft(14)}",
                    ChunkType.Bigin_Finish_Config
                        => $"[param : ({Chunk.Header.Param:X4}) {Chunk.Header.Param:D4}] {string.Empty.PadLeft(14)}",
                    ChunkType.StepData
                        => $"[level : ({Chunk.Header.Param:X04}) {Chunk.Header.Param:D04} {Chunk.Header.PlayStyle.ToMemberName().PadLeft(8)} {Chunk.Header.PlayDifficulty.ToMemberName().PadLeft(10)}]",
                    _
                        => $"[param : ({Chunk.Header.Param:X4}) {Chunk.Header.Param:D4}] {string.Empty.PadLeft(14)}",
                });
                Writer.Write($"[Entry: {Chunk.Header.Entry:D4} ({Chunk.Header.Entry:X4})]");
                Writer.WriteLine();
                if (Chunk.Header.Type is ChunkType.EndOfFile)
                    break;
            }
            Writer.WriteLine(BORDER);
        }
        /// <summary>
        /// Write Text Chunk Body Info,
        /// see <a href="https://github.com/pumpCurry/ssqcheck/blob/bb5a9a8181beae7af681612c5f85152f2548cfaa/ssqcheck.php#L265-L446">pumpCurry/ssqcheck/ssqcheck.php</a>
        /// </summary>
        /// <param name="Writer"></param>
        /// <param name="Chunk"></param>
        public static void WriteChunkBodyInfo(this TextWriter Writer, Chunk Chunk)
        {

            throw new NotImplementedException();
        }
    }
}
