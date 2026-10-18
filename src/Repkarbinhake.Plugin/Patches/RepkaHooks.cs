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
        public static bool ReleasePrompt { get; set; } // Set: written by RepkaBack every frame, LinkPrompt is the release hint for the held rope
        public static string PullPrompt { get; set; } // Set: written by RepkaBack every frame, null hides the pull hint
        public static string PullKeyPath { get; set; } // Set: written by RepkaBack every frame, input path of the pull key for its icon
        public static string UnwindPrompt { get; set; } // Set: written by RepkaBack every frame, null hides the unwind hint
        public static string UnwindKeyPath { get; set; } // Set: written by RepkaBack every frame, input path of the unwind key for its icon
        public static string LinkKeyPath { get; set; } // Set: written by RepkaBack every frame, input path of the link key for its icon
        public static float HeldRopeLength { get; set; } // Set: written by RepkaBack every frame, zero or less hides the rope gauge
        public static VanillaRopeSpool Spool { get; set; } // Set: assigned once by RepkaBack, read by the hold pose patch
        public static CarabinerHud Hud { get; set; } // Set: assigned once by RepkaBack, driven by the HUD patch
        public static PlayerListBadges Badges { get; set; } // Set: assigned once by RepkaBack, driven by the player list patch
        public static Color? HeldRopeColor { get; set; } // Set: written by RepkaBack every frame, link color of the held rope or null
        public static float HeldRopeTint { get; set; } // Set: written by RepkaBack every frame, how strongly the rope gauge takes that color
        public static RopeGaugeTint GaugeTint { get; set; } // Set: assigned once by RepkaBack, driven by the rope gauge patch
        public static PlayerConnectionLog ConnectionLog { get; set; } // Set: remembered by the connection log patch, the on-screen join/leave feed
        public static HookIndicatorState Hook { get; set; } // Set: written by RepkaBack every frame, your hook state for the indicator by the crosshair
        public static int HandUpTarget { get; set; } // Set: written by RepkaBack every frame, teammate your auto hand-up reaches for (0 = none)
        public static bool LocalHelpless { get; set; } // Set: written by RepkaBack every frame, you hang or lie helpless and may be handed up by a tied teammate
        public static HashSet<int> TiedPartners { get; } = new HashSet<int>(); // Static: refilled by RepkaBack every frame, teammates tied to you by rope
        public static bool StableCamera { get; set; } // Set: written by RepkaBack every frame, keeps the camera off the ragdoll head while tied
        public static bool CameraHanging { get; set; } // Set: written by RepkaBack every frame, you dangle on a taut rope: smooth camera position, no shake
        public static HangingCamera HangingCamera { get; set; } // Set: assigned once by RepkaBack, driven by the camera patch
        public static bool ConfigWindowOpen { get; set; } // Set: written by RepkaBack every frame, frees the cursor and blocks game input
        public static IReadOnlyDictionary<Character, RopeHoldPose> HoldPoses { get; set; } // Set: assigned once by RepkaBack, read by the hold pose patch
    }
}
