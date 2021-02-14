using System;

namespace Ddr.Ssq
{
    public class EmptyBody : IBody
    {
        public byte[] OtherData { get; set; } = Array.Empty<byte>();
    }
}
