using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ddr.Ssq.IO.Tests
{
    [TestClass()]
    public class ChunkReaderTests
    {
        static IEnumerable<object?[]> ReadHeaderTestData
        {
            get
            {
                yield return ReadHeaderTest(new MemoryStream(new byte[] { 0x00, 0x00, 0x00, 0x00 }, false), new ChunkHeader());
                static object?[] ReadHeaderTest(Stream Stream, ChunkHeader Expected)
                    => new object?[] { Stream, Expected };
            }
        }
        [TestMethod, DynamicData(nameof(ReadHeaderTestData))]
        public void ReadHeaderTest(Stream Stream, ChunkHeader Expected)
        {
            using (Stream)
            {
                using var Reader = new ChunkReader(Stream, true);
                var Header = Reader.ReadHeader();
                Assert.AreEqual(Expected.Length, Header.Length);
                Assert.AreEqual(Expected.Type, Header.Type);
                Assert.AreEqual(Expected.Param, Header.Param);
                Assert.AreEqual(Expected.Entry, Header.Entry);
            }
        }
    }
}
