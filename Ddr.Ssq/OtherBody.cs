using System;

namespace Ddr.Ssq
{
    public class OtherBody : IBody, IOtherDataBody
    {
        public byte[] Values { get; set; } = Array.Empty<byte>();
        public int Size() => Values.Length * sizeof(byte);
    }
}
