using System.Collections.Generic;
using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class BeltRing
    {
        private const string FrontBone = "WaistF1";
        private const string LeftBone = "WaistL1";
        private const string RightBone = "WaistR1";
        private const float FallbackHeight = 0.15f;
        private const float FallbackFront = 0.21f;
        private const float FallbackSide = 0.28f;
        private const float BodyReachUp = 0.12f;
        private const float BodyReachDown = 0.3f;

        private readonly Dictionary<Character, Bones> _bones = new Dictionary<Character, Bones>();
        private readonly List<Character> _gone = new List<Character>();

        public Frame FrameOf(Character character)
        {
            Vector3 hip = character.GetBodypart(BodypartType.Hip).transform.position;
            Vector3 up = (character.GetBodypart(BodypartType.Torso).transform.position - hip).normalized;
            Vector3 forward = Vector3.ProjectOnPlane(character.data.lookDirection_Flat, up);

            if (forward.sqrMagnitude < Mathf.Epsilon)
                forward = Vector3.ProjectOnPlane(Vector3.forward, up);

            forward.Normalize();
            Vector3 right = Vector3.Cross(up, forward);
            Bones bones = BonesOf(character);

            if (!bones.Found)
                return new Frame(hip + up * FallbackHeight, up, forward * FallbackFront, -right * FallbackSide, right * FallbackSide);

            Vector3 center = bones.Front.position;
            Vector3 front = bones.FrontEnd.position - center;
            Vector3 left = bones.LeftEnd.position - center;
            Vector3 side = bones.RightEnd.position - center;
            bool mirrored = Vector3.Dot(side, right) < 0f;
            return new Frame(center, up, front, mirrored ? side : left, mirrored ? left : side);
        }

        public void Prune()
        {
            _gone.Clear();

            foreach (Character character in _bones.Keys)
            {
                if (character == null)
                    _gone.Add(character);
            }

            foreach (Character character in _gone)
                _bones.Remove(character);
        }

        private Bones BonesOf(Character character)
        {
            if (_bones.TryGetValue(character, out Bones bones))
                return bones;

            bones = Resolve(character.refs.mainRenderer);

            if (!bones.Found)
                Debug.LogWarning($"{nameof(BeltRing)}: waist bones missing on {character.name}, using a fixed belt");

            _bones[character] = bones;
            return bones;
        }

        private Bones Resolve(SkinnedMeshRenderer body)
        {
            Transform front = null;
            Transform left = null;
            Transform right = null;

            foreach (Transform bone in body.bones)
            {
                if (bone == null)
                    continue;

                switch (bone.name)
                {
                    case FrontBone:
                        front = bone;
                        break;

                    case LeftBone:
                        left = bone;
                        break;

                    case RightBone:
                        right = bone;
                        break;
                }
            }

            return new Bones(front, EndOf(front), EndOf(left), EndOf(right));
        }

        private Transform EndOf(Transform bone)
        {
            return bone != null && bone.childCount > 0 ? bone.GetChild(0) : null;
        }

        public readonly struct Frame
        {
            public Frame(Vector3 center, Vector3 up, Vector3 front, Vector3 left, Vector3 right)
            {
                Center = center;
                Up = up;
                Front = front;
                Left = left;
                Right = right;
            }

            public Vector3 Center { get; }
            public Vector3 Up { get; }
            public Vector3 Front { get; }
            public Vector3 Left { get; }
            public Vector3 Right { get; }

            public Vector3 Offset(float angle)
            {
                return Front * Mathf.Cos(angle) + (angle < 0f ? Left : Right) * Mathf.Abs(Mathf.Sin(angle));
            }

            public Vector3 KeepOutside(Vector3 point, float clearance)
            {
                Vector3 local = point - Center;
                float height = Vector3.Dot(local, Up);

                if (height > BodyReachUp || height < -BodyReachDown)
                    return point;

                Vector3 flat = local - Up * height;
                float frontRadius = Front.magnitude + clearance;
                float sideRadius = (Vector3.Dot(flat, Right) < 0f ? Left : Right).magnitude + clearance;
                Vector3 forward = Vector3.ProjectOnPlane(Front, Up).normalized;
                float along = Vector3.Dot(flat, forward);
                float across = Vector3.Dot(flat, Vector3.Cross(Up, forward));
                float reach = Mathf.Sqrt(along * along / (frontRadius * frontRadius) + across * across / (sideRadius * sideRadius));

                if (reach >= 1f || reach < Mathf.Epsilon)
                    return point;

                return Center + Up * height + flat / reach;
            }
        }

        private readonly struct Bones
        {
            public Bones(Transform front, Transform frontEnd, Transform leftEnd, Transform rightEnd)
            {
                Front = front;
                FrontEnd = frontEnd;
                LeftEnd = leftEnd;
                RightEnd = rightEnd;
            }

            public Transform Front { get; }
            public Transform FrontEnd { get; }
            public Transform LeftEnd { get; }
            public Transform RightEnd { get; }
            public bool Found => Front != null && FrontEnd != null && LeftEnd != null && RightEnd != null;
        }
    }
}
