using System.Collections.Generic;
using BepInEx.Logging;
using Repkarbinhake.Unity;
using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class RopeWrapProbe
    {
        private const float ReportInterval = 2f;

        private readonly int _collisionMask;
        private readonly int _solidMask;
        private readonly ManualLogSource _log;
        private float _nextReport;

        public RopeWrapProbe(int collisionMask, ManualLogSource log)
        {
            _collisionMask = collisionMask;
            _solidMask = HelperFunctions.GetMask(HelperFunctions.LayerType.AllPhysicalExceptCharacter);
            _log = log;
        }

        public void Check(string ropeKey, IReadOnlyList<System.Numerics.Vector3> points, float now)
        {
            if (now < _nextReport)
                return;

            int crossings = 0;
            RaycastHit first = default;

            for (int i = 1; i < points.Count; i++)
            {
                if (!Physics.Linecast(points[i - 1].ToUnity(), points[i].ToUnity(), out RaycastHit hit, _solidMask, QueryTriggerInteraction.Ignore))
                    continue;

                if (crossings == 0)
                    first = hit;

                crossings++;
            }

            if (crossings == 0)
                return;

            _nextReport = now + ReportInterval;
            GameObject blocker = first.collider.gameObject;
            bool collides = (_collisionMask & (1 << blocker.layer)) != 0;
            _log.LogWarning($"{nameof(RopeWrapProbe)}: rope {ropeKey} passes through {crossings} time(s), first: {blocker.name} on layer {LayerMask.LayerToName(blocker.layer)}, collider {first.collider.GetType().Name}, rope collides with this layer: {collides}");
        }
    }
}
