using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class HangingCamera
    {
        private const float SmoothTime = 0.18f;
        private const float MaxLag = 0.6f;
        private const float ReleaseDistance = 0.03f;

        private Vector3 _position;
        private Vector3 _velocity;
        private bool _smoothing;

        public void Apply(Transform camera, Vector3 lookDirection, bool tied, bool hanging, float deltaTime)
        {
            if (tied && lookDirection != Vector3.zero)
            {
                camera.rotation = Quaternion.LookRotation(lookDirection);

                if (!hanging)
                    camera.Rotate(GamefeelHandler.instance.GetRotation(), Space.World);
            }

            Smooth(camera, hanging, deltaTime);
        }

        private void Smooth(Transform camera, bool hanging, float deltaTime)
        {
            Vector3 target = camera.position;

            if (!_smoothing)
            {
                if (!hanging)
                    return;

                _smoothing = true;
                _position = target;
                _velocity = Vector3.zero;
            }

            _position = Vector3.SmoothDamp(_position, target, ref _velocity, SmoothTime, Mathf.Infinity, deltaTime);
            Vector3 lag = _position - target;

            if (lag.sqrMagnitude > MaxLag * MaxLag)
                _position = target + lag.normalized * MaxLag;

            if (!hanging && lag.sqrMagnitude < ReleaseDistance * ReleaseDistance)
            {
                _smoothing = false;
                return;
            }

            camera.position = _position;
        }
    }
}
