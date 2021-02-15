using System;
using System.Collections.Generic;
using System.Linq;

namespace Ddr.Ssq
{
    public class StepDataBody : ITimeOffsetBody, IOtherDataBody
    {
        public int[] TimeOffsets { get; set; } = Array.Empty<int>();
        public byte[] Values { get; set; } = Array.Empty<byte>();
        byte[] IOtherDataBody.Values { get; set; } = Array.Empty<byte>();
        public LinkedList<StepDataEntry> GetEntries()
            => new(TimeOffsets.Zip(Values).Select(v => new StepDataEntry(v.First, v.Second)));
        public void SetEntries(LinkedList<StepDataEntry> Entries)
        {
            var TimeOffsets = new int[Entries.Count];
            var Values = new byte[Entries.Count];
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

        public int Size() => TimeOffsets.Length * sizeof(int) + Values.Length * sizeof(byte) + ((IOtherDataBody)this).Values.Length * sizeof(byte);
    }
    public class StepDataEntry
    {
        public StepDataEntry() { }
        public StepDataEntry(int TimeOffset, byte Value) => (this.TimeOffset, this.Value) = (TimeOffset, Value);
        public int TimeOffset { get; set; }
        public byte Value { get; set; }
    }
}
