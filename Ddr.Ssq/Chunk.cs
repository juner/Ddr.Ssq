using System.IO;

namespace Ddr.Ssq;

/// <summary>
/// Chunk 
/// </summary>
public class Chunk
{
    /// <summary>
    /// <see cref="Stream.Position"/>
    /// </summary>
    public long Offset { get; set; }
    /// <summary>
    /// Chunk Header
    /// </summary>
    public ChunkHeader Header { get; set; }
    /// <summary>
    /// Chunk Body
    /// </summary>
    public IBody Body { get; set; } = default!;
}
/// <summary>
/// Chunk Extensions
/// </summary>
public static class ChunkExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Chunk"></param>
    /// <param name="Offset"></param>
    /// <param name="Header"></param>
    /// <param name="Body"></param>
    public static void Deconstruct(this Chunk Chunk, out long Offset, out ChunkHeader Header, out IBody Body)
        => (Offset, Header, Body) = (Chunk.Offset, Chunk.Header, Chunk.Body);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Chunk"></param>
    /// <param name="Header"></param>
    /// <param name="Body"></param>
    public static void Deconstruct(this Chunk Chunk, out ChunkHeader Header, out IBody Body)
        => (Header, Body) = (Chunk.Header, Chunk.Body);
}
