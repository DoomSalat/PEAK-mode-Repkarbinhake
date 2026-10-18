using Repkarbinhake.Core.Data;
using Repkarbinhake.Core.Physics;

namespace Repkarbinhake.Plugin.Links
{
    public sealed class LinkLengths
    {
        private readonly RopeReel _reel;
        private readonly HookLengths _hooks;

        public LinkLengths(RopeReel reel, HookLengths hooks)
        {
            _reel = reel;
            _hooks = hooks;
        }

        public float EffectiveLength(RopeLink link)
        {
            return _hooks.TryGet(link, out float length) ? length : _reel.EffectiveLength(link);
        }
    }
}
