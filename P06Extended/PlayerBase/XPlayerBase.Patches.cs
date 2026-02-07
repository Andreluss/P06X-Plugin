namespace P06X
{
    using HarmonyLib;
    using UnityEngine;
    using Helpers;
    using UnityEngine.Assertions;
    using System;

    public partial class XPlayerBase : MonoBehaviour
    {
        [HarmonyPatch(typeof(PlayerBase), nameof(PlayerBase.Start))]
        public class PlayerBase_Start
        {
            public static void Postfix(PlayerBase __instance)
            {
                XI = __instance.gameObject.AddComponent<XPlayerBase>();
                I = new IPlayerBase(__instance);
                Debug.Log("Added XPlayerBase to PlayerBase object!");
            }
        }

        [HarmonyPatch(typeof(PlayerBase), nameof(PlayerBase.FixedUpdate))]
        public class PlayerBase_FixedUpdate
        {
            public static void Postfix(PlayerBase __instance)
            {
                if (XI == null) return;
                Assert.IsTrue(__instance == I.I, "PlayerBase instance mismatch!");

                // Check the possible state changes:
                if (CanWallJumpStick())
                {
                    I.I.StateMachine.ChangeState(XI.StateWallJump);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerBase), nameof(PlayerBase.Update))]
        public class PlayerBase_Update
        {
            public static void Postfix(PlayerBase __instance)
            {
                if (XI == null) return;
                Assert.IsTrue(__instance == I.I, "PlayerBase instance mismatch!");

                if (CanWallJumpJump())
                {
                    I.Flt["CurSpeed"] = WallJumpState.JumpStrength;
                    I.I.transform.forward = XI.WallJump.Normal;
                    // og note: weird hack to keep vector for jumping in direction opposite to the wall
                    if (I.I._Rigidbody.velocity.y < 3f)
                    {
                        I.I._Rigidbody.velocity += Vector3.up * (3f - I.I._Rigidbody.velocity.y);
                    }
                    I.I.StateMachine.ChangeState(I.I.GetState("StateJump"));
                }

                if (CanVDodge(ref XI.VDodge.Dir, ref XI.VDodge.ButtonName))
                {
                    I.I.StateMachine.ChangeState(XI.StateVDodge);
                }

                if (CanWaterRun())
                {
                    I.I.StateMachine.ChangeState(XI.StateWaterRun);
                }
                else if (CanWaterRunJump())
                {
                    Debug.Log("Performing water run jump!");
                    I.I.StateMachine.ChangeState(I.I.GetState("StateJump"));
                    XDebug.Comment("actually this also works with sonic and effectively replaces his seperate if-check for jumping in his own Update() function");
                }
            }
        }
    }
}
