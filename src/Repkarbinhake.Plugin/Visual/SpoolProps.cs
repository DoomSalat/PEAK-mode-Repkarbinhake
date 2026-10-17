using System.Collections.Generic;
using Repkarbinhake.Plugin.Data;
using Repkarbinhake.Plugin.Handling;
using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class SpoolProps
    {
        private const string TemplateName = "Repkarbinhake Spool Template";
        private const string PropName = "Repkarbinhake Spool";
        private const string RightHandPoint = "Hand_R";
        private const string LeftHandPoint = "Hand_L";
        private const float HalfWeight = 0.5f;

        private readonly Transform _root;
        private readonly VanillaRopeSpool _spool;
        private readonly RopeHoldPoses _poses;
        private readonly Dictionary<Character, SpoolProp> _props = new Dictionary<Character, SpoolProp>();
        private readonly List<Character> _released = new List<Character>();
        private SpoolProp _template;

        public SpoolProps(Transform root, VanillaRopeSpool spool, RopeHoldPoses poses)
        {
            _root = root;
            _spool = spool;
            _poses = poses;
        }

        public void Render()
        {
            if (_spool.Item == null)
                return;

            foreach (KeyValuePair<Character, RopeHoldPose> pose in _poses.Poses)
            {
                Character character = pose.Key;

                if (character == null || character.data.currentItem != null)
                    continue;

                if (!_props.TryGetValue(character, out SpoolProp prop))
                {
                    prop = Spawn();
                    _props.Add(character, prop);
                }

                Quaternion rotation = Quaternion.LookRotation(character.data.lookDirection, character.data.lookDirection_Up);
                Vector3 hands = (character.GetBodypart(BodypartType.Hand_R).transform.position + character.GetBodypart(BodypartType.Hand_L).transform.position) * HalfWeight;
                prop.Place(hands, rotation, pose.Value.SpoolAngle);
            }

            RemoveReleased();
        }

        public bool TryGetRopeExit(Character character, out Vector3 ropeBase, out Vector3 ropeStart)
        {
            if (_props.TryGetValue(character, out SpoolProp prop))
            {
                ropeBase = prop.RopeBase;
                ropeStart = prop.RopeStart;
                return true;
            }

            ropeBase = Vector3.zero;
            ropeStart = Vector3.zero;
            return false;
        }

        public void Clear()
        {
            foreach (SpoolProp prop in _props.Values)
                Object.Destroy(prop.gameObject);

            _props.Clear();
        }

        private void RemoveReleased()
        {
            _released.Clear();

            foreach (KeyValuePair<Character, SpoolProp> prop in _props)
            {
                if (prop.Key == null || prop.Key.data.currentItem != null || !_poses.Poses.ContainsKey(prop.Key))
                    _released.Add(prop.Key);
            }

            foreach (Character character in _released)
            {
                Object.Destroy(_props[character].gameObject);
                _props.Remove(character);
            }
        }

        private SpoolProp Spawn()
        {
            if (_template == null)
                _template = BuildTemplate();

            SpoolProp prop = Object.Instantiate(_template, _root);
            prop.name = PropName;
            prop.gameObject.SetActive(true);
            return prop;
        }

        private SpoolProp BuildTemplate()
        {
            RopeSpool source = _spool.Spool;
            Transform wheel = null;
            Transform ropeStart = null;
            Transform ropeBase = null;
            GameObject root = CopyVisual(source.transform, _root, source, ref wheel, ref ropeStart, ref ropeBase);
            root.name = TemplateName;
            root.SetActive(false);
            SpoolProp template = root.AddComponent<SpoolProp>();
            template.Initialize(wheel, ropeStart, ropeBase, GripCenter(source.transform));
            return template;
        }

        private Vector3 GripCenter(Transform spool)
        {
            Vector3 sum = Vector3.zero;
            int count = 0;

            foreach (Transform child in spool.GetComponentsInChildren<Transform>(true))
            {
                if (child.name != RightHandPoint && child.name != LeftHandPoint)
                    continue;

                sum += spool.InverseTransformPoint(child.position);
                count++;
            }

            return count == 0 ? Vector3.zero : sum / count;
        }

        private GameObject CopyVisual(Transform source, Transform parent, RopeSpool spool, ref Transform wheel, ref Transform ropeStart, ref Transform ropeBase)
        {
            GameObject copy = new GameObject(source.name) { layer = source.gameObject.layer };
            copy.transform.SetParent(parent, false);
            copy.transform.localPosition = source.localPosition;
            copy.transform.localRotation = source.localRotation;
            copy.transform.localScale = source.localScale;

            if (source.TryGetComponent(out MeshFilter sourceFilter) && source.TryGetComponent(out MeshRenderer sourceRenderer))
            {
                copy.AddComponent<MeshFilter>().sharedMesh = sourceFilter.sharedMesh;
                MeshRenderer copyRenderer = copy.AddComponent<MeshRenderer>();
                copyRenderer.sharedMaterials = sourceRenderer.sharedMaterials;
                copyRenderer.enabled = sourceRenderer.enabled;
            }

            if (source == spool.ropeSpoolTf)
                wheel = copy.transform;

            if (source == spool.ropeStart)
                ropeStart = copy.transform;

            if (source == spool.ropeBase)
                ropeBase = copy.transform;

            foreach (Transform child in source)
            {
                if (child.name != RightHandPoint && child.name != LeftHandPoint)
                    CopyVisual(child, copy.transform, spool, ref wheel, ref ropeStart, ref ropeBase);
            }

            copy.SetActive(source.gameObject.activeSelf);
            return copy;
        }
    }
}
