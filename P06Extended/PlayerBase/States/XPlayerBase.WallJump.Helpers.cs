namespace P06X
{
    using HarmonyLib;
    using UnityEngine;
    using Helpers;
    using UnityEngine.Assertions;
    using System;

    public partial class XPlayerBase : MonoBehaviour
    {
        public bool HasGroundBelow(float maxDist)
        {
            RaycastHit raycastHit;
            LayerMask layerMask = I.I.GetPropValue<LayerMask>("Collision_Mask");
            bool result = Physics.Raycast(I.I.transform.position, -I.I.transform.up, out raycastHit, maxDist, layerMask);
            return result;
        }

        public static bool CanWallJumpStick()
        {
            if (!CheckGameState()) return false;
            if (I.I.GetPrefab("knuckles") || I.I.GetPrefab("rouge")) return false; // this will be done separately in Knuckles and Rouge classes

            // If is wall jumping already - I can't extend the state enum ...
            if (XI.WallJump.IsWallJumping) return false;

            // TODO: Add check if the wall jump is enabled in the mod settings...

            if (I.I.GetPrefab("omega") && !I.Boo["FrontalCollision"])
            {
                // Try to raycast further for Omega!
                I.Boo["FrontalCollision"] = Physics.Raycast(I.I.transform.position + I.I.transform.up * 0.25f, I.I.transform.forward, out RaycastHit frontalHit, 0.4f, I.I.GetPropValue<LayerMask>("FrontalCol_Mask"));
                I.RcH["FrontalHit"] = frontalHit;
            }

            if (I.I.GetState().IsInList("Jump", "Air", "AfterHoming", "Homing", "Fly", "Glide") &&
                I.Boo["FrontalCollision"] && I.RcH["FrontalHit"].transform != null &&
                !XI.BoostState.IsBoosting && !XI.HasGroundBelow(WallJumpState.MinHeightAboveGround))
            {
                if (((I.I.GetPrefab("knuckles") || I.I.GetPrefab("rouge")) && I.RcH["FrontalHit"].transform && I.RcH["FrontalHit"].transform.tag == "ClimbableWall") ||
                    I.I.GetPrefab("sonic_fast") || I.I.GetPrefab("snow_board"))
                {
                    // Don't switch to wall jump
                    // There's also CanClimb()
                    return false;
                }
                XSingleton<XDebug>.Instance.DrawVectorFast(I.I.transform.position, I.I.transform.position + I.RcH["FrontalHit"].normal, Color.red, 2);
                float dot = Vector3.Dot(I.RcH["FrontalHit"].normal, Vector3.up);
                if (WallJumpState.MinDotNormal <= dot && I.I._Rigidbody.velocity.y < 0f && dot < WallJumpState.MaxDotNormal)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CanWallJumpJump()
        {
            if (!XI.WallJump.IsWallJumping) return false;
            if (!CheckGameState()) return false;

            return XInput.Controls.GetButtonDown("Button A");
        }
    }
}
