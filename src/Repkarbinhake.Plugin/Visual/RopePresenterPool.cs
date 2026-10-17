using System;
using System.Collections.Generic;
using Photon.Pun;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Interaction;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Core.Visual;
using Repkarbinhake.Plugin.Data;
using Repkarbinhake.Plugin.Handling;
using Repkarbinhake.Plugin.Links;
using Repkarbinhake.Unity;
using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class RopePresenterPool
    {
        private const string RopeObjectPrefix = "Repkarbinhake Rope ";
        private const string RopeCutoffProperty = "_RopeCutoff";
        private const float FullCutoff = 2f;
        private const string FallbackShader = "Universal Render Pipeline/Lit";

        private static readonly Color FallbackColor = new Color(0.55f, 0.36f, 0.18f); // Static: constant rope color
        private static readonly Color HighlightEmission = new Color(0.55f, 0.4f, 0.1f); // Static: constant hover glow

        private readonly Transform _root;
        private readonly RepkaConfig _config;
        private readonly RopeReel _reel;
        private readonly RopeSegmentation _segmentation;
        private readonly ActorStates _actors;
        private readonly LinkRegistry _registry;
        private readonly RopeHandPolicy _policy;
        private readonly IRopeObstacles _obstacles;
        private readonly VanillaRopeSpool _spool;
        private readonly RopeRayPicker _picker = new RopeRayPicker();
        private readonly Dictionary<string, RopePresenter> _presenters = new Dictionary<string, RopePresenter>();
        private readonly RopeRecolor _recolor = new RopeRecolor();
        private Material _material;

        public RopePresenterPool(Transform root, RepkaConfig config, RopeReel reel, RopeSegmentation segmentation, ActorStates actors, LinkRegistry registry, RopeHandPolicy policy, IRopeObstacles obstacles, VanillaRopeSpool spool)
        {
            _root = root;
            _config = config;
            _reel = reel;
            _segmentation = segmentation;
            _actors = actors;
            _registry = registry;
            _policy = policy;
            _obstacles = obstacles;
            _spool = spool;
        }

        public event Action<RopeLink> RopeInteracted;

        public void Add(RopeLink link)
        {
            GameObject ropeObject = new GameObject(RopeObjectPrefix + link.Key);
            ropeObject.transform.SetParent(_root, false);
            MeshFilter filter = ropeObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = ropeObject.AddComponent<MeshRenderer>();
            RopeTubeView view = ropeObject.AddComponent<RopeTubeView>();
            RopeInteractible interactible = ropeObject.AddComponent<RopeInteractible>();
            view.Initialize(filter, meshRenderer, RopeMaterial(), _config.Thickness.Value, HighlightEmission);
            interactible.Initialize(link, view, CanInteract, IsHeldLocally, PartnerNameOf);
            interactible.Interacted += OnInteracted;
            _presenters.Add(link.Key, new RopePresenter(link, view, meshRenderer, interactible));
        }

        public void Tint(RopeLink link, Color color)
        {
            if (!_presenters.TryGetValue(link.Key, out RopePresenter presenter))
                return;

            if (_config.RopeColor.Value > 0f)
                presenter.ChangeMaterial(_recolor.For(RopeMaterial(), color, _config.RopeColor.Value));
        }

        public void Remove(RopeLink link)
        {
            if (!_presenters.TryGetValue(link.Key, out RopePresenter presenter))
                return;

            presenter.Interactible.Interacted -= OnInteracted;
            presenter.Destroy();
            _presenters.Remove(link.Key);
        }

        public void Render(float deltaTime)
        {
            int ropeCount = _presenters.Count;

            foreach (RopePresenter presenter in _presenters.Values)
            {
                RopeLink link = presenter.Link;
                bool hasStart = _actors.TryGetRopePoint(link.FirstActor, link.First.IsHeld, out Vector3 start);
                bool hasEnd = _actors.TryGetRopePoint(link.SecondActor, link.Second.IsHeld, out Vector3 end);

                if (!hasStart || !hasEnd)
                {
                    presenter.Hide();
                    continue;
                }

                float length = _reel.EffectiveLength(link);
                RopeLengthState state = new RopeLengthState(length, link.First.HeldReel, link.Second.HeldReel);
                presenter.Tick(start, end, state, _segmentation.Count(length, ropeCount), deltaTime, _config.Thickness.Value, _obstacles);
                presenter.Render(LeadOf(link.First), LeadOf(link.Second));
            }
        }

        public RopeSpan SpanFor(RopeLink link, int actor)
        {
            if (_presenters.TryGetValue(link.Key, out RopePresenter presenter) && presenter.TrySpan(actor == link.FirstActor, out RopeSpan span))
                return span;

            int partner = link.Partner(actor);
            _actors.TryGetRopePoint(actor, link.IsHeldBy(actor), out Vector3 self);
            _actors.TryGetRopePoint(partner, link.IsHeldBy(partner), out Vector3 other);
            return RopeSpan.Straight(self.ToNumerics(), other.ToNumerics());
        }

        public bool TryAim(Ray ray, int localActor, out RopeInteractible aimed, out float distance)
        {
            aimed = null;
            distance = float.MaxValue;

            foreach (RopePresenter presenter in _presenters.Values)
            {
                if (!presenter.Link.Contains(localActor) || AnchorActors.IsAnchor(presenter.Link.FirstActor))
                    continue;

                if (!presenter.TryPick(_picker, ray, _config.RopeAimReach.Value, _config.RopeAimTolerance.Value, out float hit) || hit >= distance)
                    continue;

                distance = hit;
                aimed = presenter.Interactible;
            }

            if (aimed != null)
                aimed.Aim(ray.origin + ray.direction * distance);

            return aimed != null;
        }

        public bool TryGetHeld(int actor, out RopeInteractible held)
        {
            held = null;

            if (!_registry.TryGetHeld(actor, out RopeLink link) || !_presenters.TryGetValue(link.Key, out RopePresenter presenter) || presenter.Points.Count == 0)
                return false;

            held = presenter.Interactible;
            held.Aim(link.FirstActor == actor ? presenter.Points[0].ToUnity() : presenter.Points[presenter.Points.Count - 1].ToUnity());
            return true;
        }

        public bool TryGetPath(RopeLink link, out RopeWrapPath path)
        {
            bool found = _presenters.TryGetValue(link.Key, out RopePresenter presenter);
            path = found ? presenter.Path : null;
            return found;
        }

        public int SegmentsOf(RopeLink link)
        {
            return _presenters.TryGetValue(link.Key, out RopePresenter presenter) ? presenter.SegmentCount : 0;
        }

        private Vector3? LeadOf(RopeEnd end)
        {
            return end.IsHeld && _actors.TryGetRopeLead(end.Actor, out Vector3 lead) ? lead : (Vector3?)null;
        }

        private bool CanInteract(RopeLink link)
        {
            int local = _registry.LocalActor;
            return link.Contains(local) && !AnchorActors.IsAnchor(link.FirstActor) && _policy.CanHold(_actors.FootingOf(local));
        }

        private bool IsHeldLocally(RopeLink link)
        {
            return link.IsHeldBy(_registry.LocalActor);
        }

        private string PartnerNameOf(RopeLink link)
        {
            Photon.Realtime.Player partner = PhotonNetwork.CurrentRoom?.GetPlayer(link.Partner(_registry.LocalActor));
            return partner?.NickName;
        }

        private void OnInteracted(RopeLink link)
        {
            RopeInteracted?.Invoke(link);
        }

        private Material RopeMaterial()
        {
            if (_material == null)
                _material = ResolveMaterial();

            return _material;
        }

        private Material ResolveMaterial()
        {
            Material source = _config.UseVanillaRopeMaterial.Value ? _spool.RopeMaterial : null;

            if (source == null)
                return new Material(Shader.Find(FallbackShader)) { color = FallbackColor };

            Material vanilla = new Material(source);

            if (vanilla.HasProperty(RopeCutoffProperty))
                vanilla.SetFloat(RopeCutoffProperty, FullCutoff);

            return vanilla;
        }
    }
}
