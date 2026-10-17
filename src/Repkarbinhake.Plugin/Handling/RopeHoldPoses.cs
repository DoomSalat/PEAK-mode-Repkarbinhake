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

        private readonly LinkRegistry _registry;
        private readonly CharacterDirectory _characters;
        private readonly Dictionary<int, float> _angles = new Dictionary<int, float>();
        private readonly Dictionary<Character, RopeHoldPose> _poses = new Dictionary<Character, RopeHoldPose>();
        private Dictionary<int, float> _heldReels = new Dictionary<int, float>();
        private Dictionary<int, float> _previousReels = new Dictionary<int, float>();

        public RopeHoldPoses(LinkRegistry registry, CharacterDirectory characters)
        {
            _registry = registry;
            _characters = characters;
        }

        public IReadOnlyDictionary<Character, RopeHoldPose> Poses => _poses;

        public void Refresh()
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

                if (_characters.TryFind(held.Key, out Character character))
                    _poses[character] = new RopeHoldPose(held.Key, angle);
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
