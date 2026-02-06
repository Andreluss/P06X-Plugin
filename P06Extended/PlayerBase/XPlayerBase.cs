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

        public class IPlayerBase
        {
            public PlayerBase I;
            public ReflectionWrapper<int> Int;
            public ReflectionWrapper<float> Flt;
            public ReflectionWrapper<bool> Boo;
            public ReflectionWrapper<RaycastHit> RcH;
            public ReflectionWrapper<Quaternion> Qua;
            public PlayerCamera Camera;
            public StageManager StageManager;
            public ReflectionWrapper<Vector3> Vec;
            // when adding new ReflectionWrapper, don't forget to add it to the constructor!!!
            // ...
            public IPlayerBase(PlayerBase playerBase)
            {
                I = playerBase;
                Int = new ReflectionWrapper<int>(I);
                Flt = new ReflectionWrapper<float>(I);
                Boo = new ReflectionWrapper<bool>(I);
                RcH = new ReflectionWrapper<RaycastHit>(I);
                Qua = new ReflectionWrapper<Quaternion>(I);
                Camera = I.Get<PlayerCamera>("Camera");
                StageManager = I.Get<StageManager>("StageManager");
                Vec = new ReflectionWrapper<Vector3>(I);
                // ...
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
