using System;
using System.Collections.Generic;
using System.Linq;

namespace Ddr.Ssq
{
    public class BiginFinishConfigBody : ITimeOffsetBody, IOtherDataBody
    {
        public int[] TimeOffsets { get; set; } = Array.Empty<int>();
        public BiginFinishConfigType[] Values { get; set; } = Array.Empty<BiginFinishConfigType>();
        byte[] IOtherDataBody.Values { get; set; } = Array.Empty<byte>();
        public LinkedList<BiginFinishConfigEntry> GetEntries()
            => new(TimeOffsets.Zip(Values).Select(v => new BiginFinishConfigEntry(v.First, v.Second)));
        public void SetEntries(LinkedList<BiginFinishConfigEntry> Entries)
        {
            var TimeOffsets = new int[Entries.Count];
            var Values = new BiginFinishConfigType[Entries.Count];
            var i = 0;
            foreach (var e in Entries)
            {
                var index = i++;
                TimeOffsets[index] = e.TimeOffset;
                Values[index] = e.Value;
            }
            this.TimeOffsets = TimeOffsets;
            this.Values = Values;
        }
        public int Size() => TimeOffsets.Length * sizeof(int) + Values.Length * sizeof(BiginFinishConfigType) + ((IOtherDataBody)this).Values.Length * sizeof(byte);
    }
    public class BiginFinishConfigEntry
    {
        public BiginFinishConfigEntry() { }
        public BiginFinishConfigEntry(int TimeOffset, BiginFinishConfigType Value) => (this.TimeOffset, this.Value) = (TimeOffset, Value);
        public int TimeOffset { get; set; }
        public BiginFinishConfigType Value { get; set; }
    }
}
