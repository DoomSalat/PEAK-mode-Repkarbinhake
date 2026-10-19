using System.Collections.Generic;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Core.Visual;
using Repkarbinhake.Plugin.Data;
using Repkarbinhake.Plugin.Visual;
using Repkarbinhake.Unity;
using UnityEngine;

namespace Repkarbinhake.Plugin.Handling
{
    public sealed class RopeKnockdown
    {
        private readonly LinkRegistry _registry;
        private readonly RopeReel _reel;
        private readonly RopePresenterPool _ropes;
        private readonly RepkaConfig _config;
        private readonly RopeSnatch _snatch = new RopeSnatch();
        private readonly Dictionary<string, int> _pathVersions = new Dictionary<string, int>();

        public RopeKnockdown(LinkRegistry registry, RopeReel reel, RopePresenterPool ropes, RepkaConfig config)
        {
            _registry = registry;
            _reel = reel;
            _ropes = ropes;
            _config = config;
        }

        public void Tick(Character local, int actor, float deltaTime)
        {
            bool canFall = _config.EnableKnockdown.Value && local.data.isGrounded && local.data.fallSeconds <= 0f && !local.data.dead && !local.data.passedOut;
            Vector3 velocity = local.data.avarageVelocity;

            foreach (RopeLink link in _registry.Links)
            {
                if (!link.Contains(actor) || AnchorActors.IsAnchor(link.FirstActor))
                    continue;

                SkipIfReshaped(link);
                RopeSpan span = _ropes.SpanFor(link, actor);
                float stretch = span.PathLength - _reel.EffectiveLength(link);
                float ownSeparatingSpeed = Mathf.Max(-Vector3.Dot(velocity, span.Direction.ToUnity()), 0f);
                float snatch = _snatch.Measure(link.Key, stretch, ownSeparatingSpeed, deltaTime);

                if (!canFall || link.IsHeldBy(actor) || snatch < _config.KnockdownSpeed.Value)
                    continue;

                local.Fall(_config.KnockdownSeconds.Value);
                canFall = false;
            }
        }

        public void Forget(RopeLink link)
        {
            _snatch.Forget(link.Key);
            _pathVersions.Remove(link.Key);
        }

        public void Clear()
        {
            _snatch.Clear();
            _pathVersions.Clear();
        }

        private void SkipIfReshaped(RopeLink link)
        {
            if (!_ropes.TryGetPath(link, out RopeWrapPath path))
                return;

            if (_pathVersions.TryGetValue(link.Key, out int version) && version != path.Version)
                _snatch.Forget(link.Key);

            _pathVersions[link.Key] = path.Version;
        }
    }
}
