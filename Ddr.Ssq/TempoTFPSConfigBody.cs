using System;
using System.Collections.Generic;
using System.Linq;

namespace Ddr.Ssq;

/// <summary>
/// Tempo TfPS Comfig Body
/// </summary>
public class TempoTFPSConfigBody : ITimeOffsetBody, IOtherDataBody
{
    /// <summary>
    /// Time Offsets
    /// </summary>
    public int[] TimeOffsets { get; set; } = Array.Empty<int>();
    /// <summary>
    /// Values
    /// </summary>
    public int[] Values { get; set; } = Array.Empty<int>();
    byte[] IOtherDataBody.Values { get; set; } = Array.Empty<byte>();
    /// <summary>
    /// Get Entries
    /// </summary>
    /// <returns></returns>
    public LinkedList<TempoTFPSConfigEntry> GetEntries()
        => new(TimeOffsets.Zip(Values).Select(v => new TempoTFPSConfigEntry(v.First, v.Second)));
    /// <summary>
    /// Set Entries
    /// </summary>
    /// <param name="Entries"></param>
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
    /// <summary>
    /// Get Body Size
    /// </summary>
    /// <returns></returns>
    public int Size() => TimeOffsets.Length * sizeof(int) + Values.Length * sizeof(int) + ((IOtherDataBody)this).Values.Length * sizeof(byte);
}
/// <summary>
/// Tempo TfPS Comfig Entry
/// </summary>
public class TempoTFPSConfigEntry
{
    /// <summary>
    /// constructor
    /// </summary>
    public TempoTFPSConfigEntry() { }
    /// <summary>
    /// construtor
    /// </summary>
    /// <param name="TimeOffset"></param>
    /// <param name="Value"></param>
    public TempoTFPSConfigEntry(int TimeOffset, int Value) => (this.TimeOffset, this.Value) = (TimeOffset, Value);
    /// <summary>
    /// Time Offset
    /// </summary>
    public int TimeOffset { get; set; }
    /// <summary>
    /// Value
    /// </summary>
    public int Value { get; set; }
}
