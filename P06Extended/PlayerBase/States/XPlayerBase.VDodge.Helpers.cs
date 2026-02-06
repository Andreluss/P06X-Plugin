namespace P06X
{
    using HarmonyLib;
    using Helpers;
    using UnityEngine;
    using System;

    public partial class XPlayerBase
    {
        #region VDodge Helpers

        public Vector3 RealRight()
        {
            Vector3 groundNormal = I.RcH["RaycastHit"].normal;
            if (groundNormal == Vector3.zero) {
                return I.I.transform.right;
            } 
            return Vector3.ProjectOnPlane(Vector3.Cross(groundNormal, I.I.transform.forward), groundNormal).normalized;
        }

        public static bool CanVDodge(ref int direction, ref string buttonName)
        {
            if (!CheckGameState()) return false;
            if (XI.VDodge.IsVDodging || Time.time <= XI.VDodge.EndTime) return false;
            if (I.Boo["LockControls"]) return false;

            // Optionally ensure the player is on the ground. 
            //if (I.I.GetState() != "Ground") return false;

            // Set the dodge direction info, based on the button pressed & camera direction.
            float dot = Vector3.Dot(I.I.transform.forward, I.Camera.transform.forward);
            if (XInput.Controls.GetButtonDown(XInput.REWIRED_RIGHT_BUMPER))
            {
                direction = ((dot >= 0f) ? 1 : (-1));
                buttonName = XInput.REWIRED_RIGHT_BUMPER;
                // XSingleton<XDebug>.Instance.JustUsedLeftTrigger = true;
                return true;
            }
            else if (XInput.Controls.GetButtonDown(XInput.REWIRED_LEFT_BUMPER))
            {
                direction = ((dot >= 0f) ? (-1) : 1);
                buttonName = XInput.REWIRED_LEFT_BUMPER;
                // XSingleton<XDebug>.Instance.JustUsedLeftTrigger = true;
                return true;
            }
            return false;
        }

        #endregion

        #region VDodge Patch

        [HarmonyPatch(typeof(Rewired.Player), "GetButton", new Type[] { typeof(string) })]
        public class Rewired_Player_GetButton
        {
            public static void Postfix(Rewired.Player __instance, ref bool __result, string actionName)
            {
                // Take away (hide from camera script) the button press if it's being used as a dodge trigger 
                if (XI == null || actionName != XI.VDodge.ButtonName || XI.VDodge.ButtonReleased) return;

                // If the button have been released, stop blocking the button.
                if (!__result)
                {
                    Debug.Log("Dodge Button Released (button name: " + XI.VDodge.ButtonName + ")");
                    XI.VDodge.ButtonReleased = true;
                    return;
                }

                // Otherwise (when the button is still pressed), block it.
                __result = false;
            }
        }

        #endregion
    }
}