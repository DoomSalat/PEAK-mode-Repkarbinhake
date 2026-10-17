using System.Collections.Generic;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Core.Tests.Fakes;
using Xunit;

namespace Repkarbinhake.Core.Tests
{
    public class LinkRegistryTests
    {
        private const int Local = 1;
        private const int Partner = 2;
        private const int Third = 3;

        private readonly FakeTransport _transport = new FakeTransport();
        private readonly LinkRegistry _registry;
        private readonly List<RopeLink> _added = new List<RopeLink>();
        private readonly List<RopeLink> _removed = new List<RopeLink>();
        private int _remoteSequence = 1000;

        public LinkRegistryTests()
        {
            _registry = CreateRegistry(_transport, Local);
        }

        [Fact]
        public void Tie_AddsLinkAndPublishesTieWithOwnTieId()
        {
            _registry.Tie(Local, Partner);

            RopeLink link = Single();
            Assert.Same(link, Assert.Single(_added));
            Assert.True(_transport.Last.State.IsTied);
            Assert.Equal(LinkState.ComposeTieId(Local, _transport.Last.Sequence), link.TieId);
            Assert.Equal(link.TieId, _transport.Last.State.TieId);
            Assert.False(link.IsHeld);
        }

        [Fact]
        public void Untie_RemovesLinkAndPublishesUntied()
        {
            _registry.Tie(Local, Partner);

            _registry.Untie(Single());

            Assert.Empty(_registry.Links);
            Assert.Single(_removed);
            Assert.False(_transport.Last.State.IsTied);
        }

        [Fact]
        public void RemoteTieAndUntie_AreApplied()
        {
            _registry.Receive(RemoteTie(Partner, Third));
            Assert.Single(_added);

            _registry.Receive(RemoteUntie(Partner, Third));
            Assert.Empty(_registry.Links);
            Assert.Single(_removed);
        }

        [Fact]
        public void Grab_PublishesOnlyOwnEnd()
        {
            _registry.Tie(Local, Partner);

            _registry.Grab(Single());

            Assert.True(Single().IsHeldBy(Local));
            Assert.False(Single().IsHeldBy(Partner));
            Assert.Equal(Single().EndKey(Local), _transport.LastEnd.Key);
            Assert.True(_transport.LastEnd.State.IsHeld);
            Assert.Equal(Single().TieId, _transport.LastEnd.State.TieId);
        }

        [Fact]
        public void TryGetHeld_FindsOnlyTheRopeTheActorHolds()
        {
            _registry.Tie(Local, Partner);
            _registry.Tie(Local, Third);
            _registry.TryGet(Local, Third, out RopeLink link);
            _registry.Grab(link);

            Assert.True(_registry.TryGetHeld(Local, out RopeLink held));
            Assert.Same(link, held);
            Assert.False(_registry.TryGetHeld(Partner, out _));
        }

        [Fact]
        public void BothCanHoldTheSameRope()
        {
            _registry.Tie(Local, Partner);
            _registry.Grab(Single());

            _registry.ReceiveEnd(RemoteEnd(Single(), Partner, isHeld: true, reel: 2f));

            Assert.True(Single().IsHeldBy(Local));
            Assert.True(Single().IsHeldBy(Partner));
            Assert.Equal(2f, Single().TotalReel);
        }

        [Fact]
        public void Release_ResetsOwnReel()
        {
            _registry.Tie(Local, Partner);
            _registry.Grab(Single());
            _registry.SetReel(Single(), 5f);

            _registry.Release(Single());

            Assert.False(Single().IsHeldBy(Local));
            Assert.Equal(0f, Single().EndOf(Local).Reel);
            Assert.False(_transport.LastEnd.State.IsHeld);
            Assert.Equal(0f, _transport.LastEnd.State.Reel);
        }

        [Fact]
        public void SetReel_PublishesOwnEnd()
        {
            _registry.Tie(Local, Partner);
            _registry.Grab(Single());

            _registry.SetReel(Single(), 3.5f);

            Assert.Equal(3.5f, _transport.LastEnd.State.Reel);
            Assert.Equal(3.5f, Single().TotalReel);
        }

        [Fact]
        public void ReelLocally_ChangesLengthWithoutPublishing()
        {
            _registry.Tie(Local, Partner);
            _registry.Grab(Single());
            int publishedBefore = _transport.PublishedEnds.Count;

            _registry.ReelLocally(Single(), 2.5f);

            Assert.Equal(2.5f, Single().TotalReel);
            Assert.Equal(publishedBefore, _transport.PublishedEnds.Count);

            _registry.PublishOwnEnd(Single());

            Assert.Equal(2.5f, _transport.LastEnd.State.Reel);
        }

        [Fact]
        public void EchoOfOwnEnd_IsIgnored()
        {
            _registry.Tie(Local, Partner);
            _registry.Grab(Single());
            EndUpdate grab = _transport.LastEnd;
            _registry.Release(Single());

            _registry.ReceiveEnd(grab);

            Assert.False(Single().IsHeldBy(Local));
        }

        [Fact]
        public void EndFromPreviousTie_IsNotApplied()
        {
            _registry.Receive(RemoteTie(Local, Partner));
            RopeLink oldLink = Single();
            EndUpdate oldGrab = RemoteEnd(oldLink, Partner, isHeld: true, reel: 3f);
            _registry.Receive(RemoteUntie(Local, Partner));
            _registry.Receive(RemoteTie(Local, Partner));

            _registry.ReceiveEnd(oldGrab);

            Assert.False(Single().IsHeldBy(Partner));
        }

        [Fact]
        public void EndArrivingBeforeItsTie_IsAppliedWhenTieArrives()
        {
            LinkUpdate tie = RemoteTie(Partner, Third);
            EndUpdate grab = new EndUpdate(LinkKeys.BuildEnd(Partner, Third, Third), new EndState(tie.State.TieId, true, 1.5f));

            _registry.ReceiveEnd(grab);
            _registry.Receive(tie);

            Assert.True(Single().IsHeldBy(Third));
            Assert.Equal(1.5f, Single().TotalReel);
        }

        [Fact]
        public void Retie_ResetsBothEnds()
        {
            _registry.Receive(RemoteTie(Local, Partner));
            _registry.Grab(Single());
            _registry.ReceiveEnd(RemoteEnd(Single(), Partner, isHeld: true, reel: 2f));

            _registry.Receive(RemoteTie(Local, Partner));

            Assert.False(Single().IsHeld);
            Assert.Single(_added);
        }

        [Fact]
        public void StaleOwnTieEcho_AfterUntie_DoesNotResurrectLink()
        {
            _registry.Tie(Local, Partner);
            LinkUpdate tie = _transport.Last;
            _registry.Untie(Single());
            LinkUpdate untie = _transport.Last;

            _registry.Receive(tie);
            Assert.Empty(_registry.Links);

            _registry.Receive(untie);
            Assert.Empty(_registry.Links);
        }

        [Fact]
        public void OtherLinkWrite_OrderedBeforeMyPendingWrite_IsIgnored()
        {
            _registry.Tie(Local, Partner);
            LinkUpdate myTie = _transport.Last;

            _registry.Receive(RemoteUntie(Local, Partner));
            Assert.Single(_registry.Links);

            _registry.Receive(myTie);
            Assert.Single(_registry.Links);
        }

        [Fact]
        public void OtherLinkWrite_AfterMyEcho_IsApplied()
        {
            _registry.Tie(Local, Partner);
            _registry.Receive(_transport.Last);

            _registry.Receive(RemoteUntie(Local, Partner));

            Assert.Empty(_registry.Links);
        }

        [Fact]
        public void SimultaneousTies_ConvergeToLaterServerWrite()
        {
            _registry.Tie(Local, Partner);
            LinkUpdate myTie = _transport.Last;
            LinkUpdate partnerTie = RemoteTie(Local, Partner);

            _registry.Receive(myTie);
            _registry.Receive(partnerTie);

            Assert.Equal(partnerTie.State.TieId, Single().TieId);
        }

        [Fact]
        public void TransportWithoutEchoes_NeverBlocksRemoteWrites()
        {
            LinkRegistry registry = CreateRegistry(new FakeTransport(echoesOwnUpdates: false), Local);
            registry.Tie(Local, Partner);

            registry.Receive(RemoteUntie(Local, Partner));

            Assert.Empty(registry.Links);
        }

        [Fact]
        public void UntieActor_RemovesOnlyLinksOfThatActor()
        {
            _registry.Tie(Local, Partner);
            _registry.Tie(Local, Third);
            _registry.Receive(RemoteTie(Partner, Third));

            _registry.UntieActor(Partner);

            RopeLink remaining = Single();
            Assert.True(remaining.Contains(Local));
            Assert.True(remaining.Contains(Third));
        }

        [Fact]
        public void UntieActor_WithSeveralLinks_DoesNotBreakIteration()
        {
            _registry.Tie(Local, Partner);
            _registry.Tie(Local, Third);
            _registry.Tie(Local, 4);

            _registry.UntieActor(Local);

            Assert.Empty(_registry.Links);
            Assert.Equal(3, _removed.Count);
        }

        [Fact]
        public void ReleaseAllHeld_ReleasesOnlyOwnEnds()
        {
            _registry.Tie(Local, Partner);
            _registry.Tie(Local, Third);
            _registry.TryGet(Local, Partner, out RopeLink first);
            _registry.TryGet(Local, Third, out RopeLink second);
            _registry.Grab(first);
            _registry.Grab(second);
            _registry.ReceiveEnd(RemoteEnd(first, Partner, isHeld: true, reel: 1f));

            _registry.ReleaseAllHeld();

            Assert.False(first.IsHeldBy(Local));
            Assert.False(second.IsHeldBy(Local));
            Assert.True(first.IsHeldBy(Partner));
        }

        [Fact]
        public void LateJoiner_BuildsStateFromSnapshotInAnyOrder()
        {
            _registry.Tie(Local, Partner);
            _registry.Grab(Single());
            _registry.SetReel(Single(), 2f);
            LinkRegistry lateJoiner = CreateRegistry(new FakeTransport(), Third);

            lateJoiner.ReceiveEnd(_transport.LastEnd);
            lateJoiner.Receive(_transport.Last);

            RopeLink link = Assert.Single(lateJoiner.Links);
            Assert.True(link.IsHeldBy(Local));
            Assert.Equal(2f, link.TotalReel);
        }

        [Fact]
        public void Clear_RemovesLocallyWithoutPublishing()
        {
            _registry.Tie(Local, Partner);
            int publishedBefore = _transport.Published.Count;

            _registry.Clear();

            Assert.Empty(_registry.Links);
            Assert.Single(_removed);
            Assert.Equal(publishedBefore, _transport.Published.Count);
        }

        private LinkRegistry CreateRegistry(FakeTransport transport, int localActor)
        {
            LinkRegistry registry = new LinkRegistry(transport);
            registry.Bind(localActor);
            registry.LinkAdded += _added.Add;
            registry.LinkRemoved += _removed.Add;
            return registry;
        }

        private RopeLink Single()
        {
            return Assert.Single(_registry.Links);
        }

        private LinkUpdate RemoteTie(int first, int second)
        {
            _remoteSequence++;
            LinkState state = new LinkState(true, LinkState.ComposeTieId(Partner, _remoteSequence));
            return new LinkUpdate(LinkKeys.Build(first, second), state, Partner, _remoteSequence);
        }

        private LinkUpdate RemoteUntie(int first, int second)
        {
            _remoteSequence++;
            return new LinkUpdate(LinkKeys.Build(first, second), LinkState.Untied, Partner, _remoteSequence);
        }

        private EndUpdate RemoteEnd(RopeLink link, int owner, bool isHeld, float reel)
        {
            return new EndUpdate(link.EndKey(owner), new EndState(link.TieId, isHeld, reel));
        }
    }
}
