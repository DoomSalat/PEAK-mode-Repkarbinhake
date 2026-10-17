using System.Collections.Generic;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Core.Visual;
using Repkarbinhake.Unity;
using UnityEngine;

namespace Repkarbinhake.Sandbox
{
    public sealed class RopeVisual
    {
        private readonly RopeWrapPath _path = new RopeWrapPath();
        private readonly RopeSag _sag = new RopeSag();
        private readonly List<System.Numerics.Vector3> _points = new List<System.Numerics.Vector3>();
        private readonly RopeTubeView _view;
        private int _lastContacts;
        private float _lastPathLength = -1f;
        private float _firstPathLength = -1f;

        public RopeVisual(RopeTubeView view)
        {
            _view = view;
        }

        public int SegmentCount => _points.Count;
        public int ContactCount => _path.ContactCount;
        public int ContactChanges { get; private set; }
        public float LengthTravel { get; private set; }
        public float NetLengthChange => _lastPathLength - _firstPathLength;

        public void Tick(Vector3 start, Vector3 end, RopeLengthState length, int segments, float radius, float deltaTime, IRopeObstacles obstacles)
        {
            _path.Update(start.ToNumerics(), end.ToNumerics(), length, radius, deltaTime, obstacles);
            TrackStats();
            _sag.Build(_path, Vector3.down.ToNumerics(), segments, radius, deltaTime, obstacles, _points);
            _view.Render(_points);
        }

        public void ResetStats()
        {
            ContactChanges = 0;
            LengthTravel = 0f;
            _firstPathLength = _lastPathLength;
        }

        public bool TrySpan(bool fromStart, out RopeSpan span)
        {
            return _path.TrySpan(fromStart, out span);
        }

        public bool TryPick(RopeRayPicker picker, Vector3 origin, Vector3 direction, float reach, float tolerance, out float distance)
        {
            return picker.TryPick(origin.ToNumerics(), direction.ToNumerics(), reach, tolerance, _points, out distance);
        }

        public void Highlight()
        {
            _view.Highlight();
        }

        public void Unhighlight()
        {
            _view.Unhighlight();
        }

        public void Destroy()
        {
            Object.Destroy(_view.gameObject);
        }

        private void TrackStats()
        {
            float pathLength = _path.PathLength();

            if (_lastPathLength >= 0f)
                LengthTravel += Mathf.Abs(pathLength - _lastPathLength);
            else
                _firstPathLength = pathLength;

            if (_path.ContactCount != _lastContacts)
                ContactChanges++;

            _lastContacts = _path.ContactCount;
            _lastPathLength = pathLength;
        }
    }
}
