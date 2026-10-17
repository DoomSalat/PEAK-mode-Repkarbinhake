using System.Collections.Generic;
using Photon.Pun;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Plugin.Links;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class RopeDebugReport
    {
        private const float Percent = 100f;

        private readonly LinkRegistry _registry;
        private readonly ActorStates _actors;
        private readonly StabilityResolver _stability;
        private readonly TensionSolver _tension;
        private readonly RopeReel _reel;
        private readonly RopePresenterPool _ropes;
        private readonly IRopeSettings _settings;
        private readonly List<string> _lines = new List<string>();

        public RopeDebugReport(LinkRegistry registry, ActorStates actors, StabilityResolver stability, TensionSolver tension, RopeReel reel, RopePresenterPool ropes, IRopeSettings settings)
        {
            _registry = registry;
            _actors = actors;
            _stability = stability;
            _tension = tension;
            _reel = reel;
            _ropes = ropes;
            _settings = settings;
        }

        public IReadOnlyList<string> Build(int actor)
        {
            _lines.Clear();
            _lines.Add($"actor {actor}  footing {_actors.FootingOf(actor)}  stability {_stability.Resolve(actor, _registry.Links) * Percent:0}%  maxLength {_settings.MaxLength:0.0}  host {PhotonNetwork.IsMasterClient}");

            foreach (RopeLink link in _registry.Links)
            {
                float load = link.Contains(actor) ? _tension.Load(_ropes.SpanFor(link, actor), _reel.EffectiveLength(link)) : 0f;
                _lines.Add($"{link.Key}  length {_reel.EffectiveLength(link):0.00}  load {load:0.0}  held {link.First.IsHeld}/{link.Second.IsHeld}  bones {_ropes.SegmentsOf(link)}");
            }

            return _lines;
        }
    }
}
