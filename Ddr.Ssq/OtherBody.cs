using System;

namespace Ddr.Ssq
{
    public class OtherBody : IBody,IOtherDataBody
    {
        public byte[] OtherData { get; set; } = Array.Empty<byte>();
    }
}
