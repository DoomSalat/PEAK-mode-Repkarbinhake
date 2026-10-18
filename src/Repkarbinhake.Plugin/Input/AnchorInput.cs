using Repkarbinhake.Core.Data;
using Repkarbinhake.Plugin.Data;
using Repkarbinhake.Plugin.Links;
using Repkarbinhake.Plugin.Network;
using Repkarbinhake.Plugin.Visual;
using Repkarbinhake.Unity;
using UnityEngine;

namespace Repkarbinhake.Plugin.Input
{
    public sealed class AnchorInput
    {
        private readonly AnchorBoard _board;
        private readonly AnchorSync _sync;
        private readonly AnchorVisuals _visuals;
        private readonly VanillaRopeSpool _spool;
        private readonly KeyBinding _key;
        private readonly IRopeSettings _settings;
        private AnchorPlacement? _pending;
        private bool _waitForRelease;

        public AnchorInput(AnchorBoard board, AnchorSync sync, AnchorVisuals visuals, VanillaRopeSpool spool, KeyBinding key, IRopeSettings settings)
        {
            _board = board;
            _sync = sync;
            _visuals = visuals;
            _spool = spool;
            _key = key;
            _settings = settings;
        }

        public void Tick(Character local, int actor, Footing footing, Transform view, bool keyTaken)
        {
            bool placed = _board.Has(actor);

            if (placed && (local.data.dead || (!keyTaken && _key.WasPressed) || IsTooFar(local, actor)))
            {
                _sync.RemoveOwn();
                _waitForRelease = true;
                return;
            }

            if (keyTaken)
            {
                _waitForRelease = _key.IsPressed;
                Cancel();
                return;
            }

            if (_waitForRelease)
            {
                _waitForRelease = _key.IsPressed;
                return;
            }

            if (_key.WasReleased && _pending.HasValue)
            {
                _sync.Place(_pending.Value);
                Cancel();
                return;
            }

            if (!placed && _key.IsPressed && footing == Footing.Grounded && local.CanDoInput() && !local.data.dead)
                Aim(local, view);
            else
                Cancel();
        }

        public void Cancel()
        {
            _pending = null;
            _visuals.HideGhost();
        }

        private bool IsTooFar(Character local, int actor)
        {
            Vector3 anchor = _board.Anchors[actor].Position.ToUnity();
            return (anchor - local.Center).sqrMagnitude > _settings.SnapDistance * _settings.SnapDistance;
        }

        private void Aim(Character local, Transform view)
        {
            RopeTier tier = _spool.Tier;
            RaycastHit hit = HelperFunctions.LineCheck(view.position, view.position + view.forward * tier.maxAnchorGhostDistance, HelperFunctions.LayerType.TerrainMap);

            if (hit.collider == null)
            {
                Cancel();
                return;
            }

            Vector3 forward = Vector3.Cross(view.right, hit.normal);
            AnchorPlacement placement = new AnchorPlacement(hit.point.ToNumerics(), forward.ToNumerics(), hit.normal.ToNumerics());
            bool valid = (hit.point - local.Center).sqrMagnitude < tier.maxAnchorDistance * tier.maxAnchorDistance;
            _visuals.ShowGhost(placement, valid);
            _pending = valid ? placement : (AnchorPlacement?)null;
        }
    }
}
