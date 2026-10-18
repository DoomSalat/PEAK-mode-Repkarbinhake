using TMPro;
using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public static class HudPlacement // Static: stateless layout math shared by HUD widgets
    {
        public static void RightOf(RectTransform anchor, TMP_Text anchorLast, RectTransform moved, TMP_Text movedFirst, float gap)
        {
            moved.position = anchor.position;
            anchorLast.ForceMeshUpdate();
            movedFirst.ForceMeshUpdate();
            float anchorRight = anchorLast.transform.TransformPoint(anchorLast.textBounds.max).x;
            float movedLeft = movedFirst.transform.TransformPoint(movedFirst.textBounds.min).x;
            moved.position += Vector3.right * (anchorRight + gap * moved.parent.lossyScale.x - movedLeft);
        }
    }
}
