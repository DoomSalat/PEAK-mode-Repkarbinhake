using System.Collections.Generic;
using UnityEngine;

namespace Repkarbinhake.Sandbox
{
    public sealed class SandboxAimPicker
    {
        private readonly int _blockMask;

        public SandboxAimPicker(int blockMask)
        {
            _blockMask = blockMask;
        }

        public bool TryPick(SandboxClimber self, IReadOnlyList<SandboxClimber> climbers, Vector3 viewPosition, Vector3 viewForward, float maxDistance, float maxAngle, out SandboxClimber target)
        {
            target = null;
            float bestAngle = maxAngle;

            foreach (SandboxClimber candidate in climbers)
            {
                if (candidate == self || candidate.IsDead || !candidate.IsPresent)
                    continue;

                if ((candidate.Center - self.Center).sqrMagnitude > maxDistance * maxDistance)
                    continue;

                float angle = Vector3.Angle(viewForward, candidate.Center - viewPosition);

                if (angle >= bestAngle || Physics.Linecast(viewPosition, candidate.Center, _blockMask, QueryTriggerInteraction.Ignore))
                    continue;

                bestAngle = angle;
                target = candidate;
            }

            return target != null;
        }
    }
}
