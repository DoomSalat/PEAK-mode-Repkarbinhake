using System.Collections.Generic;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Xunit;

namespace Repkarbinhake.Core.Tests
{
    public class DepartedUntieRuleTests
    {
        private readonly DepartedUntieRule _rule = new DepartedUntieRule();
        private readonly List<RopeLink> _result = new List<RopeLink>();
        private readonly List<RopeLink> _links = new List<RopeLink>
        {
            new RopeLink(1, 2, 1L),
            new RopeLink(2, 3, 2L),
            new RopeLink(1, 3, 3L),
        };

        [Fact]
        public void EveryonePresent_NothingToUntie()
        {
            _rule.Collect(_links, new HashSet<int> { 1, 2, 3 }, _result);

            Assert.Empty(_result);
        }

        [Fact]
        public void DisconnectedPlayer_AllHisLinksAreUntied()
        {
            _rule.Collect(_links, new HashSet<int> { 1, 3 }, _result);

            Assert.Equal(2, _result.Count);
            Assert.All(_result, link => Assert.True(link.Contains(2)));
        }

        [Fact]
        public void LinkBetweenTwoDepartedPlayers_IsUntiedOnce()
        {
            _rule.Collect(_links, new HashSet<int> { 1 }, _result);

            Assert.Equal(3, _result.Count);
        }

        [Fact]
        public void LateJoinerSnapshot_WithLinkOfLongGonePlayer_IsCleaned()
        {
            List<RopeLink> links = new List<RopeLink> { new RopeLink(1, 9, 5L) };

            _rule.Collect(links, new HashSet<int> { 1, 2 }, _result);

            Assert.Single(_result);
        }
    }
}
