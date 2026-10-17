using UnityEngine;
using Zorro.Core;

namespace Repkarbinhake.Plugin.Data
{
    public sealed class VanillaRopeSpool
    {
        private bool _resolved;
        private Item _item;
        private RopeSpool _spool;
        private RopeBoneVisualizer _visualizer;

        public Item Item
        {
            get
            {
                Resolve();
                return _item;
            }
        }

        public RopeSpool Spool
        {
            get
            {
                Resolve();
                return _spool;
            }
        }

        public Material RopeMaterial
        {
            get
            {
                Resolve();
                return _visualizer == null ? null : _visualizer.ropeMaterial;
            }
        }

        private void Resolve()
        {
            if (_resolved)
                return;

            _resolved = true;

            foreach (Item item in SingletonAsset<ItemDatabase>.Instance.itemLookup.Values)
            {
                RopeSpool spool = item == null ? null : item.GetComponent<RopeSpool>();

                if (spool == null || spool.isAntiRope || spool.ropePrefab == null)
                    continue;

                _item = item;
                _spool = spool;
                _visualizer = spool.ropePrefab.GetComponentInChildren<RopeBoneVisualizer>(true);
                return;
            }
        }
    }
}
