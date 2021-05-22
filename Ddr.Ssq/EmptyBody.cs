using System;

namespace Ddr.Ssq
{
    /// <summary>
    /// Empty Body
    /// </summary>
    public class EmptyBody : IBody, IOtherDataBody
    {
        byte[] IOtherDataBody.Values { get; set; } = Array.Empty<byte>();
        /// <inheritdoc/>
        public int Size() => ((IOtherDataBody)this).Values.Length * sizeof(byte);
    }
}
