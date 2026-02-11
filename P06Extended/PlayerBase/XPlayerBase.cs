namespace P06X
{
    using HarmonyLib;
    using UnityEngine;
    using Helpers;
    using UnityEngine.Assertions;
    using System;

    public partial class XPlayerBase : MonoBehaviour
    {
        public static XPlayerBase XI;

        public XPlayerBase()
        {
            Debug.Log("XPlayerBase constructor calleddddd");
        }

        public class IPlayerBase : ReflectionAccessor<PlayerBase>
        {
            public ReflectionWrapper<RaycastHit> RcH;
            public PlayerCamera Camera;
            public StageManager StageManager;

            public IPlayerBase(PlayerBase playerBase) : base(playerBase)
            {
                RcH = new ReflectionWrapper<RaycastHit>(playerBase);
                Camera = playerBase.Get<PlayerCamera>("Camera");
                StageManager = playerBase.Get<StageManager>("StageManager");
            }
        }
        public static IPlayerBase I;

        public void OnDestroy()
        {
            // decided not to delete, since new playerbase will overwrite the references earlier
            // in the case when there is no new playerbase, the references will not be used anyway? 
            // XI = null;
            // I = null;
            // Debug.Log("Removed reference to XPlayerBase.");
        }


        // ------------------------------- Helpers -------------------------------
        

        private static bool CheckGameState()
        {
            return GameManager.Instance.GameState != GameManager.State.Paused &&
                   I.I.Get<StageManager>("StageManager")
                       .Get<StageManager.State>("StageState") != StageManager.State.Event &&
                   !I.Boo["IsDead"] && I.I.GetState() != "Talk";
        }
    }
}
