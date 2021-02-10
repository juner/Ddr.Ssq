using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Ddr.Ssq.Internal;
namespace Ddr.Ssq.IO
{
    public class ChunkReader : IDisposable
    {
        readonly Stream Stream;
        readonly bool LeaveOpen;
        readonly MemoryPool<byte> Pool;
        public ILogger<ChunkReader> Logger { get; init; } = NullLogger<ChunkReader>.Instance;
        public ChunkReader(Stream Stream) : this(Stream, false, default!, default!) { }
        public ChunkReader(Stream Stream, bool LeaveOpen) : this(Stream, LeaveOpen, default!, default!) { }
        public ChunkReader(Stream Stream, bool LeaveOpen, MemoryPool<byte> Pool, ILogger<ChunkReader> Logger)
            => (this.Stream, this.LeaveOpen, this.Pool, this.Logger) = (Stream, LeaveOpen, Pool ?? MemoryPool<byte>.Shared, Logger ?? NullLogger<ChunkReader>.Instance);
        public IEnumerable<Chunk> ReadToEnd()
        {
            Logger.LogDebug("START ChunkReader.ReadToEnd()");
            var Length = Stream.Length;
            var Counter = 0;
            while (Stream.Position < Length)
            {
                Logger.LogTrace("Stream Position:{Position} < Length:{Length}", Stream.Position, Length);
                Logger.LogTrace("ReadChunk [{Counter}]", Counter++);
                var Chunk = ReadChunk();
                yield return Chunk;
                if (Chunk.Header.Type is ChunkType.EndOfFile)
                {
                    Logger.LogTrace("ReadToEnd skip.");
                    yield break;
                }
            }
        }
        public Chunk ReadChunk()
        {
            var Chunk = new Chunk();
            ReadHeader(Chunk);
            ReadBody(Chunk, Chunk.Header);
            return Chunk;
        }
        static int headerSize;
        static int HeaderSize
        {
            get
            {
                if (headerSize is 0)
                    Interlocked.CompareExchange(ref headerSize, Marshal.SizeOf<ChunkHeader>(), 0);
                return headerSize;
            }
        }
        public ChunkHeader ReadHeader()
        {
            Logger.LogDebug("read header");
            using var buffer = Pool.Rent(HeaderSize);
            var memory = buffer.Memory;
            var span = memory.Span[..HeaderSize];
            var readed = Stream.Read(span);
            Logger.LogReaded(Stream, readed, memory[..readed].Span);
            if (readed < HeaderSize)
                SetZero(span[readed..]);
            var header = MemoryMarshal.Read<ChunkHeader>(span);
            Logger.LogDebug("-> {header}", header.GetDebuggerDisplay());
            return header;
        }
        public async ValueTask<ChunkHeader> ReadHeaderAsync(CancellationToken Token)
        {
            Logger.LogDebug("read header");
            using var buffer = Pool.Rent(HeaderSize);
            var memory = buffer.Memory[..HeaderSize];
            var readed = await Stream.ReadAsync(memory, Token);
            Logger.LogReaded(Stream, readed, memory[..readed].Span);
            if (readed < HeaderSize)
                SetZero(memory[readed..].Span);
            var header = MemoryMarshal.Read<ChunkHeader>(memory.Span);
            Logger.LogDebug("-> {header}", header.GetDebuggerDisplay());
            return header;
        }
        static void SetZero(Span<byte> span)
        {
            foreach (ref byte s in span)
                s = 0;
        }
        public void ReadHeader(Chunk Chunk)
        {
            var Offset = Stream.Position;
            var Header = ReadHeader();
            Chunk.Offset = Offset;
            Chunk.Header = Header;
        }
        public void ReadBody(Chunk Chunk, in ChunkHeader Header)
        {
            if (Header is { Type: ChunkType.EndOfFile } or { Length: 0 } or { Entry: 0 })
                return;
            var Entry = Header.Entry;
            var Length = Header.Length;
            var Size = Marshal.SizeOf<ChunkHeader>();
            Logger.LogDebug("usable body size Length:{Length} - HeaderSize:{HeaderSize} -> {Size}", Length, Size, Length - Size);
            //timeOffsetのリストを生成
            switch (Header.Type)
            {
                //case ChunkType.EndOfFile:
                //    return;
                default:
                case ChunkType.Tempo_TFPS_Config:
                case ChunkType.Bigin_Finish_Config:
                case ChunkType.StepData:
                    {
                        var UseSize = Entry * sizeof(uint);
                        var _Size = Size;
                        Size += UseSize;
                        Logger.LogDebug("{_Size} + {Entry} * {uint} -> {Size} Length:{Length}", _Size, Entry, sizeof(uint), Size, Length);
                        Debug.Assert(Size <= Length, $"over size exception.Size:{_Size} -> {Size} Length:{Length}");
                        using var buffer = Pool.Rent(UseSize);
                        var Span = buffer.Memory.Span[..UseSize];
                        var readed = Stream.Read(Span);
                        Logger.LogReaded(Stream, readed, Span[..readed]);
                        Debug.Assert(UseSize == readed, $"readed size is mismatch. UseSize:{UseSize} readed:{readed}");

                        var TimeOffsets = MemoryMarshal.Cast<byte, int>(Span).ToArray();
                        Chunk.TimeOffsets = TimeOffsets;
                        Logger.LogResult(nameof(Chunk.TimeOffsets), TimeOffsets);
                    }
                    break;
            }
            switch (Header.Type)
            {
                //case ChunkType.EndOfFile:
                //    return;
                case ChunkType.Tempo_TFPS_Config:
                    {
                        var _Size = Size;
                        var UseSize = Entry * sizeof(int);
                        Size += UseSize;
                        Logger.LogDebug("{_Size} + {Entry} * {int} -> {Size} Length:{Length}", _Size, Entry, sizeof(int), Size, Length);
                        Debug.Assert(Size <= Length, $"over size exception.Size:{_Size} -> {Size} Length:{Length}");
                        using var buffer = Pool.Rent(UseSize);
                        var Span = buffer.Memory.Span[..UseSize];
                        var readed = Stream.Read(Span);
                        Logger.LogReaded(Stream, readed, Span[..readed]);
                        Debug.Assert(UseSize == readed, $"readed size is mismatch. UseSize:{UseSize} readed:{readed}");
                        var Tempo_TFPS_Config = MemoryMarshal.Cast<byte, int>(Span).ToArray();
                        Chunk.Tempo_TFPS_Config = Tempo_TFPS_Config;
                        Logger.LogResult(nameof(Chunk.Tempo_TFPS_Config), Tempo_TFPS_Config);
                    }
                    break;
                case ChunkType.Bigin_Finish_Config:
                    {
                        var _Size = Size;
                        var UseSize = Entry * sizeof(short);
                        Size += UseSize;
                        Logger.LogDebug("{_Size} + {Entry} * {short} -> {Size} Length:{Length}", _Size, Entry, sizeof(short), Size, Length);
                        Debug.Assert(Size <= Length, $"over size exception.Size:{_Size} -> {Size} Length:{Length}");
                        using var buffer = Pool.Rent(UseSize);
                        var Span = buffer.Memory.Span[..UseSize];
                        var readed = Stream.Read(Span);
                        Logger.LogReaded(Stream, readed, Span[..readed]);
                        Debug.Assert(UseSize == readed, $"readed size is mismatch. UseSize:{UseSize} readed:{readed}");
                        var Bigin_Finish_Config = MemoryMarshal.Cast<byte, BiginFinishConfigType>(Span).ToArray();
                        Chunk.Bigin_Finish_Config = Bigin_Finish_Config;
                        Logger.LogResult(nameof(Chunk.Bigin_Finish_Config), Bigin_Finish_Config.Cast<short>());
                    }
                    break;
                case ChunkType.StepData:
                    {
                        var _Size = Size;
                        var UseSize = Entry * sizeof(byte);
                        Size += UseSize;
                        Logger.LogDebug("{_Size} + {Entry} * {int} -> {Size} Length:{Length}", _Size, Entry, sizeof(int), Size, Length);
                        Debug.Assert(Size <= Length, $"over size exception.Size:{_Size} -> {Size} Length:{Length}");
                        using var buffer = Pool.Rent(UseSize);
                        var Span = buffer.Memory.Span[..UseSize];
                        var readed = Stream.Read(Span);
                        Logger.LogReaded(Stream, readed, Span[..readed]);
                        Debug.Assert(UseSize == readed, $"readed size is mismatch. UseSize:{UseSize} readed:{readed}");
                        var StepData = Span.ToArray();
                        Chunk.StepData = StepData;
                        Logger.LogResult(nameof(Chunk.StepData), StepData);
                    }
                    break;
            }
            if (Size < Length)
            {
                var _Size = Size;
                var UseSize = Length - _Size;
                Size += UseSize;
                Debug.Assert(Size <= Length, $"over size exception.Size:{_Size} -> {Size} Length:{Length}");
                using var buffer = Pool.Rent(UseSize);
                var Span = buffer.Memory.Span[..UseSize];
                var readed = Stream.Read(Span);
                Logger.LogReaded(Stream, readed, Span[..readed]);
                Debug.Assert(UseSize == readed, $"readed size is mismatch. UseSize:{UseSize} readed:{readed}");
                var OtherData = Span.ToArray();
                Chunk.OtherData = OtherData;
                Logger.LogResult(nameof(Chunk.OtherData), OtherData);
            }
            Debug.Assert(Size == Length, $"has no read byte. Size:{Size} Length:{Length}");
        }
        public void Dispose()
        {
            if (!LeaveOpen)
                Stream.Dispose();
            GC.SuppressFinalize(this);
        }
    }
    static class Log
    {
        static readonly Action<ILogger, JoinFormatter, Exception?> dataReadLog = LoggerMessage.Define<JoinFormatter>(LogLevel.Trace, new EventId(0, "DataLoadTrace"), "[{data}]");
        static readonly Action<ILogger, int, long, Exception?> readedAndPositionLog = LoggerMessage.Define<int, long>(LogLevel.Debug, new EventId(0, "DataLoad"), " -> readed:{readed} Position:{Position}");
        static readonly Action<ILogger, int, Exception?> readedLog = LoggerMessage.Define<int>(LogLevel.Debug, new EventId(0, "DataLoad"), " -> readed:{readed}");
        static readonly Action<ILogger, string, JoinFormatter, Exception?> resultLog = LoggerMessage.Define<string, JoinFormatter>(LogLevel.Debug, new EventId(0, "LoadResult"), "{ResultName} [{OtherData}]");
        /// <summary>
        /// <see cref="Stream"/> 読み込み時のログ
        /// </summary>
        /// <param name="Logger"></param>
        /// <param name="Stream"></param>
        /// <param name="readed"></param>
        /// <param name="Memory"></param>
        public static void LogReaded(this ILogger Logger, Stream Stream, int readed, Span<byte> Memory)
        {
            if (Logger.IsEnabled(LogLevel.Trace))
                dataReadLog(Logger, new JoinFormatter(" ", Memory.ToArray().Select(v => $"{v,2:X2}")), null);
            if (Stream.CanSeek)
                readedAndPositionLog(Logger, readed, Stream.Position, null);
            else
                readedLog(Logger, readed, null);
        }
        public static void LogResult<T>(this ILogger Logger, string ResultName, IEnumerable<T> ResultArray)
        {
            if (Logger.IsEnabled(LogLevel.Debug))
                resultLog(Logger, ResultName, new JoinFormatter(" ", ResultArray.Select(v => string.Format("{0:X" + (Marshal.SizeOf<T>() * 2) + "}", v))), null);
        }
    }
}
