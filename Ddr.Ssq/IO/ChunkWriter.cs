using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ddr.Ssq.IO
{
    public class ChunkWriter : IDisposable
    {

        readonly Stream Stream;
        readonly bool LeaveOpen;
        readonly MemoryPool<byte> Pool;
        public ILogger<ChunkWriter> Logger { get; init; } = NullLogger<ChunkWriter>.Instance;
        public ChunkWriter(Stream Stream) : this(Stream, false, default!, default!) { }
        public ChunkWriter(Stream Stream, bool LeaveOpen) : this(Stream, LeaveOpen, default!, default!) { }
        public ChunkWriter(Stream Stream, bool LeaveOpen, MemoryPool<byte> Pool, ILogger<ChunkWriter> Logger)
            => (this.Stream, this.LeaveOpen, this.Pool, this.Logger) = (Stream, LeaveOpen, Pool ?? MemoryPool<byte>.Shared, Logger ?? NullLogger<ChunkWriter>.Instance);
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
        static void SetZero(Span<byte> span)
        {
            foreach (ref byte s in span)
                s = 0;
        }
        public void WriteToEnd(IEnumerable<Chunk> Chunks)
        {
            foreach (var (Header, Body) in Chunks)
            {
                WriteChunk(Header, Body);
            }
        }
        public void WriteChunk(ChunkHeader Header, IBody Body)
        {
            var Length = Header.Length;
            Debug.Assert((Header.Type is ChunkType.EndOfFile && Length == 0) || Length == HeaderSize + Body.Size());
            if (Length == 0)
                Length = sizeof(int);
            using var Owner = Pool.Rent(Length);
            var Span = Owner.Memory[..Length].Span;
            switch ((Header.Type, Body))
            {
                case (ChunkType.EndOfFile, _):
                    {
                        SetZero(Span);
                    }
                    break;
                case (ChunkType.TempoTFPSConfig, TempoTFPSConfigBody TempoTFPSConfigBody):
                    {
                        Write(Span, Header);
                        Write(Span[HeaderSize..], TempoTFPSConfigBody);
                    }
                    break;
                case (ChunkType.BiginFinishConfig, BiginFinishConfigBody BiginFinishConfigBody):
                    {
                        Write(Span, Header);
                        Write(Span[HeaderSize..], BiginFinishConfigBody);
                    }
                    break;
                case (ChunkType.StepData, StepDataBody StepDataBody):
                    {
                        Write(Span, Header);
                        Write(Span[HeaderSize..], StepDataBody);
                    }
                    break;
                case (_, OtherBody OtherBody):
                    {
                        Write(Span, Header);
                        Write(Span[HeaderSize..], OtherBody);
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
            Stream.Write(Span);
        }
        void Write(Span<byte> Span, ChunkHeader Header)
        {
            MemoryMarshal.Write(Span, ref Header);
        }
        void Write(Span<byte> Span, TempoTFPSConfigBody Body)
        {
            var Length = Span.Length;
            var TimeOffsets = MemoryMarshal.Cast<int, byte>(Body.TimeOffsets);
            var Values = MemoryMarshal.Cast<int, byte>(Body.Values);
            Debug.Assert(Length == TimeOffsets.Length + Values.Length);
            TimeOffsets.CopyTo(Span);
            Span = Span[TimeOffsets.Length..];
            Values.CopyTo(Span);
            if (Body is IOtherDataBody OtherBody && OtherBody.Values.Length > 0)
            {
                Span = Span[Values.Length..];
                OtherBody.Values.CopyTo(Span);
            }
        }
        void Write(Span<byte> Span, BiginFinishConfigBody Body)
        {
            var Length = Span.Length;
            var TimeOffsets = MemoryMarshal.Cast<int, byte>(Body.TimeOffsets);
            var Values = MemoryMarshal.Cast<BiginFinishConfigType, byte>(Body.Values);
            Debug.Assert(Length == TimeOffsets.Length + Values.Length);
            TimeOffsets.CopyTo(Span);
            Span = Span[TimeOffsets.Length..];
            Values.CopyTo(Span);
            if (Body is IOtherDataBody OtherBody && OtherBody.Values.Length > 0)
            {
                Span = Span[Values.Length..];
                OtherBody.Values.CopyTo(Span);
            }
        }
        void Write(Span<byte> Span, StepDataBody Body)
        {
            var Length = Span.Length;
            var TimeOffsets = MemoryMarshal.Cast<int, byte>(Body.TimeOffsets);
            var Values = Body.Values.AsSpan();
            Debug.Assert(Length == TimeOffsets.Length + Values.Length);
            TimeOffsets.CopyTo(Span);
            Span = Span[TimeOffsets.Length..];
            Values.CopyTo(Span);
            if (Body is IOtherDataBody OtherBody && OtherBody.Values.Length > 0)
            {
                Span = Span[Values.Length..];
                OtherBody.Values.CopyTo(Span);
            }

        }
        void Write(Span<byte> Span, OtherBody Body)
        {
            var Length = Span.Length;
            var OtherData = Body.Values.AsSpan();
            Debug.Assert(Length == OtherData.Length);
            OtherData.CopyTo(Span);
        }
        public void Dispose()
        {
            if (LeaveOpen)
                return;
            Stream.Dispose();
        }
    }
}
