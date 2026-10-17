using System.Collections.Generic;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Core.Visual;
using Repkarbinhake.Unity;
using UnityEngine;

namespace Repkarbinhake.Sandbox
{
    public sealed class SandboxRopeVisuals
    {
        private const float AimReach = 9f;
        private const float AimTolerance = 0.25f;

        private readonly Dictionary<string, RopeVisual> _visuals = new Dictionary<string, RopeVisual>();
        private readonly RopeRayPicker _picker = new RopeRayPicker();
        private readonly SandboxCrew _crew;
        private readonly SandboxRopeSettings _settings;
        private readonly RopeReel _reel;
        private readonly RopeSegmentation _segmentation;
        private readonly IRopeObstacles _obstacles;
        private readonly Material _material;
        private readonly Color _highlightEmission;
        private RopeVisual _highlighted;

        public SandboxRopeVisuals(SandboxCrew crew, SandboxRopeSettings settings, RopeReel reel, RopeSegmentation segmentation, IRopeObstacles obstacles, Material material, Color highlightEmission)
        {
            _crew = crew;
            _settings = settings;
            _reel = reel;
            _segmentation = segmentation;
            _obstacles = obstacles;
            _material = material;
            _highlightEmission = highlightEmission;
        }

        public void Add(RopeLink link)
        {
            GameObject ropeObject = new GameObject($"Rope #{link.FirstActor}-#{link.SecondActor}");
            MeshFilter filter = ropeObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = ropeObject.AddComponent<MeshRenderer>();
            RopeTubeView view = ropeObject.AddComponent<RopeTubeView>();
            view.Initialize(filter, meshRenderer, _material, _settings.Thickness, _highlightEmission);
            _visuals.Add(link.Key, new RopeVisual(view));
        }

        public void Remove(RopeLink link)
        {
            RopeVisual visual = _visuals[link.Key];

            if (_highlighted == visual)
                _highlighted = null;

            visual.Destroy();
            _visuals.Remove(link.Key);
        }

        public void Render(IReadOnlyCollection<RopeLink> links, float deltaTime)
        {
            foreach (RopeLink link in links)
            {
                float length = _reel.EffectiveLength(link);
                int segments = _segmentation.Count(length, links.Count);
                RopeLengthState state = new RopeLengthState(length, link.First.HeldReel, link.Second.HeldReel);
                _visuals[link.Key].Tick(_crew.ClimberOf(link.FirstActor).RopePoint, _crew.ClimberOf(link.SecondActor).RopePoint, state, segments, _settings.Thickness, deltaTime, _obstacles);
            }
        }

        public RopeSpan SpanFor(RopeLink link, SandboxClimber self)
        {
            if (_visuals.TryGetValue(link.Key, out RopeVisual visual) && visual.TrySpan(self.Actor == link.FirstActor, out RopeSpan span))
                return span;

            SandboxClimber partner = _crew.ClimberOf(link.Partner(self.Actor));
            return RopeSpan.Straight(self.RopePoint.ToNumerics(), partner.RopePoint.ToNumerics());
        }

        public RopeLink PickOwn(IReadOnlyCollection<RopeLink> links, int actor, Vector3 origin, Vector3 direction)
        {
            RopeLink best = null;
            float bestDistance = float.MaxValue;

            foreach (RopeLink link in links)
            {
                if (!link.Contains(actor))
                    continue;

                if (_visuals[link.Key].TryPick(_picker, origin, direction, AimReach, AimTolerance, out float distance) && distance < bestDistance)
                {
                    bestDistance = distance;
                    best = link;
                }
            }

            return best;
        }

        public void Highlight(RopeLink link)
        {
            RopeVisual visual = link == null ? null : _visuals[link.Key];

            if (_highlighted == visual)
                return;

            _highlighted?.Unhighlight();
            _highlighted = visual;
            _highlighted?.Highlight();
        }

        public int SegmentsOf(RopeLink link)
        {
            return _visuals[link.Key].SegmentCount;
        }

        public string StatsOf(RopeLink link)
        {
            RopeVisual visual = _visuals[link.Key];
            return $"contacts {visual.ContactCount}, contact changes {visual.ContactChanges}, length travel {visual.LengthTravel:0.00} m, net change {visual.NetLengthChange:0.00} m";
        }

        public void ResetStats()
        {
            foreach (RopeVisual visual in _visuals.Values)
                visual.ResetStats();
        }

        public int ContactsOf(RopeLink link)
        {
            return _visuals[link.Key].ContactCount;
        }
    }
}
