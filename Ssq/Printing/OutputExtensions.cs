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
                            => $"[level : ({Chunk.Header.Param:X04}) {Chunk.Header.Param:D04} {Chunk.Header.PlayStyle.ToMemberName(),-8} {Chunk.Header.PlayDifficulty.ToMemberName(),-10}]",
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
            var Builder = new StringBuilder();
            try
            {
                Builder.AppendLine(Chunk.Header.Type switch
                {
                    ChunkType.StepData
                        => $"[[[{(short)Chunk.Header.Type:X2}:{Chunk.Header.Type.ToMemberName():-20}]]] [level : ({Chunk.Header.Param:X4}) {Chunk.Header.PlayStyle.ToMemberName(),-8} {Chunk.Header.PlayDifficulty.ToMemberName(),-10}]",
                    _
                        => $"[[[{(short)Chunk.Header.Type:X2}:{Chunk.Header.Type.ToMemberName():-20}]]]",
                });
                switch (Chunk.Header.Type)
                {
                    case ChunkType.EndOfFile:
                        return;
                    case ChunkType.Tempo_TFPS_Config:
                        {
                            for(var i=0;i<Chunk.Header.Entry; i++)
                            {
                                if (i is 0) continue;
                                var TimeOffset = Chunk.TimeOffsets[i];
                                var LastTimeOffset = Chunk.TimeOffsets[i - 1];
                                var DeltaOffset = TimeOffset - LastTimeOffset;
                                var DeltaTicks = Chunk.Tempo_TFPS_Config[i] - Chunk.Tempo_TFPS_Config[i - 1];

                                var offset_hexD = $"{DeltaOffset:X6}"[^6..6];
                                var offset_hex0 = $"{LastTimeOffset:X6}"[^6..6]; //負の表示対応 (DDR A)
                                var offset_hex1 = $"{TimeOffset:X6}"[^6..6]; //負の表示対応 (DDR A)

                                var TfPS = Chunk.Header.Param;
                                var MeasureLength = 4096;
                                
                                var bpm = (DeltaOffset / (double)MeasureLength) / ((DeltaTicks / (double)TfPS) / 240);

                                Builder.AppendLine($"[01:BPM][({offset_hex0:6}:{offset_hex1:6}) {LastTimeOffset:8D}:{TimeOffset:8D}][BPM:{bpm:F5}] Delta> Offset:({offset_hexD:6}){DeltaOffset:D7} / ({DeltaTicks:X5}){DeltaTicks:D7}");
                            }
                        }
                        break;
                    case ChunkType.Bigin_Finish_Config:
                        {

                            for (var i = 0; i < Chunk.Header.Entry; i++)
                            {
                                var TimeOffset = Chunk.TimeOffsets[i];
                                var offset_hex1 = $"{TimeOffset:X6}"[^6..6];
                                var LastTimeOffset = i is 0 ? 0 : Chunk.TimeOffsets[i - 1];
                                var offset_hexD = i is 0 ? "0" : $"{TimeOffset - Chunk.TimeOffsets[i - 1]:X6}"[^6..6];

                                var DeltaOffset = TimeOffset - LastTimeOffset;
                                var ConfigType = Chunk.Bigin_Finish_Config[i];
                                Builder.AppendLine($"[02:BFC][({offset_hex1:6}) {TimeOffset:D8}][func.{(short)ConfigType:X4}: {Chunk.Bigin_Finish_Config[i].ToMemberName():-18} ] Delta> Offset:({offset_hexD:6}){DeltaOffset:D7} ");
                            }
                        }
                        break;
                    case ChunkType.StepData:
                        {
                            var lastStep = new List<byte>();
                            var lastCount = 0;

                            for (var i = 0; i < Chunk.Header.Entry; i++)
                            {
                                var TimeOffset = Chunk.TimeOffsets[i];
                                var LastTimeOffset = i is 0 ? 0 : Chunk.TimeOffsets[i - 1];
                                var DeltaOffset = TimeOffset - LastTimeOffset;
                                var Step = Chunk.Header.PlayStyle switch
                                {
                                    PlayStyle.Single 
                                        => new[] { "　", "…", "…", "…", "…", "　", "　", "　", "　", "　", "　", "　", ":  " },
                                    PlayStyle.Solo 
                                        => new[] { "…", "…", "…", "…", "…", "…", "　", "　", "　", "　", "　", "　", ":  " },
                                    PlayStyle.Double
                                        => new[] { "　", "…", "…", "…", "…", "　", "　", "…", "…", "…", "…", "　", ":  " },
                                    PlayStyle.Battle
                                        => new[] { "　", "…", "…", "…", "…", "　", "　", "…", "…", "…", "…", "　", ":  " },
                                    _
                                        => new[] { "　", "　", "　", "　", "　", "　", "　", "　", "　", "　", "　", "　", ":  " },
                                };
                                var _step = Chunk.StepData[i];
                                Step[12] = $"{(byte)_step:X02}";
                                if (_step is default(StepType))
                                {
                                    var l = Shift(lastStep);
                                    Step[l] = "＃";
                                    var p = l switch
                                    {
                                        7 or 8 or 9 or 10 => 6,
                                        1 or 2 or 3 or 4 or _ => 5,
                                    };
                                    Step[p] = $"L{(lastCount - lastStep.Count)}";
                                } else
                                {
                                    lastStep.Clear();
                                    lastCount = 0;
                                    if (Chunk.Header.PlayStyle 
                                        is PlayStyle.Single 
                                        or PlayStyle.Double 
                                        or PlayStyle.Battle) //solo以外
                                    {
                                        if (((byte)_step & 0x01) > 0) { Step[1] = "←"; lastStep.Add(1); lastCount++; }
                                        if (((byte)_step & 0x02) > 0) { Step[2] = "↓"; lastStep.Add(2); lastCount++; }
                                        if (((byte)_step & 0x04) > 0) { Step[3] = "↑"; lastStep.Add(3); lastCount++; }
                                        if (((byte)_step & 0x08) > 0) { Step[4] = "→"; lastStep.Add(4); lastCount++; }
                                        if (((byte)_step & 0xf) == 0xf) // Shock Arrow
                                        { Step[1] = "◆"; Step[2] = "◆"; Step[3] = "◆"; Step[4] = "◆"; Step[5] = "衝"; } 

                                        if (i < Chunk.Header.Entry - 1)
                                        {
                                            if (((byte)_step & 0xf) > 0 && (byte)Chunk.StepData[i+1] == 0x0) // freeze arrow 
                                            { Step[5] = "長"; } 
                                        }
                                    }
                                    if (Chunk.Header.PlayStyle 
                                        is PlayStyle.Solo)
                                    {
                                        if (((byte)_step & 0x01) > 0) { Step[0] = "←"; }
                                        if (((byte)_step & 0x10) > 0) { Step[1] = "↖"; }
                                        if (((byte)_step & 0x02) > 0) { Step[2] = "↓"; }
                                        if (((byte)_step & 0x04) > 0) { Step[3] = "↑"; }
                                        if (((byte)_step & 0x40) > 0) { Step[4] = "↗"; }
                                        if (((byte)_step & 0x08) > 0) { Step[5] = "→"; }
                                    }

                                    if (Chunk.Header.PlayStyle 
                                        is PlayStyle.Double 
                                        or PlayStyle.Battle)
                                    {
                                        if (((byte)_step & 0x10) > 0) { Step[7] = "←"; }
                                        if (((byte)_step & 0x20) > 0) { Step[8] = "↓"; }
                                        if (((byte)_step & 0x40) > 0) { Step[9] = "↑"; }
                                        if (((byte)_step & 0x80) > 0) { Step[10] = "→"; }
                                        if (((byte)_step & 0xF0) == 0xF0) //Shock Arrow
                                        { Step[7] = "◆"; Step[8] = "◆"; Step[9] = "◆"; Step[10] = "◆"; Step[6] = "衝"; }
                                        
                                        if (i < Chunk.Header.Entry -1)
                                        {
                                            if (((byte)_step & 0xf0) > 0 && (byte)Chunk.StepData[i + 1] == 0x0) // freeze arrow 
                                            { Step[6] = "長"; }
                                        }
                                    }
                                }
                                Builder
                                    .Append($"[03:STP][({TimeOffset:X6}) {TimeOffset:D8}][")
                                    .Append(string.Join("", Step))
                                    .AppendLine($"] Delta> Offset:({DeltaOffset:X6}){DeltaOffset:D7}");
                            }
                        }
                        break;
                    default:
                        break;

                }
            }
            finally
            {
                Writer.WriteLine(Builder.ToString());
                Writer.Flush();
            }

            static T Shift<T>(IList<T> Value)
            {
                var Count = Value.Count;
                var V = Value[Count - 1];
                Value.RemoveAt(Count - 1);
                return V;
            }
        }
    }
}
