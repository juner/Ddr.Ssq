using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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
                // EndOfFile Data
                yield return WriteAndReadTest(new ChunkHeader(Length: 0), new EmptyBody());
                {
                    // TempoTFPSConfig
                    var Body = new TempoTFPSConfigBody { TimeOffsets = new[] { 0x0, 0x35000, }, Values = new[] { 0x0, 0x373C, }, };
                    var Header = new ChunkHeader(Length: 28, Type: ChunkType.TempoTFPSConfig, Param: 0x0096, Entry: 0x2);
                    yield return WriteAndReadTest(Header, Body);
                }
                {
                    // BiginFinishConfig
                    var Body = new BiginFinishConfigBody { TimeOffsets = new int[] { 0x00035000 }, Values = new[] { BiginFinishConfigType.BufferLength } };
                    var Header = new ChunkHeader(Length: Marshal.SizeOf<ChunkHeader>() + Body.Size(), Type: ChunkType.BiginFinishConfig, Param: 0x0001, Entry: 0x01);
                    yield return WriteAndReadTest(Header, Body);
                }
                {
                    // StepData
                    var Body = new StepDataBody { TimeOffsets = new[] { 0x9800, 0x9C00, 0xA000, }, Values = new byte[] { 0x10, 0x18, 0x40, } };
                    var Header = new ChunkHeader(Length: Marshal.SizeOf<ChunkHeader>() + Body.Size(), Type: ChunkType.StepData, Param: 0x0118, Entry: 0x3);
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
            if ((Body, ActualBody) is (ITimeOffsetBody TimeOffsetBody, ITimeOffsetBody ActualTimeOffsetBody))
                CollectionAssert.AreEqual(TimeOffsetBody.TimeOffsets, ActualTimeOffsetBody.TimeOffsets, nameof(ITimeOffsetBody) + "." + nameof(ITimeOffsetBody.TimeOffsets));
            switch (Header.Type, Body, ActualBody)
            {
                case (ChunkType.EndOfFile, EmptyBody _, EmptyBody _):
                    break;
                case (ChunkType.BiginFinishConfig, BiginFinishConfigBody BFCBody, BiginFinishConfigBody ActualBFCBody):
                    CollectionAssert.AreEqual(BFCBody.Values, ActualBFCBody.Values, nameof(BiginFinishConfigBody) + "." + nameof(BiginFinishConfigBody.Values));
                    break;
                case (ChunkType.TempoTFPSConfig, TempoTFPSConfigBody TTFPSCBody, TempoTFPSConfigBody ActualTTFPSCBody):
                    CollectionAssert.AreEqual(TTFPSCBody.Values, ActualTTFPSCBody.Values, nameof(BiginFinishConfigBody) + "." + nameof(BiginFinishConfigBody.Values));
                    break;
                case (ChunkType.StepData, StepDataBody StepDataBody, StepDataBody ActualStepDataBody):
                    CollectionAssert.AreEqual(StepDataBody.Values, ActualStepDataBody.Values, nameof(BiginFinishConfigBody) + "." + nameof(BiginFinishConfigBody.Values));
                    break;
                case (_, OtherBody, OtherBody):
                    break;
                default:
                    Assert.Fail();
                    break;
            }
            if ((Body, ActualBody) is (IOtherDataBody OtherDataBody, IOtherDataBody ActualOtherDataBody))
                CollectionAssert.AreEqual(OtherDataBody.Values, ActualOtherDataBody.Values, nameof(IOtherDataBody) + "." + nameof(IOtherDataBody.Values));

        }
        [TestMethod, DynamicData(nameof(WriteAndReadTestData))]
        public async Task WriteAndReadAsyncTest(ChunkHeader Header, IBody Body)
        {
            Chunk Chunk;
            using var Stream = new MemoryStream();
            {
                using var Writer = new ChunkWriter(Stream, LeaveOpen: true);
                await Writer.WriteChunkAsync(Header, Body);
                Stream.Seek(0, SeekOrigin.Begin);
            }
            {
                using var Reader = new ChunkReader(Stream, LeaveOpen: true);
                Chunk = await Reader.ReadChunkAsync();
            }
            var (Offset, ActualHeaer, ActualBody) = Chunk;
            Assert.AreEqual(0, Offset, nameof(Offset));
            Assert.AreEqual(Header, ActualHeaer, nameof(ChunkHeader));
            Assert.IsInstanceOfType(ActualBody, Body.GetType());
            if ((Body, ActualBody) is (ITimeOffsetBody TimeOffsetBody, ITimeOffsetBody ActualTimeOffsetBody))
                CollectionAssert.AreEqual(TimeOffsetBody.TimeOffsets, ActualTimeOffsetBody.TimeOffsets, nameof(ITimeOffsetBody) + "." + nameof(ITimeOffsetBody.TimeOffsets));
            switch (Header.Type, Body, ActualBody)
            {
                case (ChunkType.EndOfFile, EmptyBody _, EmptyBody _):
                    break;
                case (ChunkType.BiginFinishConfig, BiginFinishConfigBody BFCBody, BiginFinishConfigBody ActualBFCBody):
                    CollectionAssert.AreEqual(BFCBody.Values, ActualBFCBody.Values, nameof(BiginFinishConfigBody) + "." + nameof(BiginFinishConfigBody.Values));
                    break;
                case (ChunkType.TempoTFPSConfig, TempoTFPSConfigBody TTFPSCBody, TempoTFPSConfigBody ActualTTFPSCBody):
                    CollectionAssert.AreEqual(TTFPSCBody.Values, ActualTTFPSCBody.Values, nameof(BiginFinishConfigBody) + "." + nameof(BiginFinishConfigBody.Values));
                    break;
                case (ChunkType.StepData, StepDataBody StepDataBody, StepDataBody ActualStepDataBody):
                    CollectionAssert.AreEqual(StepDataBody.Values, ActualStepDataBody.Values, nameof(BiginFinishConfigBody) + "." + nameof(BiginFinishConfigBody.Values));
                    break;
                case (_, OtherBody, OtherBody):
                    break;
                default:
                    Assert.Fail();
                    break;
            }
            if ((Body, ActualBody) is (IOtherDataBody OtherDataBody, IOtherDataBody ActualOtherDataBody))
                CollectionAssert.AreEqual(OtherDataBody.Values, ActualOtherDataBody.Values, nameof(IOtherDataBody) + "." + nameof(IOtherDataBody.Values));

        }
    }
}
