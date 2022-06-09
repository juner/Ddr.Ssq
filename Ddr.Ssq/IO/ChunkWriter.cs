using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ddr.Ssq.IO;

/// <summary>
/// Chunk Writer
/// </summary>
public class ChunkWriter : IDisposable, IAsyncDisposable
{

    readonly Stream Stream;
    readonly bool LeaveOpen;
    readonly MemoryPool<byte> Pool;
    /// <summary>
    /// Logger
    /// </summary>
    public ILogger<ChunkWriter> Logger { get; init; } = NullLogger<ChunkWriter>.Instance;
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="Stream"></param>
    public ChunkWriter(Stream Stream) : this(Stream, false, default!, default!) { }
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="LeaveOpen"></param>
    public ChunkWriter(Stream Stream, bool LeaveOpen) : this(Stream, LeaveOpen, default!, default!) { }
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="Stream"></param>
    /// <param name="LeaveOpen"></param>
    /// <param name="Pool"></param>
    /// <param name="Logger"></param>
    public ChunkWriter(Stream Stream, bool LeaveOpen, MemoryPool<byte> Pool, ILogger<ChunkWriter> Logger)
        => (this.Stream, this.LeaveOpen, this.Pool, this.Logger) = (Stream, LeaveOpen, Pool ?? MemoryPool<byte>.Shared, Logger ?? NullLogger<ChunkWriter>.Instance);
    static int headerSize;
    private bool disposedValue;

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
    /// <summary>
    /// Write All Chunk
    /// </summary>
    /// <param name="Chunks"></param>
    public void WriteToEnd(IEnumerable<Chunk> Chunks)
    {
        foreach (var (Header, Body) in Chunks)
        {
            WriteChunk(Header, Body);
        }
    }
    /// <summary>
    /// Write All Chunk Async
    /// </summary>
    /// <param name="Chunks"></param>
    /// <param name="Token"></param>
    /// <returns></returns>
    public async ValueTask WriteToEndAsync(IEnumerable<Chunk> Chunks, CancellationToken Token = default)
    {
        foreach (var (Header, Body) in Chunks)
        {
            await WriteChunkAsync(Header, Body, Token);
        }
    }
    /// <summary>
    /// Write Chunk
    /// </summary>
    /// <param name="Header"></param>
    /// <param name="Body"></param>
    public void WriteChunk(in ChunkHeader Header, IBody Body)
    {
        var Length = Header.Length;
        Debug.Assert((Header.Type is ChunkType.EndOfFile && Length == 0) || Length == HeaderSize + Body.Size());
        if (Length == 0)
            Length = sizeof(int);
        using var Owner = Pool.Rent(Length);
        var Span = Owner.Memory[..Length].Span;
        InnerWrite(Span, Header, Body);
        Stream.Write(Span);
    }
    /// <summary>
    /// Write Chunk Async
    /// </summary>
    /// <param name="Header"></param>
    /// <param name="Body"></param>
    /// <param name="Token"></param>
    /// <returns></returns>
    public ValueTask WriteChunkAsync(in ChunkHeader Header, IBody Body, CancellationToken Token = default)
    {
        var Length = Header.Length;
        Debug.Assert((Header.Type is ChunkType.EndOfFile && Length == 0) || Length == HeaderSize + Body.Size());
        if (Length == 0)
            Length = sizeof(int);
        using var Owner = Pool.Rent(Length);
        var Memory = Owner.Memory[..Length];
        InnerWrite(Memory.Span, Header, Body);
        return Stream.WriteAsync(Memory, Token);
    }
    static void InnerWrite(Span<byte> Span, ChunkHeader Header, IBody Body)
    {
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
    }
    static void Write(Span<byte> Span, ChunkHeader Header)
    {
        MemoryMarshal.Write(Span, ref Header);
    }
    static void Write(Span<byte> Span, TempoTFPSConfigBody Body)
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
    static void Write(Span<byte> Span, BiginFinishConfigBody Body)
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
    static void Write(Span<byte> Span, StepDataBody Body)
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
    static void Write(Span<byte> Span, OtherBody Body)
    {
        var Length = Span.Length;
        var OtherData = Body.Values.AsSpan();
        Debug.Assert(Length == OtherData.Length);
        OtherData.CopyTo(Span);
    }
    /// <summary>
    /// Dispose
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (LeaveOpen)
                    return;
                Stream.Dispose();
            }
            disposedValue = true;
        }
    }
    /// <summary>
    /// destructor
    /// </summary>
    ~ChunkWriter()
    {
        // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        Dispose(disposing: false);
    }
    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    /// <summary>
    /// Dispose Async Core
    /// </summary>
    /// <returns></returns>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (!disposedValue)
        {
            if (!LeaveOpen)
                await Stream.DisposeAsync();
            disposedValue = true;
        }
    }
    /// <summary>
    /// Dispose Async
    /// </summary>
    /// <returns></returns>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(disposing: false);
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
        // Suppress finalization.
        GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
    }
}
