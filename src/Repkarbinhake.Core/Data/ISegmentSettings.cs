namespace Repkarbinhake.Core.Data
{
    public interface ISegmentSettings
    {
        SegmentMode SegmentMode { get; }
        int FixedSegments { get; }
        int SegmentBudget { get; }
        int AutoMaxSegments { get; }
        float SegmentLength { get; }
    }
}
