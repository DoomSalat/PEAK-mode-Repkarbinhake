using System.Collections.Generic;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Xunit;

namespace Repkarbinhake.Core.Tests
{
    public class DeathUntieRuleTests
    {
        private const int Local = 1;
        private const int Partner = 2;
        private const int Third = 3;

        private readonly FakeLives _lives = new FakeLives();
        private readonly DeathUntieRule _rule;
        private readonly List<RopeLink> _result = new List<RopeLink>();
        private readonly List<RopeLink> _links = new List<RopeLink>
        {
            new RopeLink(Local, Partner, 1L),
            new RopeLink(Partner, Third, 2L),
            new RopeLink(Local, Third, 3L),
        };

        public DeathUntieRuleTests()
        {
            _rule = new DeathUntieRule(_lives);
        }

        [Fact]
        public void NobodyDead_NothingToUntie()
        {
            _rule.Collect(_links, Local, isHost: true, _result);

            Assert.Empty(_result);
        }

        [Fact]
        public void LocalPlayerDies_UntiesAllOwnLinks()
        {
            _lives.Kill(Local);

            _rule.Collect(_links, Local, isHost: false, _result);

            Assert.Equal(2, _result.Count);
            Assert.All(_result, link => Assert.True(link.Contains(Local)));
        }

        [Fact]
        public void RemotePlayerDies_NonHostLeavesItToOwner()
        {
            _lives.Kill(Partner);

            _rule.Collect(_links, Local, isHost: false, _result);

            Assert.Empty(_result);
        }

        [Fact]
        public void RemotePlayerDies_HostUntiesAsFallback()
        {
            _lives.Kill(Partner);

            _rule.Collect(_links, Local, isHost: true, _result);

            Assert.Equal(2, _result.Count);
            Assert.All(_result, link => Assert.True(link.Contains(Partner)));
        }

        private sealed class FakeLives : IActorLifeSource
        {
            private readonly HashSet<int> _dead = new HashSet<int>();

            public void Kill(int actor)
            {
                _dead.Add(actor);
            }

            public bool IsDead(int actor)
            {
                return _dead.Contains(actor);
            }
        }
    }
}
