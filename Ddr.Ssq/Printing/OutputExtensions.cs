using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using static Ddr.Ssq.SoloStepType;
using static Ddr.Ssq.StepType;

namespace Ddr.Ssq.Printing
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
                    Builder.Append($"[{Index + 1,3}]");
                    Builder.Append($"[{Chunk.Offset,5:X}][{Chunk.Header.Length,6} ({Chunk.Header.Length,5:X})]");
                    Builder.Append($"[{(short)Chunk.Header.Type:X2}:{Chunk.Header.Type.ToMemberName(),-20}]");
                    Builder.Append(Chunk.Header.Type switch
                    {
                        ChunkType.TempoTFPSConfig
                            => $"[TfPS  : ({Chunk.Header.Param,4:X}) {Chunk.Header.Param,4}] {string.Empty,14}",
                        ChunkType.BiginFinishConfig
                            => $"[param : ({Chunk.Header.Param,4:X}) {Chunk.Header.Param,4}] {string.Empty,14}",
                        ChunkType.StepData
                            => $"[level : ({Chunk.Header.Param,4:X4}) {Chunk.Header.PlayStyle.ToMemberName(),-8} {Chunk.Header.PlayDifficulty.ToMemberName(),-10}]",
                        _
                            => $"[param : ({Chunk.Header.Param,4:X}) {Chunk.Header.Param,4}] {string.Empty,14}",
                    });
                    Builder.Append($"[Entry: {Chunk.Header.Entry,4} ({Chunk.Header.Entry,4:X})]");
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
        public static void WriteChunkBodyInfo(this TextWriter Writer, Chunk Chunk, OutputOptions? Options = null)
        {
            var Builder = new StringBuilder();
            try
            {
                Builder.AppendLine(Chunk.Header.Type switch
                {
                    ChunkType.StepData
                        => $"[[[{(short)Chunk.Header.Type,2:X2}:{Chunk.Header.Type.ToMemberName(),-20}]]] [level : ({Chunk.Header.Param,4:X4}) {Chunk.Header.PlayStyle.ToMemberName(),-8} {Chunk.Header.PlayDifficulty.ToMemberName(),-10}]",
                    _
                        => $"[[[{(short)Chunk.Header.Type,2:X2}:{Chunk.Header.Type.ToMemberName(),-20}]]]",
                });
                Options = (Options ?? new OutputOptions()).AutoMapping(Writer.Encoding);
                var Lines = (Chunk.Header.Type, Chunk.Body) switch
                {
                    (ChunkType.EndOfFile, _) => Enumerable.Empty<string>(),
                    (ChunkType.TempoTFPSConfig, TempoTFPSConfigBody Body) => Tempo_TFPS_ConfigToFormatEnumerable(Chunk.Header, Body, Options),
                    (ChunkType.BiginFinishConfig, BiginFinishConfigBody Body) => Bigin_Finish_ConfigToFormatEnumerable(Chunk.Header, Body, Options),
                    (ChunkType.StepData, StepDataBody Body) => StepDataToFormatEnumerable(Chunk.Header, Body, Options),
                    _ => Enumerable.Empty<string>(),
                };
                
                if (Chunk.Body is IOtherDataBody OtherDataBody)
                    Lines = Lines.Concat(OtherDataToFormatEnumerable(Chunk.Header, OtherDataBody, Options));

                foreach (var Line in Lines)
                    Builder.AppendLine(Line);
            }
            finally
            {
                Writer.WriteLine(Builder.ToString());
                Writer.Flush();
            }
        }
        /// <summary>
        /// output <see cref="ChunkType.TempoTFPSConfig"/> body information.
        /// </summary>
        /// <param name="Header"></param>
        /// <param name="Body"></param>
        /// <returns></returns>
        internal static IEnumerable<string> Tempo_TFPS_ConfigToFormatEnumerable(ChunkHeader Header, TempoTFPSConfigBody Body, OutputOptions Options)
        {
            const ChunkType BASE_TYPE = ChunkType.TempoTFPSConfig;
            if (Header.Type is not BASE_TYPE)
                throw new ArgumentException($"{nameof(Header)} type is not {BASE_TYPE}({(short)BASE_TYPE:X}). {nameof(Header.Type)} : {Header.Type}({(short)Header.Type:X})", nameof(Header));

            for (var i = 0; i < Header.Entry; i++)
            {
                if (i is 0) continue;
                var TimeOffset = Body.TimeOffsets[i];
                var LastTimeOffset = Body.TimeOffsets[i - 1];
                var DeltaOffset = TimeOffset - LastTimeOffset;
                var DeltaTicks = Body.Values[i] - Body.Values[i - 1];

                var TfPS = Header.Param;
                var MeasureLength = 4096;

                var bpm = (DeltaOffset / (double)MeasureLength) / ((DeltaTicks / (double)TfPS) / 240);

                yield return $"[01:BPM][({LastTimeOffset,8:X}:{TimeOffset,8:X}) {LastTimeOffset,8}:{TimeOffset,8}][BPM:{bpm:F5}] Delta> Offset:({DeltaOffset,6:X}){DeltaOffset,7} / Ticks:({DeltaTicks,5:X}){DeltaTicks,7} ";
            }
        }
        /// <summary>
        /// output <see cref="ChunkType.BiginFinishConfig"/> body information.
        /// </summary>
        /// <param name="Header"></param>
        /// <param name="Body"></param>
        /// <returns></returns>
        internal static IEnumerable<string> Bigin_Finish_ConfigToFormatEnumerable(ChunkHeader Header, BiginFinishConfigBody Body, OutputOptions Options)
        {
            const ChunkType BASE_TYPE = ChunkType.BiginFinishConfig;
            if (Header.Type is not BASE_TYPE)
                throw new ArgumentException($"{nameof(Header)} type is not {BASE_TYPE}({(short)BASE_TYPE:X}). {nameof(Chunk.Header.Type)} : {Header.Type}({(short)Header.Type:X})", nameof(Header));

            for (var i = 0; i < Header.Entry; i++)
            {
                var TimeOffset = Body.TimeOffsets[i];
                var LastTimeOffset = i is 0 ? 0 : Body.TimeOffsets[i - 1];

                var DeltaOffset = TimeOffset - LastTimeOffset;
                var ConfigType = Body.Values[i];
                yield return $"[02:BFC][({TimeOffset,6:X}) {TimeOffset,8}][func.{(short)ConfigType,4:X}: {Body.Values[i].ToMemberName(),-18} ] Delta> Offset:({DeltaOffset,6:X}){DeltaOffset,7} ";
            }
        }
        /// <summary>
        /// output <see cref="ChunkType.StepData"/> body information.
        /// </summary>
        /// <param name="Header"></param>
        /// <param name="Body"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static IEnumerable<string> StepDataToFormatEnumerable(ChunkHeader Header, StepDataBody Body, OutputOptions Options)
        {
            const ChunkType BASE_TYPE = ChunkType.StepData;
            Debug.Assert(Options.MappingType is not CharMappingType.Auto);
            if (Header.Type is not BASE_TYPE)
                throw new ArgumentException($"{nameof(Header)} type is not {BASE_TYPE}({(short)BASE_TYPE:X}). {nameof(Header.Type)} : {Header.Type}({(short)Header.Type:X})", nameof(Header));
            var EA = Options.EmptyArrow;
            var NA = Options.NoArrow;
            var LeftArrow = Options.LeftArrow;
            var RightArrow = Options.RightArrow;
            var UpArrow = Options.UpArrow;
            var DownArrow = Options.DownArrow;
            var NorthEastArrow = Options.NorthEastArrow;
            var NorthWestArrow = Options.NorthWestArrow;
            var ShockArrow = Options.ShockArrow;
            var ShockArrowWord = Options.ShockArrowWord;
            var FreezeArrowSign = Options.FreezeArrowSign;
            var FreezeArrowWord = Options.FreezeArrowWord;
            var FreezeArrowCharactor = Options.FreezeArrowCharactor;
            var lastStep = new Queue<byte>();
            var lastCount = 0;

            for (var i = 0; i < Header.Entry; i++)
            {
                var TimeOffset = Body.TimeOffsets[i];
                var LastTimeOffset = i is 0 ? 0 : Body.TimeOffsets[i - 1];
                var DeltaOffset = TimeOffset - LastTimeOffset;
                var Step = Header.PlayStyle switch
                {
                    PlayStyle.Single
                        => new[] { NA, EA, EA, EA, EA, NA, NA, NA, NA, NA, NA, NA, ":  " },
                    PlayStyle.Solo
                        => new[] { EA, EA, EA, EA, EA, EA, NA, NA, NA, NA, NA, NA, ":  " },
                    PlayStyle.Double
                        => new[] { NA, EA, EA, EA, EA, NA, NA, EA, EA, EA, EA, NA, ":  " },
                    PlayStyle.Battle
                        => new[] { NA, EA, EA, EA, EA, NA, NA, EA, EA, EA, EA, NA, ":  " },
                    _
                        => new[] { NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, NA, ":  " },
                };
                var _step = Body.Values[i];
                Step[12] = $":{_step:X2}";
                if (_step is 0)
                // freeze arrow判定
                {
                    var l = lastStep.Any() ? lastStep.Dequeue() : 0;
                    Step[l] = FreezeArrowSign;
                    var p = l switch
                    {
                        7 or 8 or 9 or 10 => 6,
                        1 or 2 or 3 or 4 or _ => 5,
                    };
                    Step[p] = $"{FreezeArrowCharactor}{(lastCount - lastStep.Count)}";
                }
                else
                // nomal arrow or shock arrow
                {
                    lastStep.Clear();
                    lastCount = 0;
                    if (Header.PlayStyle
                        is PlayStyle.Single
                        or PlayStyle.Double
                        or PlayStyle.Battle) //solo以外
                    {
                        var s = (StepType)_step;
                        if ((s & Player1Left) > 0) { Step[1] = LeftArrow; lastStep.Enqueue(1); lastCount++; }
                        if ((s & Player1Down) > 0) { Step[2] = DownArrow; lastStep.Enqueue(2); lastCount++; }
                        if ((s & Player1Up) > 0) { Step[3] = UpArrow; lastStep.Enqueue(3); lastCount++; }
                        if ((s & Player1Right) > 0) { Step[4] = RightArrow; lastStep.Enqueue(4); lastCount++; }
                        if ((s & Player1Special) == Player1Special) // Shock Arrow
                        { Step[1] = Step[2] = Step[3] = Step[4] = ShockArrow; Step[5] = ShockArrowWord; }

                        if (i < Header.Entry - 1)
                        {
                            if ((s & Player1Special) > 0 && Body.Values[i + 1] == 0x0) // freeze arrow 
                            { Step[5] = FreezeArrowWord; }
                        }
                    }
                    if (Header.PlayStyle
                        is PlayStyle.Solo)
                    {
                        var s = (SoloStepType)_step;
                        if ((s & SoloPlayerLeft) > 0) { Step[0] = LeftArrow; }
                        if ((s & SoloPlayerNorthWest) > 0) { Step[1] = NorthWestArrow; }
                        if ((s & SoloPlayerDown) > 0) { Step[2] = DownArrow; }
                        if ((s & SoloPlayerUp) > 0) { Step[3] = UpArrow; }
                        if ((s & SoloPlayerNorthEast) > 0) { Step[4] = NorthEastArrow; }
                        if ((s & SoloPlayerRight) > 0) { Step[5] = RightArrow; }
                    }

                    if (Header.PlayStyle
                        is PlayStyle.Double
                        or PlayStyle.Battle)
                    {
                        var s = (StepType)_step;
                        if ((s & Player2Left) > 0) { Step[7] = LeftArrow; lastStep.Enqueue(7); lastCount++; }
                        if ((s & Player2Down) > 0) { Step[8] = DownArrow; lastStep.Enqueue(8); lastCount++; }
                        if ((s & Player2Up) > 0) { Step[9] = UpArrow; lastStep.Enqueue(9); lastCount++; }
                        if ((s & Player2Right) > 0) { Step[10] = RightArrow; lastStep.Enqueue(10); lastCount++; }

                        if ((s & Player2Special) == Player2Special) //Shock Arrow
                        { Step[7] = Step[8] = Step[9] = Step[10] = ShockArrow; Step[6] = ShockArrowWord; }

                        if (i < Header.Entry - 1)
                        {
                            if ((s & Player2Special) > 0 && Body.Values[i + 1] == 0x0) // freeze arrow 
                            { Step[6] = FreezeArrowWord; }
                        }
                    }
                }
                yield return $"[03:STP][({TimeOffset,6:X}) {TimeOffset,8}][{string.Join("", Step)}] Delta> Offset:({DeltaOffset,6:X}){DeltaOffset,7} ";
            }
        }
        internal static IEnumerable<string> OtherDataToFormatEnumerable(ChunkHeader Header, IOtherDataBody Body, OutputOptions Options)
        {
            if (!Options.ViewOtherBinary)
                yield break;
            if (!Body.Values.Any())
                yield break;

            yield return $"[binary: length: {Body.Values.Length}]";
            yield return $"[{string.Join(" ", Body.Values.Select(b => $"{b:X2}"))}]";
        }
    }
}
