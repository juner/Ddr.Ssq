using System;

namespace Ddr.Ssq;

/// <summary>
/// Other Body
/// </summary>
public class OtherBody : IBody, IOtherDataBody
{
    /// <summary>
    /// Values
    /// </summary>
    public byte[] Values { get; set; } = Array.Empty<byte>();
    /// <inheritdoc/>
    public int Size() => Values.Length * sizeof(byte);
}
