using System;
using System.Collections.Generic;
using System.Linq;

namespace Ddr.Ssq
{
    public class TempoTFPSConfigBody : ITimeOffsetBody, IOtherDataBody
    {
        public int[] TimeOffsets { get; set; } = Array.Empty<int>();
        public int[] Values { get; set; } = Array.Empty<int>();
        byte[] IOtherDataBody.Values { get; set; } = Array.Empty<byte>();
        public LinkedList<TempoTFPSConfigEntry> GetEntries()
            => new(TimeOffsets.Zip(Values).Select(v => new TempoTFPSConfigEntry(v.First, v.Second)));
        public void SetEntries(LinkedList<TempoTFPSConfigEntry> Entries)
        {
            var TimeOffsets = new int[Entries.Count];
            var Values = new int[Entries.Count];
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
        public int Size() => TimeOffsets.Length * sizeof(int) + Values.Length * sizeof(int) + ((IOtherDataBody)this).Values.Length * sizeof(byte);
    }
    public class TempoTFPSConfigEntry
    {
        public TempoTFPSConfigEntry() { }
        public TempoTFPSConfigEntry(int TimeOffset, int Value) => (this.TimeOffset, this.Value) = (TimeOffset, Value);
        public int TimeOffset { get; set; }
        public int Value { get; set; }
    }
}
