using System;
using System.Collections.Generic;
using Repkarbinhake.Core.Data;

namespace Repkarbinhake.Core.Links
{
    public sealed class LinkRegistry
    {
        private readonly ILinkTransport _transport;
        private readonly Dictionary<long, RopeLink> _links = new Dictionary<long, RopeLink>();
        private readonly Dictionary<string, EndUpdate> _orphanEnds = new Dictionary<string, EndUpdate>();
        private readonly Dictionary<string, int> _sentSequences = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _echoedSequences = new Dictionary<string, int>();
        private readonly List<RopeLink> _removalBuffer = new List<RopeLink>();
        private int _localActor;
        private int _sequence;

        public LinkRegistry(ILinkTransport transport)
        {
            _transport = transport;
        }

        public event Action<RopeLink> LinkAdded;
        public event Action<RopeLink> LinkRemoved;

        public IReadOnlyCollection<RopeLink> Links => _links.Values;
        public int PendingEndCount => _orphanEnds.Count;
        public int LocalActor => _localActor;

        public void Bind(int localActor)
        {
            _localActor = localActor;
        }

        public bool TryGet(int firstActor, int secondActor, out RopeLink link)
        {
            return _links.TryGetValue(LinkKeys.PairId(firstActor, secondActor), out link);
        }

        public bool TryGetHeld(int actor, out RopeLink held)
        {
            foreach (RopeLink link in _links.Values)
            {
                if (link.IsHeldBy(actor))
                {
                    held = link;
                    return true;
                }
            }

            held = null;
            return false;
        }

        public void Receive(LinkUpdate update)
        {
            if (!LinkKeys.TryParse(update.Key, out int firstActor, out int secondActor))
                return;

            if (update.WriterActor == _localActor)
            {
                _echoedSequences[update.Key] = update.Sequence;

                if (!HasPendingWrite(update.Key))
                    Apply(firstActor, secondActor, update.State);

                return;
            }

            if (HasPendingWrite(update.Key))
                return;

            Apply(firstActor, secondActor, update.State);
        }

        public void ReceiveEnd(EndUpdate update)
        {
            if (!LinkKeys.TryParseEnd(update.Key, out int firstActor, out int secondActor, out int endActor))
                return;

            if (endActor == _localActor)
                return;

            if (!TryGet(firstActor, secondActor, out RopeLink link) || link.TieId != update.State.TieId)
            {
                _orphanEnds[update.Key] = update;
                return;
            }

            ApplyEnd(link.EndOf(endActor), update.State);
        }

        public void Tie(int firstActor, int secondActor)
        {
            int sequence = NextSequence();
            LinkState state = new LinkState(true, LinkState.ComposeTieId(_localActor, sequence));
            Apply(firstActor, secondActor, state);
            Send(LinkKeys.Build(firstActor, secondActor), state, sequence);
        }

        public void Untie(RopeLink link)
        {
            Remove(link.FirstActor, link.SecondActor);
            Send(link.Key, LinkState.Untied, NextSequence());
        }

        public void Grab(RopeLink link)
        {
            RopeEnd end = link.EndOf(_localActor);
            end.IsHeld = true;
            end.Reel = 0f;
            PublishEnd(link, end);
        }

        public void Release(RopeLink link)
        {
            RopeEnd end = link.EndOf(_localActor);
            end.Reset();
            PublishEnd(link, end);
        }

        public void SetReel(RopeLink link, float reel)
        {
            ReelLocally(link, reel);
            PublishOwnEnd(link);
        }

        public void ReelLocally(RopeLink link, float reel)
        {
            link.EndOf(_localActor).Reel = reel;
        }

        public void PublishOwnEnd(RopeLink link)
        {
            PublishEnd(link, link.EndOf(_localActor));
        }

        public void ReleaseAllHeld()
        {
            foreach (RopeLink link in _links.Values)
            {
                if (link.IsHeldBy(_localActor))
                    Release(link);
            }
        }

        public void UntieActor(int actor)
        {
            _removalBuffer.Clear();

            foreach (RopeLink link in _links.Values)
            {
                if (link.Contains(actor))
                    _removalBuffer.Add(link);
            }

            foreach (RopeLink link in _removalBuffer)
                Untie(link);
        }

        public void Clear()
        {
            _removalBuffer.Clear();
            _removalBuffer.AddRange(_links.Values);

            foreach (RopeLink link in _removalBuffer)
                Remove(link.FirstActor, link.SecondActor);

            _orphanEnds.Clear();
            _sentSequences.Clear();
            _echoedSequences.Clear();
        }

        private bool HasPendingWrite(string key)
        {
            if (!_sentSequences.TryGetValue(key, out int sent))
                return false;

            _echoedSequences.TryGetValue(key, out int echoed);
            return sent > echoed;
        }

        private int NextSequence()
        {
            _sequence++;
            return _sequence;
        }

        private void Send(string key, LinkState state, int sequence)
        {
            if (_transport.EchoesOwnUpdates)
                _sentSequences[key] = sequence;

            _transport.Publish(new LinkUpdate(key, state, _localActor, sequence));
        }

        private void PublishEnd(RopeLink link, RopeEnd end)
        {
            _transport.PublishEnd(new EndUpdate(link.EndKey(end.Actor), new EndState(link.TieId, end.IsHeld, end.Reel)));
        }

        private void Apply(int firstActor, int secondActor, LinkState state)
        {
            long pair = LinkKeys.PairId(firstActor, secondActor);

            if (!state.IsTied)
            {
                Remove(firstActor, secondActor);
                return;
            }

            if (_links.TryGetValue(pair, out RopeLink existing))
            {
                if (existing.TieId == state.TieId)
                    return;

                existing.Retie(state.TieId);
                AdoptOrphans(existing);
                return;
            }

            RopeLink link = new RopeLink(firstActor, secondActor, state.TieId);
            _links.Add(pair, link);
            AdoptOrphans(link);
            LinkAdded?.Invoke(link);
        }

        private void AdoptOrphans(RopeLink link)
        {
            AdoptOrphan(link, link.First);
            AdoptOrphan(link, link.Second);
        }

        private void AdoptOrphan(RopeLink link, RopeEnd end)
        {
            string endKey = link.EndKey(end.Actor);

            if (!_orphanEnds.TryGetValue(endKey, out EndUpdate orphan))
                return;

            _orphanEnds.Remove(endKey);

            if (orphan.State.TieId == link.TieId)
                ApplyEnd(end, orphan.State);
        }

        private void ApplyEnd(RopeEnd end, EndState state)
        {
            end.IsHeld = state.IsHeld;
            end.Reel = state.IsHeld ? state.Reel : 0f;
        }

        private void Remove(int firstActor, int secondActor)
        {
            long pair = LinkKeys.PairId(firstActor, secondActor);

            if (!_links.TryGetValue(pair, out RopeLink link))
                return;

            _links.Remove(pair);
            _orphanEnds.Remove(link.EndKey(link.FirstActor));
            _orphanEnds.Remove(link.EndKey(link.SecondActor));
            LinkRemoved?.Invoke(link);
        }
    }
}
