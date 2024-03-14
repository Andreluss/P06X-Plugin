namespace P06X
{
    using HarmonyLib;
    using P06X.Helpers;
    using UnityEngine;

    public class XSonicNew : MonoBehaviour // should be a generic character patch subclass (for the future) - todo
    {
        // Singleton-like pattern, but that can only be created with AddComponent by the game itself.
        public static XSonicNew Instance;

        // -------------- Attach to SonicNew whenever it's created --------------
        [HarmonyPatch(typeof(SonicNew), "Start")] // Note: always check if the og object has a Start method!
        public class SonicNew_Start
        {
            public static void Postfix(SonicNew __instance)
            {
                Instance = __instance.gameObject.AddComponent<XSonicNew>();
                Debug.Log("Added XSonicNew to SonicNew. Now you can use eXtended States!");
            }
        }

        // -------------- Clean up references when being destroyed (Unity Message) --------------
        public void OnDestroy()
        {
            XSonicNew.Instance = null;
            Debug.Log("Removed reference to XSonicNew because SonicNew is being destoyed!");
        }
      

        // -------------- List of states --------------

        // ------ Flying ------
        public void StateFlyingStart()
        {
            Debug.Log("StateFlyingStart");
        }

        public void StateFlying()
        {
            Debug.Log("StateFlying");
        }

        public void StateFlyingEnd()
        {
            Debug.Log("StateFlyingEnd");
        }

        // ------- Stomp -------
        public void StateStompStart()
        {
            Debug.Log("StateStompStart");
        }
        public void StateStomp()
        {
            Debug.Log("StateStomp");
        }
        public void StateStompEnd()
        {
            Debug.Log("StateStompEnd");
        }
        // -------------------------------------------

        // -------------- Add a new state to SonicNew --------------
        [HarmonyPatch(typeof(SonicNew), "Update")]
        public class SonicNew_Update
        {
            private static bool CanStomp(SonicNew __instance)
            {
                if (!XInput.Controls.GetButtonDown(XInput.REWIRED_B)) return false;

                bool cond1 = Singleton<GameManager>.Instance.GameState != GameManager.State.Paused &&
                             Singleton<GameManager>.Instance.GameState != GameManager.State.Result &&
                             __instance.GetFieldValue<StageManager>("StageManager")
                                       .GetFieldValue<StageManager.State>("StageState") != StageManager.State.Event &&
                             !__instance.GetFieldValue<bool>("IsDead") &&
                             __instance.GetFieldValue<SonicNew.State>("PlayerState") != SonicNew.State.Talk;

                bool cond2 = __instance.GetFieldValue<SonicNew.State>("PlayerState").IsInList(
                                SonicNew.State.Spring, SonicNew.State.WideSpring, SonicNew.State.JumpPanel,
                                SonicNew.State.RainbowRing, SonicNew.State.GunDriveMove, SonicNew.State.Pole,
                                SonicNew.State.Rope) && !__instance.GetFieldValue<bool>("LockControls") ||
                             __instance.GetFieldValue<SonicNew.State>("PlayerState").IsInList(
                                SonicNew.State.Jump, SonicNew.State.Air,
                                SonicNew.State.SlowFall, SonicNew.State.AfterHoming, SonicNew.State.TrickJump
                             );

                return cond1 && cond2;
            }
            public static void Postfix(SonicNew __instance)
            {
                //ensure the extension code is actually attached
                if (Instance == null) return;
                Debug.Log("sonicNew update");

                //check the possibile state changes
                if (CanStomp(__instance))
                {
                    __instance.StateMachine.ChangeState(Instance.StateStomp);
                }
            }
        }
    }
}
