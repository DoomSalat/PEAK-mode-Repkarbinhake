using System.Collections.Generic;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Physics;
using Repkarbinhake.Core.Visual;
using Repkarbinhake.Plugin.Handling;
using Repkarbinhake.Unity;
using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class RopePresenter
    {
        private readonly RopeWrapPath _path = new RopeWrapPath();
        private readonly RopeSag _sag = new RopeSag();
        private readonly List<System.Numerics.Vector3> _points = new List<System.Numerics.Vector3>();
        private readonly List<System.Numerics.Vector3> _renderPoints = new List<System.Numerics.Vector3>();
        private readonly RopeTubeView _view;
        private readonly MeshRenderer _renderer;

        public RopePresenter(RopeLink link, RopeTubeView view, MeshRenderer renderer, RopeInteractible interactible)
        {
            Link = link;
            _view = view;
            _renderer = renderer;
            Interactible = interactible;
        }

        public RopeLink Link { get; }
        public RopeInteractible Interactible { get; }
        public int SegmentCount => _points.Count;
        public int ContactCount => _path.ContactCount;
        public IReadOnlyList<System.Numerics.Vector3> Points => _points;

        public void Tick(Vector3 start, Vector3 end, RopeLengthState length, int segments, float deltaTime, float radius, IRopeObstacles obstacles)
        {
            _renderer.enabled = true;
            _path.Update(start.ToNumerics(), end.ToNumerics(), length, radius, deltaTime, obstacles);
            _sag.Build(_path, Physics.gravity.normalized.ToNumerics(), segments, radius, deltaTime, obstacles, _points);
        }

        public void Render(Vector3? startLead, Vector3? endLead)
        {
            _renderPoints.Clear();

            if (startLead.HasValue)
                _renderPoints.Add(startLead.Value.ToNumerics());

            _renderPoints.AddRange(_points);

            if (endLead.HasValue)
                _renderPoints.Add(endLead.Value.ToNumerics());

            _view.Render(_renderPoints);
        }

        public void Hide()
        {
            _renderer.enabled = false;
            _path.Clear();
        }

        public bool TrySpan(bool fromStart, out RopeSpan span)
        {
            return _path.TrySpan(fromStart, out span);
        }

        public bool TryPick(RopeRayPicker picker, Ray ray, float reach, float tolerance, out float distance)
        {
            return picker.TryPick(ray.origin.ToNumerics(), ray.direction.ToNumerics(), reach, tolerance, _points, out distance);
        }

        public void Destroy()
        {
            Object.Destroy(_view.gameObject);
        }
    }
}
