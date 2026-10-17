using System.Collections.Generic;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Core.Visual;

namespace Repkarbinhake.Sandbox
{
    public sealed class SandboxOverlayReport
    {
        private const float Percent = 100f;

        private readonly List<string> _lines = new List<string>();
        private readonly LinkRegistry _registry;
        private readonly StabilityResolver _stability;
        private readonly RopeReel _reel;
        private readonly RopeSegmentation _segmentation;
        private readonly SandboxRopeSettings _settings;
        private readonly SandboxCrew _crew;
        private readonly SandboxRopeVisuals _visuals;
        private readonly SandboxRopeForces _forces;
        private readonly SandboxLifeRules _rules;

        public SandboxOverlayReport(LinkRegistry registry, StabilityResolver stability, RopeReel reel, RopeSegmentation segmentation, SandboxRopeSettings settings, SandboxCrew crew, SandboxRopeVisuals visuals, SandboxRopeForces forces, SandboxLifeRules rules)
        {
            _registry = registry;
            _stability = stability;
            _reel = reel;
            _segmentation = segmentation;
            _settings = settings;
            _crew = crew;
            _visuals = visuals;
            _forces = forces;
            _rules = rules;
        }

        public IReadOnlyList<string> Build(SandboxClimber selected, LiftTrial trial)
        {
            _lines.Clear();
            int ropeCount = _registry.Links.Count;
            _lines.Add($"Верёвок: {ropeCount}   режим костей: {_settings.SegmentMode}   потолок на верёвку: {_segmentation.Cap(ropeCount)}");

            foreach (SandboxClimber climber in _crew.All)
                _lines.Add(DescribeClimber(climber, climber == selected));

            foreach (RopeLink link in _registry.Links)
                _lines.Add(DescribeLink(link));

            trial.Describe(_lines);
            return _lines;
        }

        private string DescribeClimber(SandboxClimber climber, bool isSelected)
        {
            float stability = _stability.Resolve(climber.Actor, _registry.Links) * Percent;
            string marker = isSelected ? "►" : " ";
            string holding = _rules.IsHolding(climber.Actor) ? "да " : "нет";
            return $"{marker} #{climber.Actor}  держит: {holding}  устойчивость: {stability:0}%  стамина: {climber.Stamina * Percent:0}%  тяга: {_forces.PullOf(climber.Actor):0.0}  {StateOf(climber)}";
        }

        private string DescribeLink(RopeLink link)
        {
            float distance = (_crew.ClimberOf(link.FirstActor).RopePoint - _crew.ClimberOf(link.SecondActor).RopePoint).magnitude;
            return $"   #{link.FirstActor}—#{link.SecondActor}: длина {_reel.EffectiveLength(link):0.0} / {_reel.FullLength:0.0} м, дистанция {distance:0.0} м, держат {HoldersOf(link)}, костей {_visuals.SegmentsOf(link)}, контактов {_visuals.ContactsOf(link)}";
        }

        private string StateOf(SandboxClimber climber)
        {
            if (!climber.IsPresent)
                return "ВЫШЕЛ ИЗ ИГРЫ";

            if (climber.IsDead)
                return "МЁРТВ";

            if (climber.IsUnconscious)
                return "без сознания";

            if (climber.IsFallen)
                return "упал";

            return climber.IsGrounded ? "на земле" : "в воздухе";
        }

        private string HoldersOf(RopeLink link)
        {
            string first = $"#{link.FirstActor} (смотал {link.First.Reel:0.0})";
            string second = $"#{link.SecondActor} (смотал {link.Second.Reel:0.0})";

            if (link.First.IsHeld && link.Second.IsHeld)
                return first + " и " + second;

            if (link.First.IsHeld)
                return first;

            return link.Second.IsHeld ? second : "никто";
        }
    }
}
