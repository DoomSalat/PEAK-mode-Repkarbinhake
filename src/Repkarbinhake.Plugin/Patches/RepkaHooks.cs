using System.Collections.Generic;
using BepInEx.Logging;
using Repkarbinhake.Plugin.Data;
using Repkarbinhake.Plugin.Handling;
using Repkarbinhake.Plugin.Links;
using UnityEngine;

namespace Repkarbinhake.Plugin.Patches
{
    public static class RepkaHooks // Static: Harmony patches are static and can reach mod state only through statics
    {
        public static RopeInteractible AimedRope { get; set; } // Set: written by RepkaBack every frame, read by the interaction patch
        public static RopeInteractible HeldRope { get; set; } // Set: written by RepkaBack every frame, wins the interact key over everything
        public static bool ShowCarabinerReticle { get; set; } // Set: written by RepkaBack every frame, read by the reticle patch
        public static float LinkHoldProgress { get; set; } // Set: written by RepkaBack every frame, read by the progress ring patch
        public static string LinkPrompt { get; set; } // Set: written by RepkaBack every frame, null hides the prompt
        public static string LinkKeyPath { get; set; } // Set: written by RepkaBack every frame, input path of the link key for its icon
        public static float HeldRopeLength { get; set; } // Set: written by RepkaBack every frame, zero or less hides the rope gauge
        public static Sprite CarabinerSprite { get; set; } // Set: assigned once by the plugin after loading assets
        public static ManualLogSource Log { get; set; } // Set: assigned once by the plugin, patches have no logger of their own
        public static VanillaRopeSpool Spool { get; set; } // Set: assigned once by RepkaBack, read by the hold pose patch
        public static LinkColors LinkColors { get; set; } // Set: assigned once by RepkaBack, read by the player list patch
        public static IReadOnlyDictionary<Character, RopeHoldPose> HoldPoses { get; set; } // Set: assigned once by RepkaBack, read by the hold pose patch
    }
}
