using System;
using UnityEngine;

namespace Repkarbinhake.Sandbox
{
    [Serializable]
    public sealed class SandboxClimberColors
    {
        [SerializeField] private Color _idle = new Color(0.55f, 0.6f, 0.7f);
        [SerializeField] private Color _selected = new Color(0.95f, 0.75f, 0.2f);
        [SerializeField] private Color _holding = new Color(0.3f, 0.8f, 0.4f);
        [SerializeField] private Color _unconscious = new Color(0.55f, 0.4f, 0.8f);
        [SerializeField] private Color _dead = new Color(0.2f, 0.2f, 0.2f);

        public Color Idle => _idle;
        public Color Selected => _selected;
        public Color Holding => _holding;
        public Color Unconscious => _unconscious;
        public Color Dead => _dead;
    }
}
