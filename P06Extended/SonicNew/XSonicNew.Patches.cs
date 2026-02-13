namespace P06X
{
    using HarmonyLib;
    using System;
    using UnityEngine;
    using Helpers;
    using UnityEngine.Assertions;

    public partial class XSonicNew : MonoBehaviour // should be a generic character patch subclass (for the future) - todo
    {
        [HarmonyPatch(typeof(SonicNew), "Start")] // Note: always check if the og object has a Start method!
        public class SonicNew_Start
        {
            public static void Postfix(SonicNew __instance)
            {
                XInstance = __instance.gameObject.AddComponent<XSonicNew>();
                I = new ISonicNew(__instance);
                Debug.Log("Added XSonicNew to SonicNew. Now you can use eXtended States!");
            }
        }

        // -------------- Clean up references when being destroyed (Unity Message) --------------
        public void OnDestroy()
        {
            XInstance = null;
            I = null;
            Debug.Log("Removed reference to XSonicNew because SonicNew is being destoyed!");
        }

        [HarmonyPatch(typeof(SonicNew), "StateAfterHoming")]
        public class SonicNew_StateAfterHoming
        {
            private static float CurSpeed = 0.0f;
            public static void Prefix(SonicNew __instance)
            {
                CurSpeed = I.Flt["CurSpeed"];
            }

            public static void Postfix(SonicNew __instance)
            {
                if (XDebug.Instance.Moveset_AHMovement.Value)
                {
                    I.Flt["CurSpeed"] = Mathf.Min(CurSpeed, XDebug.Instance.Moveset_AHMovementMaxSpeed.Value);
                }
            }
        }

        [HarmonyPatch(typeof(SonicNew), "Update")]
        public class SonicNew_Update
        {
            private static bool CanStomp(SonicNew __instance)
            {
                if (!XInput.Controls.GetButtonDown(XInput.REWIRED_B)) return false;

                bool cond1 = Singleton<GameManager>.Instance.GameState != GameManager.State.Paused &&
                             Singleton<GameManager>.Instance.GameState != GameManager.State.Result &&
                             __instance.Get<StageManager>("StageManager")
                                       .Get<StageManager.State>("StageState") != StageManager.State.Event &&
                             !__instance.Get<bool>("IsDead") &&
                             __instance.Get<SonicNew.State>("PlayerState") != SonicNew.State.Talk;

                bool cond2 = __instance.Get<SonicNew.State>("PlayerState").IsInList(
                                SonicNew.State.Spring, SonicNew.State.WideSpring, SonicNew.State.JumpPanel,
                                SonicNew.State.RainbowRing, SonicNew.State.GunDriveMove, SonicNew.State.Pole,
                                SonicNew.State.Rope) && !__instance.Get<bool>("LockControls") ||
                             __instance.Get<SonicNew.State>("PlayerState").IsInList(
                                SonicNew.State.Jump, SonicNew.State.Air,
                                SonicNew.State.SlowFall, SonicNew.State.AfterHoming, SonicNew.State.TrickJump) ||
                             __instance.Get<SonicNew.State>("PlayerState") == SonicNew.State.BoundAttack
                                && __instance.Get<int>("BoundState") != 42;

                return cond1 && cond2;
            }

            public static void Postfix(SonicNew __instance)
            {
                // ensure the extension code is actually attached
                if (XInstance == null) return;
                Assert.IsTrue(__instance == II);

                // check the possibile state changes
                if (CanStomp(__instance))
                {
                    __instance.StateMachine.ChangeState(XInstance.StateStomp);
                }
            }
        }
    }
}
