using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Visual;
using Xunit;

namespace Repkarbinhake.Core.Tests
{
    public class RopeSegmentationTests
    {
        private readonly SegmentSettings _settings = new SegmentSettings();
        private readonly RopeSegmentation _segmentation;

        public RopeSegmentationTests()
        {
            _segmentation = new RopeSegmentation(_settings);
        }

        [Fact]
        public void ShortRope_GetsOnlyBonesItNeeds()
        {
            Assert.Equal(10, _segmentation.Count(3f, 1));
        }

        [Fact]
        public void TinyRope_NeverGoesBelowMinimum()
        {
            Assert.Equal(RopeSegmentation.MinSegments, _segmentation.Count(0.1f, 1));
        }

        [Fact]
        public void Auto_FewRopes_AreCappedByAutoMax()
        {
            Assert.Equal(40, _segmentation.Count(14f, 1));
        }

        [Fact]
        public void Auto_ManyRopes_ShareBudget()
        {
            Assert.Equal(24, _segmentation.Count(14f, 10));
        }

        [Fact]
        public void Auto_MoreRopes_NeverIncreaseBones()
        {
            int previous = int.MaxValue;

            for (int ropes = 1; ropes <= 100; ropes++)
            {
                int count = _segmentation.Count(14f, ropes);
                Assert.True(count <= previous);
                previous = count;
            }
        }

        [Fact]
        public void Auto_HugeLobby_KeepsMinimum()
        {
            Assert.Equal(RopeSegmentation.MinSegments, _segmentation.Count(14f, 500));
        }

        [Fact]
        public void Auto_ZeroRopes_IsTreatedAsOne()
        {
            Assert.Equal(_segmentation.Cap(1), _segmentation.Cap(0));
        }

        [Fact]
        public void Fixed_IgnoresRopeCount()
        {
            _settings.SegmentMode = SegmentMode.Fixed;
            _settings.FixedSegments = 12;

            Assert.Equal(12, _segmentation.Count(14f, 1));
            Assert.Equal(12, _segmentation.Count(14f, 50));
        }

        [Fact]
        public void Fixed_BelowMinimum_IsRaisedToMinimum()
        {
            _settings.SegmentMode = SegmentMode.Fixed;
            _settings.FixedSegments = 1;

            Assert.Equal(RopeSegmentation.MinSegments, _segmentation.Count(14f, 1));
        }

        private sealed class SegmentSettings : ISegmentSettings
        {
            public SegmentMode SegmentMode { get; set; } = SegmentMode.Auto;
            public int FixedSegments { get; set; } = 24;
            public int SegmentBudget { get; set; } = 240;
            public int AutoMaxSegments { get; set; } = 40;
            public float SegmentLength { get; set; } = 0.3f;
        }
    }
}
