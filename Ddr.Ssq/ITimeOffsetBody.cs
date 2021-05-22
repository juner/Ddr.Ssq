namespace Ddr.Ssq
{
    /// <summary>
    /// Time Offset Body Interface
    /// </summary>
    public interface ITimeOffsetBody : IBody
    {
        /// <summary>
        /// TimeOffsets
        /// </summary>
        int[] TimeOffsets { get; set; }
    }
}
