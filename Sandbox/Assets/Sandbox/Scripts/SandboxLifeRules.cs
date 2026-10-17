using System.Collections.Generic;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Interaction;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Unity;

namespace Repkarbinhake.Sandbox
{
    public sealed class SandboxLifeRules
    {
        private readonly LinkRegistry _registry;
        private readonly RopeHandPolicy _policy;
        private readonly TensionSolver _tension;
        private readonly SandboxCrew _crew;
        private readonly DeathUntieRule _deathRule;
        private readonly DepartedUntieRule _departedRule = new DepartedUntieRule();
        private readonly HashSet<int> _presentActors = new HashSet<int>();
        private readonly List<RopeLink> _toUntie = new List<RopeLink>();

        public SandboxLifeRules(LinkRegistry registry, RopeHandPolicy policy, TensionSolver tension, SandboxCrew crew)
        {
            _registry = registry;
            _policy = policy;
            _tension = tension;
            _crew = crew;
            _deathRule = new DeathUntieRule(new SandboxLives(crew.All));
        }

        public void Apply(int selectedActor)
        {
            UntieDeparted();
            UntieSnapped();
            _deathRule.Collect(_registry.Links, selectedActor, true, _toUntie);
            UntieCollected();
            ReleaseIncapacitatedHands(selectedActor);
        }

        public bool IsHolding(int actor)
        {
            foreach (RopeLink link in _registry.Links)
            {
                if (link.IsHeldBy(actor))
                    return true;
            }

            return false;
        }

        private void UntieDeparted()
        {
            _presentActors.Clear();

            foreach (SandboxClimber climber in _crew.Present())
                _presentActors.Add(climber.Actor);

            _departedRule.Collect(_registry.Links, _presentActors, _toUntie);
            UntieCollected();
        }

        private void UntieSnapped()
        {
            _toUntie.Clear();

            foreach (RopeLink link in _registry.Links)
            {
                if (_tension.IsSnapped(_crew.ClimberOf(link.FirstActor).RopePoint.ToNumerics(), _crew.ClimberOf(link.SecondActor).RopePoint.ToNumerics()))
                    _toUntie.Add(link);
            }

            UntieCollected();
        }

        private void UntieCollected()
        {
            foreach (RopeLink link in _toUntie)
                _registry.Untie(link);

            _toUntie.Clear();
        }

        private void ReleaseIncapacitatedHands(int selectedActor)
        {
            foreach (SandboxClimber climber in _crew.All)
            {
                if (_policy.CanHold(climber.Footing) || !IsHolding(climber.Actor))
                    continue;

                _registry.Bind(climber.Actor);
                _registry.ReleaseAllHeld();
                _registry.Bind(selectedActor);
            }
        }
    }
}
