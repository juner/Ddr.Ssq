using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ssq
{
    public class ChunkReader : IDisposable
    {
        Stream Stream;
        readonly bool LeaveOpen;
        public ILogger<ChunkReader> Logger { get; init; } = NullLogger<ChunkReader>.Instance; 
        public ChunkReader(Stream Stream) : this(Stream, false, default!) { }
        public ChunkReader(Stream Stream, bool LeaveOpen) : this(Stream,LeaveOpen, default!) { }
        public ChunkReader(Stream Stream, bool LeaveOpen, ILogger<ChunkReader> Logger)
            => (this.Stream, this.LeaveOpen, this.Logger) = (Stream, LeaveOpen, Logger ?? NullLogger<ChunkReader>.Instance);
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
            ReadBody(Chunk);
            return Chunk;
        }
        ChunkHeader ReadHeader()
        {
            var headerSize = Marshal.SizeOf<ChunkHeader>();
            Logger.LogTrace("headerSize:{size}", headerSize);
            var buffer = ArrayPool<byte>.Shared.Rent(headerSize);
            try
            {
                var span = buffer.AsSpan();
                var readed = Stream.Read(span);
                Logger.LogDebug("Stream.Read() -> readed:{readed}", readed);
                Debug.Assert(readed != headerSize, "header size invalid.");
                var header = MemoryMarshal.Read<ChunkHeader>(span);
                Logger.LogTrace("-> {header}", header.GetDebuggerDisplay());
                return header;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
        void ReadHeader(Chunk Chunk)
        {
            var Offset = Stream.Position;
            var Header = ReadHeader();
            Chunk.Offset = Offset;
            Chunk.Header = Header;
        }
        void ReadBody(Chunk Chunk)
        {
            if (Chunk.Header is { Type: ChunkType.EndOfFile } or { Length: 0 } or { Entry: 0 })
                return;
            var Entry = Chunk.Header.Entry;
            var Length = Chunk.Header.Length;
            var Size = Marshal.SizeOf<ChunkHeader>();
            Logger.LogTrace("usable body size Length:{Length} - HeaderSize:{HeaderSize} -> {Size}", Length, Size, Length - Size);
            //timeOffsetのリストを生成
            switch (Chunk.Header.Type)
            {
                //case ChunkType.EndOfFile:
                //    return;
                default:
                case ChunkType.Tempo_TFPS_Config:
                case ChunkType.Bigin_Finish_Config:
                case ChunkType.StepData:
                    {
                        var _Size = Size;
                        Size += Entry * sizeof(uint);
                        Debug.Assert(Size <= Length, $"over size exception.Size:{_Size} -> {Size} Length:{Length}");
                        using var Reader = new BinaryReader(Stream, Encoding.UTF8, true);
                        var TimeOffsets = new uint[Entry];
                        for (var i = 0; i < Entry; i++)
                        {
                            TimeOffsets[i] = Reader.ReadUInt32();
                        }
                        Chunk.TimeOffsets = TimeOffsets;
                        Logger.LogTrace(nameof(TimeOffsets) + " [{TimeOffsets}]", new JoinFormatter(", ", TimeOffsets.Select(v => $"{v:X8}")));
                    }
                    break;
            }
            switch (Chunk.Header.Type)
            {
                //case ChunkType.EndOfFile:
                //    return;
                case ChunkType.Tempo_TFPS_Config:
                    {
                        var _Size = Size;
                        Size += Entry * sizeof(int);
                        Debug.Assert(Size <= Length, $"over size exception.Size:{_Size} -> {Size} Length:{Length}");
                        using var Reader = new BinaryReader(Stream, Encoding.UTF8, true);
                        var Tempo_TFPS_Config = new int[Entry];
                        for (var i = 0; i < Entry; i++)
                        {
                            Tempo_TFPS_Config[i] = Reader.ReadInt32();
                        }
                        Chunk.Tempo_TFPS_Config = Tempo_TFPS_Config;
                        Logger.LogTrace(nameof(Tempo_TFPS_Config) + " [{Tempo_TFPS_Config}]", new JoinFormatter(", ", Tempo_TFPS_Config.Select(v => $"{v:X8}")));
                    }
                    break;
                case ChunkType.Bigin_Finish_Config:
                    {
                        var _Size = Size;
                        Size += Entry * sizeof(short);
                        Debug.Assert(Size <= Length, $"over size exception.Size:{_Size} -> {Size} Length:{Length}");
                        using var Reader = new BinaryReader(Stream, Encoding.UTF8, true);
                        var Bigin_Finish_Config = new short[Entry];
                        for (var i = 0; i < Entry; i++)
                        {
                            Bigin_Finish_Config[i] = Reader.ReadInt16();
                        }
                        Chunk.Bigin_Finish_Config = Bigin_Finish_Config;
                        Logger.LogTrace(nameof(Bigin_Finish_Config) + " [{Bigin_Finish_Config}]", new JoinFormatter(", ", Bigin_Finish_Config.Select(v => $"{v:X4}")));
                    }
                    break;
                case ChunkType.StepData:
                    {
                        var _Size = Size;
                        Size += Entry * sizeof(byte);
                        Debug.Assert(Size <= Length, $"over size exception.Size:{_Size} -> {Size} Length:{Length}");
                        using var Reader = new BinaryReader(Stream, Encoding.UTF8, true);
                        var StepData = new byte[Entry];
                        for (var i = 0; i < Entry; i++)
                        {
                            StepData[i] = Reader.ReadByte();
                        }
                        Chunk.StepData = StepData;
                        Logger.LogTrace(nameof(StepData) + " [{StepData}]", new JoinFormatter(", ", StepData.Select(v => $"{v:X2}")));
                    }
                    break;
                default:
                    {
                        var _Size = Size;
                        Size += Entry * sizeof(byte);
                        Debug.Assert(Size <= Length, $"over size exception.Size:{_Size} -> {Size} Length:{Length}");
                        using var Reader = new BinaryReader(Stream, Encoding.UTF8, true);
                        var OtherData = new byte[Entry];
                        for (var i = 0; i < Entry; i++)
                        {
                            OtherData[i] = Reader.ReadByte();
                        }
                        Chunk.OtherData = OtherData;
                        Logger.LogTrace(nameof(OtherData) + " [{OtherData}]", new JoinFormatter(", ", OtherData.Select(v => $"{v:X2}")));
                    }
                    break;
            }
        }
        public void Dispose()
        {
            if (!LeaveOpen)
                Stream.Dispose();
        }
    }
}
