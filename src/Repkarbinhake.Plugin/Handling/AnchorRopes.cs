using System.Collections.Generic;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Plugin.Links;
using Repkarbinhake.Plugin.Visual;
using UnityEngine;

namespace Repkarbinhake.Plugin.Handling
{
    public sealed class AnchorRopes
    {
        private const float FallSpeed = 3f;

        private readonly LinkRegistry _registry;
        private readonly AnchorBoard _board;
        private readonly RopePresenterPool _ropes;
        private readonly IRopeSettings _settings;
        private readonly AnchorLength _ownLength = new AnchorLength();

        public AnchorRopes(LinkRegistry registry, AnchorBoard board, RopePresenterPool ropes, IRopeSettings settings)
        {
            _registry = registry;
            _board = board;
            _ropes = ropes;
            _settings = settings;
        }

        public void Tick(Character local, int actor, Footing footing)
        {
            if (!_board.Has(actor))
                _ownLength.Reset();

            _board.EnsureLinks();

            foreach (KeyValuePair<int, AnchorPlacement> anchor in _board.Anchors)
            {
                int anchorActor = AnchorActors.Of(anchor.Key);

                if (!_registry.TryGet(anchor.Key, anchorActor, out RopeLink link))
                    continue;

                float pathLength = _ropes.SpanFor(link, anchor.Key).PathLength;
                float length = anchor.Key == actor ? OwnLength(local, footing, pathLength) : Mathf.Clamp(pathLength, _settings.MinLength, _settings.MaxLength);
                RopeEnd end = link.EndOf(anchorActor);
                end.IsHeld = true;
                end.Reel = Mathf.Max(_settings.MaxLength - length, 0f);
            }
        }

        private float OwnLength(Character local, Footing footing, float pathLength)
        {
            bool supported = footing == Footing.Grounded || footing == Footing.Climbing;
            bool falling = (footing == Footing.Airborne || footing == Footing.Unconscious) && local.data.avarageVelocity.y < -FallSpeed;
            return _ownLength.Next(pathLength, supported, falling, _settings.MinLength, _settings.MaxLength);
        }
    }
}
