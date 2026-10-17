using System;
using Repkarbinhake.Core.Data;
using Repkarbinhake.Plugin.Data;
using Repkarbinhake.Unity;
using UnityEngine;

namespace Repkarbinhake.Plugin.Handling
{
    public sealed class RopeInteractible : MonoBehaviour, IInteractible
    {
        private RopeLink _link;
        private RopeTubeView _view;
        private Func<RopeLink, bool> _canInteract;
        private Func<RopeLink, bool> _isHeldLocally;
        private Func<RopeLink, string> _partnerName;

        public event Action<RopeLink> Interacted;

        public Vector3 AimPoint { get; private set; }
        public string PartnerName => _partnerName(_link);
        public Color Color { get; private set; } = Color.white;

        public void Initialize(RopeLink link, RopeTubeView view, Func<RopeLink, bool> canInteract, Func<RopeLink, bool> isHeldLocally, Func<RopeLink, string> partnerName)
        {
            _link = link;
            _view = view;
            _canInteract = canInteract;
            _isHeldLocally = isHeldLocally;
            _partnerName = partnerName;
        }

        public void Paint(Color color)
        {
            Color = color;
        }

        public void Aim(Vector3 point)
        {
            AimPoint = point;
        }

        public bool IsInteractible(Character interactor)
        {
            return _canInteract(_link);
        }

        public void Interact(Character interactor)
        {
            Interacted?.Invoke(_link);
        }

        public void HoverEnter()
        {
            _view.Highlight();
        }

        public void HoverExit()
        {
            _view.Unhighlight();
        }

        public Vector3 Center()
        {
            return AimPoint;
        }

        public Transform GetTransform()
        {
            return transform;
        }

        public string GetInteractionText()
        {
            return _isHeldLocally(_link) ? RepkaTexts.Release : RepkaTexts.Grab;
        }

        public string GetName()
        {
            return RepkaTexts.RopeName;
        }
    }
}
