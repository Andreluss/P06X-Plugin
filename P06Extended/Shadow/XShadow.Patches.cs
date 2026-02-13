namespace P06X
{
    using HarmonyLib;
    using System;
    using UnityEngine;
    using Helpers;
    using UnityEngine.Assertions;

    public partial class XShadow : MonoBehaviour // should be a generic character patch subclass (for the future) - todo
    {
        [HarmonyPatch(typeof(Shadow), "Awake")] // Note: always check if the og object has a Start method!
        public class Shadow_Start
        {
            public static void Postfix(Shadow __instance)
            {
                XI = __instance.gameObject.AddComponent<XShadow>();
                I = new IShadow(__instance);
                Debug.Log("Added XShadow.");
            }
        }

        // -------------- Clean up references when being destroyed (Unity Message) --------------
        public void OnDestroy()
        {
            XI = null;
            I = null;
            Debug.Log("Removed reference to XShadow because Shadow is being destroyed!");
        }

        [HarmonyPatch(typeof(Shadow), "Update")]
        public class Shadow_Update
        {
            private static bool CanStomp(Shadow __instance)
            {
                if (!XInput.Controls.GetButtonDown(XInput.REWIRED_B)) return false;

                bool cond1 = Singleton<GameManager>.Instance.GameState != GameManager.State.Paused &&
                             Singleton<GameManager>.Instance.GameState != GameManager.State.Result &&
                             __instance.Get<StageManager>("StageManager")
                                       .Get<StageManager.State>("StageState") != StageManager.State.Event &&
                             !__instance.Get<bool>("IsDead") &&
                             __instance.Get<Shadow.State>("PlayerState") != Shadow.State.Talk;

                bool cond2 = __instance.Get<Shadow.State>("PlayerState").IsInList(
                                Shadow.State.Spring, Shadow.State.WideSpring, Shadow.State.JumpPanel,
                                Shadow.State.RainbowRing, Shadow.State.Pole,
                                Shadow.State.Rope) && !__instance.Get<bool>("LockControls") ||
                             __instance.Get<Shadow.State>("PlayerState").IsInList(
                                Shadow.State.Jump, Shadow.State.Air,
                                Shadow.State.SlowFall, Shadow.State.AfterHoming, Shadow.State.TrickJump);

                return cond1 && cond2;
            }

            public static void Postfix(Shadow __instance)
            {
                // ensure the extension code is actually attached
                if (XI == null) return;
                Assert.IsTrue(__instance == II);

                // check the possible state changes
                if (CanStomp(__instance))
                {
                    __instance.StateMachine.ChangeState(XI.StateStomp);
                }
            }
        }


        [HarmonyPatch(typeof(Shadow), "StateAfterHoming")]
        public class Shadow_StateAfterHoming
        {
            private static float CurSpeed = 0.0f;
            public static void Prefix(Shadow __instance)
            {
                CurSpeed = I.Flt["CurSpeed"];
            }

            public static void Postfix(Shadow __instance)
            {
                if (XDebug.Instance.Moveset_AHMovement.Value)
                {
                    I.Flt["CurSpeed"] = Mathf.Min(CurSpeed, XDebug.Instance.Moveset_AHMovementMaxSpeed.Value);
                }
            }
        }
    }
}
