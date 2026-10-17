using System;
using Repkarbinhake.Core.Data;

namespace Repkarbinhake.Core.Visual
{
    public sealed class RopeSegmentation
    {
        public const int MinSegments = 4;

        private const float MinSegmentLength = 0.01f;

        private readonly ISegmentSettings _settings;

        public RopeSegmentation(ISegmentSettings settings)
        {
            _settings = settings;
        }

        public int Count(float length, int ropeCount)
        {
            int byLength = (int)MathF.Ceiling(length / MathF.Max(_settings.SegmentLength, MinSegmentLength));
            return Math.Clamp(byLength, MinSegments, Cap(ropeCount));
        }

        public int Cap(int ropeCount)
        {
            if (_settings.SegmentMode == SegmentMode.Fixed)
                return Math.Max(_settings.FixedSegments, MinSegments);

            int share = _settings.SegmentBudget / Math.Max(ropeCount, 1);
            return Math.Clamp(share, MinSegments, Math.Max(_settings.AutoMaxSegments, MinSegments));
        }
    }
}
