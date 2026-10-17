using System.Collections.Generic;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Plugin.Visual;
using Repkarbinhake.Unity;
using UnityEngine;

namespace Repkarbinhake.Plugin.Links
{
    public sealed class AnchorBoard
    {
        private readonly LinkRegistry _registry;
        private readonly AnchorVisuals _visuals;
        private readonly Dictionary<int, AnchorPlacement> _anchors = new Dictionary<int, AnchorPlacement>();
        private readonly List<int> _owners = new List<int>();

        public AnchorBoard(LinkRegistry registry, AnchorVisuals visuals)
        {
            _registry = registry;
            _visuals = visuals;
        }

        public IReadOnlyDictionary<int, AnchorPlacement> Anchors => _anchors;

        public bool Has(int owner)
        {
            return _anchors.ContainsKey(owner);
        }

        public void Set(int owner, AnchorPlacement placement)
        {
            _anchors[owner] = placement;
            _visuals.Show(owner, placement);

            if (!_registry.TryGet(owner, AnchorActors.Of(owner), out _))
                _registry.Tie(owner, AnchorActors.Of(owner));
        }

        public void Remove(int owner)
        {
            if (!_anchors.Remove(owner))
                return;

            _visuals.Hide(owner);

            if (_registry.TryGet(owner, AnchorActors.Of(owner), out RopeLink link))
                _registry.Untie(link);
        }

        public void EnsureLinks()
        {
            foreach (int owner in _anchors.Keys)
            {
                if (!_registry.TryGet(owner, AnchorActors.Of(owner), out _))
                    _registry.Tie(owner, AnchorActors.Of(owner));
            }
        }

        public void Clear()
        {
            _owners.Clear();
            _owners.AddRange(_anchors.Keys);

            foreach (int owner in _owners)
                Remove(owner);

            _visuals.Clear();
        }

        public bool TryGetRopePoint(int anchorActor, out Vector3 point)
        {
            int owner = AnchorActors.OwnerOf(anchorActor);

            if (_visuals.TryGetRopePoint(owner, out point))
                return true;

            if (_anchors.TryGetValue(owner, out AnchorPlacement placement))
            {
                point = placement.Position.ToUnity();
                return true;
            }

            return false;
        }
    }
}
