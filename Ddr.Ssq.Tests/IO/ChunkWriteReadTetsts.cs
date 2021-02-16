using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ddr.Ssq.IO.Tests
{
    [TestClass]
    public class ChunkWriteReadTetsts
    {
        static IEnumerable<object?[]> WriteAndReadTestData
        {
            get
            {
                yield return WriteAndReadTest(new ChunkHeader(Length: 0), new EmptyBody());
                {
                    var Body = new BiginFinishConfigBody { TimeOffsets = new int[] { 0x00035000 }, Values = new[] { BiginFinishConfigType.BufferLength } };
                    var Header = new ChunkHeader(Length: Marshal.SizeOf<ChunkHeader>() + Body.Size(), Type: ChunkType.BiginFinishConfig, Param: 0x0001, Entry: 0x01);
                    yield return WriteAndReadTest(Header, Body);
                }
                static object?[] WriteAndReadTest(ChunkHeader Header, IBody Body)
                    => new object?[] { Header, Body };
            }
        }
        [TestMethod, DynamicData(nameof(WriteAndReadTestData))]
        public void WriteAndReadTest(ChunkHeader Header, IBody Body)
        {
            Chunk Chunk;
            using var Stream = new MemoryStream();
            {
                using var Writer = new ChunkWriter(Stream, LeaveOpen: true);
                Writer.WriteChunk(Header, Body);
                Stream.Seek(0, SeekOrigin.Begin);
            }
            {
                using var Reader = new ChunkReader(Stream, LeaveOpen: true);
                Chunk = Reader.ReadChunk();
            }
            var (Offset, ActualHeaer, ActualBody) = Chunk;
            Assert.AreEqual(0, Offset, nameof(Offset));
            Assert.AreEqual(Header, ActualHeaer, nameof(ChunkHeader));
            Assert.IsInstanceOfType(ActualBody, Body.GetType());
            switch (Header.Type, Body, ActualBody)
            {
                case (ChunkType.EndOfFile, EmptyBody _, EmptyBody _):
                    break;
                case (ChunkType.BiginFinishConfig, BiginFinishConfigBody BFCBody, BiginFinishConfigBody ActualBFCBody):
                    CollectionAssert.AreEqual(BFCBody.TimeOffsets, ActualBFCBody.TimeOffsets, nameof(BiginFinishConfigBody) + "." + nameof(BiginFinishConfigBody.TimeOffsets));
                    CollectionAssert.AreEqual(BFCBody.Values, ActualBFCBody.Values, nameof(BiginFinishConfigBody) + "." + nameof(BiginFinishConfigBody.Values));
                    CollectionAssert.AreEqual(((IOtherDataBody)BFCBody).Values, ((IOtherDataBody)ActualBFCBody).Values, nameof(IOtherDataBody) + "." + nameof(BiginFinishConfigBody.Values));
                    break;
                default:
                case (ChunkType.TempoTFPSConfig, TempoTFPSConfigBody TTFPSCBody, TempoTFPSConfigBody ActualTTFPSCBody):
                    Assert.Fail();
                    break;
            }
        }
    }
}
