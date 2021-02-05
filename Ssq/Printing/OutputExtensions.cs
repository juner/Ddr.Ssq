using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
            var Builder = new StringBuilder();
            try
            {
                Builder.AppendLine($"{BORDER}{Environment.NewLine}{HEADER}{Environment.NewLine}{BORDER}");
                foreach (var (Chunk, Index) in Chunks.Select((v, i) => (v, i)))
                {
                    Builder.Append($"[{Index + 1:d3}]");
                    Builder.Append($"[{Chunk.Offset:X4}][{Chunk.Header.Length:d6} (0x{Chunk.Header.Length:X5})]");
                    Builder.Append($"[{(short)Chunk.Header.Type:X2}:{Chunk.Header.Type.ToMemberName(),20}]");
                    Builder.Append(Chunk.Header.Type switch
                    {
                        ChunkType.Tempo_TFPS_Config
                            => $"[TfPS  : ({Chunk.Header.Param:X4}) {Chunk.Header.Param:D4}] {string.Empty,14}",
                        ChunkType.Bigin_Finish_Config
                            => $"[param : ({Chunk.Header.Param:X4}) {Chunk.Header.Param:D4}] {string.Empty,14}",
                        ChunkType.StepData
                            => $"[level : ({Chunk.Header.Param:X04}) {Chunk.Header.Param:D04} {Chunk.Header.PlayStyle.ToMemberName(),8} {Chunk.Header.PlayDifficulty.ToMemberName(),10}]",
                        _
                            => $"[param : ({Chunk.Header.Param:X4}) {Chunk.Header.Param:D4}] {string.Empty,14}",
                    });
                    Builder.Append($"[Entry: {Chunk.Header.Entry:D4} ({Chunk.Header.Entry:X4})]");
                    Builder.AppendLine();
                    if (Chunk.Header.Type is ChunkType.EndOfFile)
                        break;
                }
                Builder.Append(BORDER);
            }
            finally
            {
                Writer.WriteLine(Builder.ToString());
                Writer.Flush();
            }
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
