using System;
using System.Collections.Generic;
using System.Linq;

namespace Ddr.Ssq;

/// <summary>
/// Begin/Finish Config Body
/// </summary>
public class BiginFinishConfigBody : ITimeOffsetBody, IOtherDataBody
{
    /// <summary>
    /// Time Offset Array
    /// </summary>
    public int[] TimeOffsets { get; set; } = Array.Empty<int>();
    /// <summary>
    /// Bigin/Finish Config Type Array
    /// </summary>
    public BiginFinishConfigType[] Values { get; set; } = Array.Empty<BiginFinishConfigType>();
    byte[] IOtherDataBody.Values { get; set; } = Array.Empty<byte>();
    /// <summary>
    /// Get Entries from <see cref="TimeOffsets"/> and <see cref="Values"/>
    /// </summary>
    /// <returns></returns>
    public LinkedList<BiginFinishConfigEntry> GetEntries()
        => new(TimeOffsets.Zip(Values).Select(v => new BiginFinishConfigEntry(v.First, v.Second)));
    /// <summary>
    /// Set Entries to <see cref="TimeOffsets"/> and <see cref="Values"/>
    /// </summary>
    /// <param name="Entries"></param>
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
    /// <inheritdoc/>
    public int Size() => TimeOffsets.Length * sizeof(int) + Values.Length * sizeof(BiginFinishConfigType) + ((IOtherDataBody)this).Values.Length * sizeof(byte);
}
/// <summary>
/// Begin/Finish Config Entry
/// </summary>
public class BiginFinishConfigEntry
{
    /// <summary>
    /// Begin/Finish Config Entry constructor
    /// </summary>
    public BiginFinishConfigEntry() { }
    /// <summary>
    /// Begin/Finish Config Entry constructor
    /// </summary>
    /// <param name="TimeOffset"></param>
    /// <param name="Value"></param>
    public BiginFinishConfigEntry(int TimeOffset, BiginFinishConfigType Value) => (this.TimeOffset, this.Value) = (TimeOffset, Value);
    /// <summary>
    /// Begin/Finish Config Entry deconstructor
    /// </summary>
    /// <param name="TimeOffset"></param>
    /// <param name="Value"></param>
    public void Deconstruct(out int TimeOffset, out BiginFinishConfigType Value) => (TimeOffset, Value) = (this.TimeOffset, this.Value);
    /// <summary>
    /// time offset.
    /// </summary>
    public int TimeOffset { get; set; }
    /// <summary>
    /// Bigin/Finish Config Type
    /// </summary>
    public BiginFinishConfigType Value { get; set; }
}
