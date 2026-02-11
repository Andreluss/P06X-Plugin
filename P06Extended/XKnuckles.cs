using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P06X
{
    using HarmonyLib;
    using UnityEngine;
    using Helpers;
    using UnityEngine.Assertions;

    public class XKnuckles : MonoBehaviour // should be a generic character patch subclass (for the future) - todo
    {
        public static XKnuckles XI;
        //public class IKnuckles : ReflectionAccessor<Knuckles>
        //{
        //    public ReflectionWrapper<RaycastHit> RcH;
        //    public PlayerCamera Camera;
        //    public StageManager StageManager;

        //    public IKnuckles(Knuckles knuckles) : base(knuckles)
        //    {
        //        RcH = new ReflectionWrapper<RaycastHit>(knuckles);
        //        Camera = knuckles.Get<PlayerCamera>("Camera");
        //        StageManager = knuckles.Get<StageManager>("StageManager");
        //    }
        //}
        public class IKnuckles
        {
            public Knuckles I;
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
            public IKnuckles(Knuckles knuckles)
            {
                I = knuckles;
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
        public static IKnuckles I;

        // ------------ Attach this script to the Knuckles object ------------
        [HarmonyPatch(typeof(Knuckles), nameof(Knuckles.Awake))]
        public class Knuckles_Awake
        {
            public static void Postfix(Knuckles __instance)
            {
                XI = __instance.gameObject.AddComponent<XKnuckles>();
                I = new IKnuckles(__instance);
                Debug.Log("Added XKnuckles to Knuckles object!");
            }
        }

        // -------------- Clean up references when being destroyed (Unity Message) --------------
        public void OnDestroy()
        {
            XI = null;
            I = null;
            Debug.Log("Removed reference to Xknuckles because Knuckles is being destoyed!");
        }

        // Patch the Knuckles' private void CanClimb(bool CannotAttach = false) method with prefix
        [HarmonyPatch(typeof(Knuckles), "CanClimb")]
        public class Knuckles_CanClimb
        {
            public static bool Prefix(ref bool CannotAttach)
            {
                if (!CannotAttach && I.Boo["FrontalCollision"] && (XDebug.Instance.Moveset_ClimbAll.Value || I.RcH["FrontalHit"].transform.tag == "ClimbableWall") && I.Vec["FrontNormal"] != Vector3.zero && 0.75f > I.Vec["FrontNormal"].y && -0.75f < I.Vec["FrontNormal"].y)
                {
                    I.I.StateMachine.ChangeState(I.I.GetState("StateClimb"));
                    return false; // skip the original method
                }
                return true;
            }
        }

        // Patch the Knuckles' private void StateClimb() method with prefix
        [HarmonyPatch(typeof(Knuckles), "StateClimb")]
        public class Knuckles_StateClimb
        {
            public static void Prefix()
            {
                // make the wall climbable by setting the tag of the climbhit gameobject
                if (I.RcH["ClimbHit"].normal != Vector3.zero && XDebug.Instance.Moveset_ClimbAll.Value)
                {
                    I.RcH["ClimbHit"].transform.tag = "ClimbableWall"; // todo: override the whole StateClimb method instead of this
                }
            }
        }
    }
}
