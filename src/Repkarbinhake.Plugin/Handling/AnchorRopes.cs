using BepInEx.Configuration;
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
        private const float ClosestToHook = 1f;
        private const float AtMaxTolerance = 0.05f;

        private readonly LinkRegistry _registry;
        private readonly AnchorBoard _board;
        private readonly RopePresenterPool _ropes;
        private readonly IRopeSettings _settings;
        private readonly ConfigEntry<float> _retractSpeed;
        private readonly ConfigEntry<float> _hookLength;
        private readonly HookLengths _lengths;
        private readonly AnchorLength _ownLength = new AnchorLength();

        public AnchorRopes(LinkRegistry registry, AnchorBoard board, RopePresenterPool ropes, IRopeSettings settings, ConfigEntry<float> retractSpeed, ConfigEntry<float> hookLength, HookLengths lengths)
        {
            _registry = registry;
            _board = board;
            _ropes = ropes;
            _settings = settings;
            _retractSpeed = retractSpeed;
            _hookLength = hookLength;
            _lengths = lengths;
        }

        public bool IsOwnLocked => _ownLength.IsLocked;
        public float OwnMeters { get; private set; }
        public bool IsOwnAtMax { get; private set; }
        public bool IsOwnRetracting { get; private set; }

        public void Tick(Character local, int actor, Footing footing, float deltaTime)
        {
            if (!_board.Has(actor))
            {
                _ownLength.Reset();
                OwnMeters = 0f;
                IsOwnAtMax = false;
                IsOwnRetracting = false;
            }

            _board.EnsureLinks();
            _lengths.Clear();

            foreach (KeyValuePair<int, AnchorPlacement> anchor in _board.Anchors)
            {
                int anchorActor = AnchorActors.Of(anchor.Key);

                if (!_registry.TryGet(anchor.Key, anchorActor, out RopeLink link))
                    continue;

                float pathLength = _ropes.SpanFor(link, anchor.Key).PathLength;
                float length = anchor.Key == actor ? OwnLength(local, footing, pathLength, deltaTime) : Mathf.Max(pathLength, _settings.MinLength);
                _lengths.Set(link, length);
                RopeEnd end = link.EndOf(anchorActor);
                end.IsHeld = true;
                end.Reel = Mathf.Max(_settings.MaxLength - length, 0f);
            }
        }

        private float OwnLength(Character local, Footing footing, float pathLength, float deltaTime)
        {
            bool supported = footing == Footing.Grounded || footing == Footing.Climbing;
            bool falling = (footing == Footing.Airborne || footing == Footing.Unconscious) && local.data.avarageVelocity.y < -FallSpeed;
            float maxLength = Mathf.Max(_hookLength.Value, _settings.MinLength);
            float held = _ownLength.Next(pathLength, supported, falling, _settings.MinLength, maxLength);
            OwnMeters = _ownLength.Retract(_retractSpeed.Value * deltaTime, Mathf.Max(ClosestToHook, _settings.MinLength));
            IsOwnRetracting = OwnMeters < held;
            IsOwnAtMax = OwnMeters >= maxLength - AtMaxTolerance;
            return OwnMeters;
        }
    }
}
