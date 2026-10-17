using System.Collections.Generic;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Links;
using Repkarbinhake.Plugin.Links;

namespace Repkarbinhake.Plugin.Handling
{
    public sealed class RopeHoldPoses
    {
        private const float SpoolDegreesPerMeter = 200f;
        private const float FullTurn = 360f;
        private const float HalfTurn = 180f;
        private const float SpinFollowRate = 10f;

        private readonly LinkRegistry _registry;
        private readonly CharacterDirectory _characters;
        private readonly Dictionary<int, float> _angles = new Dictionary<int, float>();
        private readonly Dictionary<int, float> _shownAngles = new Dictionary<int, float>();
        private readonly Dictionary<Character, RopeHoldPose> _poses = new Dictionary<Character, RopeHoldPose>();
        private Dictionary<int, float> _heldReels = new Dictionary<int, float>();
        private Dictionary<int, float> _previousReels = new Dictionary<int, float>();

        public RopeHoldPoses(LinkRegistry registry, CharacterDirectory characters)
        {
            _registry = registry;
            _characters = characters;
        }

        public IReadOnlyDictionary<Character, RopeHoldPose> Poses => _poses;

        public void Refresh(float deltaTime)
        {
            _poses.Clear();
            _heldReels.Clear();

            foreach (RopeLink link in _registry.Links)
            {
                CollectHeld(link.First);
                CollectHeld(link.Second);
            }

            foreach (KeyValuePair<int, float> held in _heldReels)
            {
                _angles.TryGetValue(held.Key, out float angle);

                if (_previousReels.TryGetValue(held.Key, out float previous))
                    angle = (angle + (held.Value - previous) * SpoolDegreesPerMeter) % FullTurn;

                _angles[held.Key] = angle;
                float shown = Follow(held.Key, angle, deltaTime);

                if (_characters.TryFind(held.Key, out Character character))
                    _poses[character] = new RopeHoldPose(held.Key, shown);
            }

            Dictionary<int, float> swap = _previousReels;
            _previousReels = _heldReels;
            _heldReels = swap;
        }

        public void Clear()
        {
            _poses.Clear();
            _heldReels.Clear();
            _previousReels.Clear();
            _shownAngles.Clear();
        }

        private float Follow(int actor, float angle, float deltaTime)
        {
            if (!_shownAngles.TryGetValue(actor, out float shown))
                shown = angle;

            float gap = ((angle - shown + HalfTurn) % FullTurn + FullTurn) % FullTurn - HalfTurn;
            shown = (shown + gap * System.Math.Min(SpinFollowRate * deltaTime, 1f)) % FullTurn;
            _shownAngles[actor] = shown;
            return shown;
        }

        private void CollectHeld(RopeEnd end)
        {
            if (!end.IsHeld)
                return;

            _heldReels.TryGetValue(end.Actor, out float reel);
            _heldReels[end.Actor] = reel + end.Reel;
        }
    }
}
