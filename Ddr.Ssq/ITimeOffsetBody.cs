namespace Ddr.Ssq
{
    public interface ITimeOffsetBody: IBody
    {
        int[] TimeOffsets { get; set; }
    }
}
