using System.Collections.Generic;
using Repkarbinhake.Plugin.Data;
using Repkarbinhake.Plugin.Handling;
using Repkarbinhake.Plugin.Visual;
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
        public static VanillaRopeSpool Spool { get; set; } // Set: assigned once by RepkaBack, read by the hold pose patch
        public static CarabinerHud Hud { get; set; } // Set: assigned once by RepkaBack, driven by the HUD patch
        public static PlayerListBadges Badges { get; set; } // Set: assigned once by RepkaBack, driven by the player list patch
        public static bool ConfigWindowOpen { get; set; } // Set: written by RepkaBack every frame, frees the cursor and blocks game input
        public static IReadOnlyDictionary<Character, RopeHoldPose> HoldPoses { get; set; } // Set: assigned once by RepkaBack, read by the hold pose patch
    }
}
