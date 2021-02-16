using System;

namespace Ddr.Ssq
{
    public class EmptyBody : IBody, IOtherDataBody
    {
        byte[] IOtherDataBody.Values { get; set; } = Array.Empty<byte>();

        public int Size() => ((IOtherDataBody)this).Values.Length * sizeof(byte);
    }
}
