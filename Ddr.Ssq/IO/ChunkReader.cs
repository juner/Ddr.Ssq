﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Ddr.Ssq.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
                else if (Chunk.Body is EmptyBody)
                {
                    Logger.LogTrace("EmptyBody skip.");
                    yield break;
                }
            }
        }
        public Chunk ReadChunk()
        {
            var Offset = Stream.Position;
            var Header = ReadHeader();
            var Body = ReadBody(Header);
            return new Chunk
            {
                Offset = Offset,
                Header = Header,
                Body = Body,
            };
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
        public IBody ReadBody(in ChunkHeader Header)
        {
            if (Header is { Type: ChunkType.EndOfFile })
                return new EmptyBody();
            if (Header is { Length: <= 0 })
            {
                Logger.LogWarning("Length is {Length}", Header.Length);
                return new EmptyBody();
            }
            if (Header is { Entry: <= 0 })
            {
                Logger.LogWarning("Entry is {Entry}", Header.Entry);
                return new EmptyBody();
            }
            var Entry = Header.Entry;
            var Length = Header.Length;
            var Size = Marshal.SizeOf<ChunkHeader>();
            Logger.LogDebug("usable body size Length:{Length} - HeaderSize:{HeaderSize} -> {Size}", Length, Size, Length - Size);
            IBody Body = Header.Type switch
            {
                ChunkType.TempoTFPSConfig => new TempoTFPSConfigBody(),
                ChunkType.BiginFinishConfig => new BiginFinishConfigBody(),
                ChunkType.StepData => new StepDataBody(),
                _ => new OtherBody(),
            };
            if (Body is ITimeOffsetBody TimeOffsetBody)
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
                TimeOffsetBody.TimeOffsets = TimeOffsets;
                Logger.LogResult(nameof(TimeOffsetBody.TimeOffsets), TimeOffsets);
            }
            switch (Body)
            {
                case TempoTFPSConfigBody TempoTFPSConfigBody:
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
                        TempoTFPSConfigBody.Values = Tempo_TFPS_Config;
                        Logger.LogResult(nameof(TempoTFPSConfigBody.Values), Tempo_TFPS_Config);
                    }
                    break;
                case BiginFinishConfigBody BiginFinishConfigBody:
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
                        var BiginFinishConfig = MemoryMarshal.Cast<byte, BiginFinishConfigType>(Span).ToArray();
                        BiginFinishConfigBody.Values = BiginFinishConfig;
                        Logger.LogResult(nameof(BiginFinishConfigBody.Values), BiginFinishConfig.Cast<short>());
                    }
                    break;
                case StepDataBody StepDataBody:
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
                        StepDataBody.Values = StepData;
                        Logger.LogResult(nameof(StepDataBody.Values), StepData);
                    }
                    break;
            }
            if (Size == Length)
                return Body;
            if (Header.Type is ChunkType.TempoTFPSConfig or ChunkType.BiginFinishConfig or ChunkType.StepData)
                Logger.LogWarning("{ChunkType} has OtherData", Header.Type);
            if (Body is IOtherDataBody OtherDataBody)
            {
                var _Size = Size;
                var UseSize = Length - _Size;
                Size += UseSize;
                Debug.Assert(Size <= Length, $"over size exception.Size:{_Size} -> {Size} Length:{Length}");
                using var buffer = Pool.Rent(UseSize);
                var Span = buffer.Memory.Span[..UseSize];
                var readed = Stream.Read(Span);
                Logger.LogReaded(Stream, readed, Span[..readed]);
                if (UseSize != readed)
                    Logger.LogWarning($"readed size is mismatch. UseSize:{UseSize} readed:{readed}");
                var OtherData = Span[..readed].ToArray();
                OtherDataBody.Values = OtherData;
                Logger.LogResult(nameof(IOtherDataBody.Values), OtherData);
            }
            Debug.Assert(Size == Length, $"has no read byte. Size:{Size} Length:{Length}");
            return Body;
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
