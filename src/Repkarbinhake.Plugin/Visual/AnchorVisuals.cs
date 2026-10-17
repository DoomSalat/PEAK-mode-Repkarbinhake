using System.Collections.Generic;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Plugin.Data;
using Repkarbinhake.Unity;
using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class AnchorVisuals
    {
        private const string AnchorName = "Repkarbinhake Anchor ";
        private const string GhostName = "Repkarbinhake Anchor Ghost";

        private readonly VanillaRopeSpool _spool;
        private readonly Dictionary<int, RopeAnchor> _anchors = new Dictionary<int, RopeAnchor>();
        private RopeAnchor _ghost;

        public AnchorVisuals(VanillaRopeSpool spool)
        {
            _spool = spool;
        }

        public void Show(int owner, AnchorPlacement placement)
        {
            if (!_anchors.TryGetValue(owner, out RopeAnchor anchor) || anchor == null)
            {
                anchor = CreateSolid(AnchorName + owner);
                _anchors[owner] = anchor;
            }

            Place(anchor.transform, placement);
        }

        public void Hide(int owner)
        {
            if (_anchors.TryGetValue(owner, out RopeAnchor anchor) && anchor != null)
                Object.Destroy(anchor.gameObject);

            _anchors.Remove(owner);
        }

        public bool TryGetRopePoint(int owner, out Vector3 point)
        {
            if (_anchors.TryGetValue(owner, out RopeAnchor anchor) && anchor != null)
            {
                point = anchor.anchorPoint.position;
                return true;
            }

            point = Vector3.zero;
            return false;
        }

        public void ShowGhost(AnchorPlacement placement, bool valid)
        {
            if (_ghost == null)
            {
                _ghost = Create(GhostName);
                _ghost.anchorPoint.gameObject.SetActive(false);
            }

            _ghost.Ghost = !valid;
            Place(_ghost.transform, placement);
        }

        public void HideGhost()
        {
            if (_ghost != null)
                Object.Destroy(_ghost.gameObject);

            _ghost = null;
        }

        public void Clear()
        {
            foreach (RopeAnchor anchor in _anchors.Values)
            {
                if (anchor != null)
                    Object.Destroy(anchor.gameObject);
            }

            _anchors.Clear();
            HideGhost();
        }

        private RopeAnchor Create(string name)
        {
            RopeAnchor anchor = Object.Instantiate(_spool.Tier.anchorPreview).GetComponent<RopeAnchor>();
            anchor.name = name;
            return anchor;
        }

        private RopeAnchor CreateSolid(string name)
        {
            RopeAnchor anchor = Create(name);
            RopeAnchor solid = _spool.Tier.anchorPrefab.GetComponent<RopeAnchor>();
            Transform root = solid.transform;
            Transform part = solid.normalPart.transform;
            Transform body = Object.Instantiate(solid.normalPart, anchor.transform).transform;
            body.SetLocalPositionAndRotation(root.InverseTransformPoint(part.position), Quaternion.Inverse(root.rotation) * part.rotation);
            body.gameObject.SetActive(true);
            anchor.anchorPoint.position = anchor.transform.TransformPoint(root.InverseTransformPoint(solid.anchorPoint.position));
            anchor.normalPart.SetActive(false);
            anchor.ghostPart.SetActive(false);
            return anchor;
        }

        private void Place(Transform anchor, AnchorPlacement placement)
        {
            anchor.SetPositionAndRotation(placement.Position.ToUnity(), Quaternion.LookRotation(placement.Forward.ToUnity(), placement.Up.ToUnity()));
        }
    }
}
