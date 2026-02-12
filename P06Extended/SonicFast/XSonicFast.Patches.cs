namespace P06X
{
    using HarmonyLib;
    using System;
    using UnityEngine;
    using Helpers;
    using UnityEngine.Assertions;

    public partial class XSonicFast : MonoBehaviour // should be a generic character patch subclass (for the future) - todo
    {
        [HarmonyPatch(typeof(SonicFast), "Awake")] // Note: always check if the og object has a Start method!
        public class SonicFast_Awake
        {
            public static void Postfix(SonicFast __instance)
            {
                XI = __instance.gameObject.AddComponent<XSonicFast>();
                I = new ISonicFast(__instance);
                Debug.Log("Added XSonicFast");
            }
        }

        // -------------- Clean up references when being destroyed (Unity Message) --------------
        public void OnDestroy()
        {
            XI = null;
            I = null;
            Debug.Log("Removed reference to XSonicFast because SonicFast is being destoyed!");
        }

        [HarmonyPatch(typeof(SonicFast), "Update")]
        public class SonicFast_Update
        {
            private static bool CanStomp(SonicFast __instance)
            {
                if (!XInput.Controls.GetButtonDown(XInput.REWIRED_B)) return false;

                bool cond1 = Singleton<GameManager>.Instance.GameState != GameManager.State.Paused &&
                             Singleton<GameManager>.Instance.GameState != GameManager.State.Result &&
                             __instance.Get<StageManager>("StageManager")
                                       .Get<StageManager.State>("StageState") != StageManager.State.Event &&
                             !__instance.Get<bool>("IsDead") &&
                             __instance.Get<SonicFast.State>("PlayerState") != SonicFast.State.Talk;

                bool cond2 = __instance.Get<SonicFast.State>("PlayerState").IsInList(
                                SonicFast.State.Spring, SonicFast.State.WideSpring, SonicFast.State.JumpPanel,
                                SonicFast.State.RainbowRing)
                                && !__instance.Get<bool>("LockControls") ||
                             __instance.Get<SonicFast.State>("PlayerState").IsInList(
                                SonicFast.State.Jump, SonicFast.State.Air) ||
                             __instance.Get<SonicFast.State>("PlayerState") == SonicFast.State.BoundAttack 
                                && __instance.Get<int>("BoundState") != 42;

                return cond1 && cond2;
            }
            
            public static void Postfix(SonicFast __instance)
            {
                // ensure the extension code is actually attached
                if (XI == null) return;
                Assert.IsTrue(__instance == II);

                // check the possibile state changes
                if (CanStomp(__instance))
                {
                    __instance.StateMachine.ChangeState(XI.StateStomp);
                }
            }
        }
    }
}
