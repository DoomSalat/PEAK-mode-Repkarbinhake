using System.Collections.Generic;
using BepInEx.Configuration;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Plugin.Data;
using Repkarbinhake.Plugin.Links;
using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class BeltCarabiners
    {
        private const float Scale = 2f;
        private const float HookAngle = 180f;
        private const float LowerRowDrop = 0.11f;
        private const float NeighborAngle = 50f;
        private const float Damping = 0.94f;
        private const float RopeAnchorShare = 0.85f;
        private const float ResetFactor = 4f;
        private const float SkinGap = 0.03f;
        private const float ContactRadius = 0.035f;
        private const float MiddleShare = 0.5f;
        private const float OutwardTilt = 0.42f;

        private static readonly float[] UpperAngles = { 40f, -40f, 85f, -85f, 130f, -130f }; // Static: fixed upper row slots, pairs from the front of the thighs to the sides
        private static readonly float[] LowerAngles = { 155f, -155f, 107.5f, -107.5f, 62.5f, -62.5f, 0f }; // Static: fixed lower row slots between two upper ones, filled back to front against the upper row; links past it get no carabiner

        private readonly Transform _root;
        private readonly CarabinerModel _model;
        private readonly CarabinerMaterials _materials;
        private readonly LinkRegistry _registry;
        private readonly CharacterDirectory _characters;
        private readonly LinkColors _colors;
        private readonly ConfigEntry<bool> _enabled;
        private readonly ConfigEntry<bool> _showOwn;
        private readonly BeltRing _ring = new BeltRing();
        private readonly BodyContacts _contacts;
        private readonly Dictionary<(string, int), Slot> _slots = new Dictionary<(string, int), Slot>();
        private readonly Dictionary<int, List<RopeLink>> _byActor = new Dictionary<int, List<RopeLink>>();
        private readonly HashSet<(string, int)> _seen = new HashSet<(string, int)>();
        private readonly List<(string, int)> _stale = new List<(string, int)>();
        private readonly List<RopeLink> _pending = new List<RopeLink>();
        private readonly List<float> _placedAngles = new List<float>();
        private readonly List<Color> _placedColors = new List<Color>();

        public BeltCarabiners(Transform root, CarabinerModel model, CarabinerMaterials materials, LinkRegistry registry, CharacterDirectory characters, LinkColors colors, ConfigEntry<bool> enabled, ConfigEntry<bool> showOwn)
        {
            _showOwn = showOwn;
            _root = root;
            _model = model;
            _materials = materials;
            _registry = registry;
            _characters = characters;
            _colors = colors;
            _enabled = enabled;
            _contacts = new BodyContacts(root);
        }

        public void Tick(float deltaTime)
        {
            if (!_enabled.Value)
            {
                Clear();
                return;
            }

            _seen.Clear();
            GroupByActor();

            foreach (KeyValuePair<int, List<RopeLink>> pair in _byActor)
            {
                if (pair.Value.Count == 0 || IsHiddenOwn(pair.Key) || !_characters.TryFind(pair.Key, out Character character) || character.data.dead)
                    continue;

                pair.Value.Sort(CompareKeys);
                PlaceBelt(character, pair.Key, pair.Value, deltaTime);
            }

            RemoveStale();
            _ring.Prune();
        }

        public bool TryGetRopePoint(RopeLink link, int actor, out Vector3 point)
        {
            if (_slots.TryGetValue((link.Key, actor), out Slot slot) && slot.Active)
            {
                point = slot.RopePoint;
                return true;
            }

            point = Vector3.zero;
            return false;
        }

        public void Clear()
        {
            foreach (Slot slot in _slots.Values)
                slot.View.Destroy();

            _slots.Clear();
        }

        private void GroupByActor()
        {
            foreach (List<RopeLink> links in _byActor.Values)
                links.Clear();

            foreach (RopeLink link in _registry.Links)
            {
                AddEnd(link, link.FirstActor);
                AddEnd(link, link.SecondActor);
            }
        }

        private void AddEnd(RopeLink link, int actor)
        {
            if (actor <= 0)
                return;

            if (!_byActor.TryGetValue(actor, out List<RopeLink> links))
            {
                links = new List<RopeLink>();
                _byActor[actor] = links;
            }

            links.Add(link);
        }

        private void PlaceBelt(Character character, int actor, List<RopeLink> links, float deltaTime)
        {
            BeltRing.Frame frame = _ring.FrameOf(character);
            _pending.Clear();
            _placedAngles.Clear();
            _placedColors.Clear();

            foreach (RopeLink link in links)
            {
                if (IsHook(link))
                    Hang(link, character, actor, frame, HookAngle, 0f, deltaTime);
                else
                    _pending.Add(link);
            }

            for (int place = 0; place < UpperAngles.Length + LowerAngles.Length && _pending.Count > 0; place++)
            {
                bool upper = place < UpperAngles.Length;
                float angle = upper ? UpperAngles[place] : LowerAngles[place - UpperAngles.Length];
                RopeLink link = TakeUnlikeNeighbors(angle);
                Hang(link, character, actor, frame, angle, upper ? 0f : LowerRowDrop, deltaTime);
            }
        }

        private RopeLink TakeUnlikeNeighbors(float angle)
        {
            int chosen = 0;

            for (int i = 0; i < _pending.Count; i++)
            {
                if (!_colors.TryGetColor(_pending[i], out Color color) || !HasNeighborColored(angle, color))
                {
                    chosen = i;
                    break;
                }
            }

            RopeLink link = _pending[chosen];
            _pending.RemoveAt(chosen);
            _placedAngles.Add(angle);
            _placedColors.Add(_colors.TryGetColor(link, out Color placed) ? placed : Color.clear);
            return link;
        }

        private bool HasNeighborColored(float angle, Color color)
        {
            for (int i = 0; i < _placedAngles.Count; i++)
            {
                if (Mathf.Abs(_placedAngles[i] - angle) <= NeighborAngle && _placedColors[i] == color)
                    return true;
            }

            return false;
        }

        private void Hang(RopeLink link, Character character, int actor, BeltRing.Frame frame, float angle, float drop, float deltaTime)
        {
            Vector3 offset = frame.Offset(angle * Mathf.Deg2Rad);
            Vector3 outward = offset.normalized;
            Slot slot = SlotFor(link, actor);
            Swing(slot, character, frame, frame.Center + offset + outward * SkinGap - frame.Up * drop, outward, deltaTime);
        }

        private bool IsHiddenOwn(int actor)
        {
            return actor == _registry.LocalActor && !_showOwn.Value;
        }

        private bool IsHook(RopeLink link)
        {
            return AnchorActors.IsAnchor(link.FirstActor) || AnchorActors.IsAnchor(link.SecondActor);
        }

        private Slot SlotFor(RopeLink link, int actor)
        {
            (string, int) key = (link.Key, actor);
            _seen.Add(key);

            if (!_slots.TryGetValue(key, out Slot slot))
            {
                slot = new Slot(new BeltCarabinerView(_root, _model.Mesh, Scale));
                _slots[key] = slot;
            }

            bool colored = _colors.TryGetColor(link, out Color color);

            if (!slot.Painted || slot.Color != color)
            {
                slot.View.Paint(colored ? _materials.For(color) : _materials.ForHook());
                slot.Color = color;
                slot.Painted = true;
            }

            return slot;
        }

        private void Swing(Slot slot, Character character, BeltRing.Frame frame, Vector3 pivot, Vector3 outward, float deltaTime)
        {
            float length = _model.HangLength * Scale;

            if (!slot.Active || (slot.Tip - pivot).sqrMagnitude > length * length * ResetFactor * ResetFactor)
            {
                slot.Tip = pivot + Vector3.down * length;
                slot.PreviousTip = slot.Tip;
            }

            Vector3 velocity = (slot.Tip - slot.PreviousTip) * Damping;
            slot.PreviousTip = slot.Tip;
            slot.Tip += velocity + Physics.gravity * deltaTime * deltaTime;
            Vector3 direction = DirectionFrom(pivot, slot.Tip);
            direction = Collide(character, frame, pivot, direction, length * MiddleShare);
            direction = Collide(character, frame, pivot, direction, length);
            direction = TiltOutward(direction, outward);
            slot.Tip = pivot + direction * length;
            Vector3 facing = Vector3.ProjectOnPlane(outward, direction);

            if (facing.sqrMagnitude < Mathf.Epsilon)
                facing = Vector3.Cross(direction, frame.Up);

            slot.View.Place(pivot, Quaternion.LookRotation(facing.normalized, -direction));
            slot.View.Show();
            slot.RopePoint = pivot + direction * length * RopeAnchorShare;
            slot.Active = true;
        }

        private Vector3 Collide(Character character, BeltRing.Frame frame, Vector3 pivot, Vector3 direction, float reach)
        {
            Vector3 point = pivot + direction * reach;
            point = frame.KeepOutside(point, SkinGap);
            point = _contacts.PushOut(character, point, ContactRadius);
            return DirectionFrom(pivot, point);
        }

        private Vector3 TiltOutward(Vector3 direction, Vector3 outward)
        {
            float share = Vector3.Dot(direction, outward);
            return share >= OutwardTilt ? direction : (direction + outward * (OutwardTilt - share)).normalized;
        }

        private Vector3 DirectionFrom(Vector3 pivot, Vector3 point)
        {
            Vector3 direction = point - pivot;
            return direction.sqrMagnitude < Mathf.Epsilon ? Vector3.down : direction.normalized;
        }

        private void RemoveStale()
        {
            _stale.Clear();

            foreach (KeyValuePair<(string, int), Slot> pair in _slots)
            {
                if (!_seen.Contains(pair.Key))
                    _stale.Add(pair.Key);
            }

            foreach ((string, int) key in _stale)
            {
                _slots[key].View.Destroy();
                _slots.Remove(key);
            }
        }

        private int CompareKeys(RopeLink first, RopeLink second)
        {
            return string.CompareOrdinal(first.Key, second.Key);
        }

        private sealed class Slot
        {
            public Slot(BeltCarabinerView view)
            {
                View = view;
            }

            public BeltCarabinerView View { get; }
            public Color Color { get; set; } // Set: repainted when the link color changes
            public bool Painted { get; set; } // Set: false until the first paint
            public bool Active { get; set; } // Set: true once placed on a belt
            public Vector3 Tip { get; set; } // Set: pendulum state, advanced every frame
            public Vector3 PreviousTip { get; set; } // Set: pendulum state, advanced every frame
            public Vector3 RopePoint { get; set; } // Set: where the rope leaves this carabiner, updated every frame
        }
    }
}
