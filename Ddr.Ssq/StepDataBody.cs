using System;
using System.Collections.Generic;
using System.Linq;

namespace Ddr.Ssq;

/// <summary>
/// Step Data Body
/// </summary>
public class StepDataBody : ITimeOffsetBody, IOtherDataBody
{
    /// <inheritdoc/>
    public int[] TimeOffsets { get; set; } = Array.Empty<int>();
    /// <summary>
    /// Values
    /// </summary>
    public byte[] Values { get; set; } = Array.Empty<byte>();
    byte[] IOtherDataBody.Values { get; set; } = Array.Empty<byte>();
    /// <summary>
    /// get entries
    /// </summary>
    /// <returns></returns>
    public LinkedList<StepDataEntry> GetEntries()
        => new(TimeOffsets.Zip(Values).Select(v => new StepDataEntry(v.First, v.Second)));
    /// <summary>
    /// set entries
    /// </summary>
    /// <param name="Entries"></param>
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
    /// <inheritdoc/>
    public int Size() => TimeOffsets.Length * sizeof(int) + Values.Length * sizeof(byte) + ((IOtherDataBody)this).Values.Length * sizeof(byte);
}
/// <summary>
/// Step Data Entry
/// </summary>
public class StepDataEntry
{
    /// <summary>
    /// constructor
    /// </summary>
    public StepDataEntry() { }
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="TimeOffset"></param>
    /// <param name="Value"></param>
    public StepDataEntry(int TimeOffset, byte Value) => (this.TimeOffset, this.Value) = (TimeOffset, Value);
    /// <summary>
    /// TimeOffset
    /// </summary>
    public int TimeOffset { get; set; }
    /// <summary>
    /// Value
    /// </summary>
    public byte Value { get; set; }
}
