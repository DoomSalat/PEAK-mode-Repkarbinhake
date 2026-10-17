using System.Collections.Generic;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Unity;
using UnityEngine;

namespace Repkarbinhake.Sandbox
{
    public sealed class SandboxRopeForces
    {
        private readonly LinkRegistry _registry;
        private readonly StabilityResolver _stability;
        private readonly TensionSolver _tension;
        private readonly RopeReel _reel;
        private readonly SandboxRopeVisuals _visuals;
        private readonly SandboxRopeSettings _settings;
        private readonly Dictionary<int, float> _pulls = new Dictionary<int, float>();

        public SandboxRopeForces(LinkRegistry registry, StabilityResolver stability, TensionSolver tension, RopeReel reel, SandboxRopeVisuals visuals, SandboxRopeSettings settings)
        {
            _registry = registry;
            _stability = stability;
            _tension = tension;
            _reel = reel;
            _visuals = visuals;
            _settings = settings;
        }

        public void Apply(IReadOnlyList<SandboxClimber> climbers)
        {
            foreach (SandboxClimber climber in climbers)
            {
                float stability = _stability.Resolve(climber.Actor, _registry.Links);
                Vector3 total = Vector3.zero;

                foreach (RopeLink link in _registry.Links)
                {
                    if (link.Contains(climber.Actor))
                        total += _tension.Solve(_visuals.SpanFor(link, climber), climber.Velocity.ToNumerics(), _reel.EffectiveLength(link), stability).ToUnity();
                }

                climber.AddAcceleration(total);
                _pulls[climber.Actor] = total.magnitude;

                if (_settings.RopeSupportCapsAirTime && !climber.IsGrounded && total.y >= _settings.SupportLoadThreshold)
                    climber.ClampAirTime(_settings.HangAirTimeCap);
            }
        }

        public float PullOf(int actor)
        {
            return _pulls.TryGetValue(actor, out float pull) ? pull : 0f;
        }

        public float LoadOn(RopeLink link, SandboxClimber self)
        {
            return _tension.Load(_visuals.SpanFor(link, self), _reel.EffectiveLength(link));
        }
    }
}
