using Repkarbinhake.Plugin.Data;
using Repkarbinhake.Plugin.Links;
using UnityEngine;

namespace Repkarbinhake.Plugin.Input
{
    public sealed class LinkTargetPicker
    {
        private readonly RepkaConfig _config;
        private readonly CharacterDirectory _characters;
        private readonly TestDummy _dummy;

        public LinkTargetPicker(RepkaConfig config, CharacterDirectory characters, TestDummy dummy)
        {
            _config = config;
            _characters = characters;
            _dummy = dummy;
        }

        public bool TryPick(Character self, Transform view, out int target)
        {
            target = 0;
            float bestAngle = _config.LinkAimAngle.Value;

            foreach (Character candidate in Character.AllCharacters)
            {
                if (candidate == null || candidate == self || candidate.isBot || candidate.data.dead)
                    continue;

                if (IsBetter(view, candidate.Center, ref bestAngle))
                    target = _characters.ActorOf(candidate);
            }

            if (_dummy.IsPlaced && IsDummyAimed(view, ref bestAngle))
                target = TestDummy.Actor;

            return target != 0;
        }

        private bool IsDummyAimed(Transform view, ref float bestAngle)
        {
            if (!_dummy.IsHitBy(new Ray(view.position, view.forward), global::Interaction.instance.distance))
                return IsBetter(view, _dummy.Center, ref bestAngle);

            bestAngle = 0f;
            return true;
        }

        private bool IsBetter(Transform view, Vector3 center, ref float bestAngle)
        {
            float maxDistance = global::Interaction.instance.distance;

            if ((center - view.position).sqrMagnitude > maxDistance * maxDistance)
                return false;

            float angle = Vector3.Angle(view.forward, center - view.position);

            if (angle >= bestAngle || IsBlocked(view.position, center))
                return false;

            bestAngle = angle;
            return true;
        }

        private bool IsBlocked(Vector3 from, Vector3 to)
        {
            return HelperFunctions.LineCheck(from, to, HelperFunctions.LayerType.TerrainMap).collider != null;
        }
    }
}
