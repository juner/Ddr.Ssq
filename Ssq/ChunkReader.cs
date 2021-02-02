using System;
using System.IO;

namespace Ssq
{
    public class ChunkReader : IDisposable
    {
        Stream Stream;
        public ChunkReader(Stream Stream)
            => this.Stream = Stream;
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
