using HarmonyLib;
using P06X.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using static MelonLoader.MelonLogger;
using static P06X.XSonicNew;

namespace P06X
{
    public partial class XSonicFast
    {
        public class SecondJump
        {
            public bool Ready = false;
            public static readonly float speedMult = 0.5f;
        };
        public SecondJump SecondJumpState = new SecondJump();

        private static bool CanSecondJump(SonicFast sonicFast)
        {
            return I.CheckGameState() && XI.SecondJumpState.Ready && I.Stt["PlayerState"].IsIn(SonicFast.State.Jump,
                                                                                               SonicFast.State.Air,
                                                                                               SonicFast.State.BoundAttack,
                                                                                               SonicFast.State.WallSlam)
                  && I.Boo["ReleasedKey"] && XInput.Controls.GetButton("Button A");
        }


        [HarmonyPatch(typeof(SonicFast), "StateJumpStart")]
        public class SonicFast_StateJumpStart
        {
            public static void Postfix(SonicFast __instance)
            {
                XI.SecondJumpState.Ready = true;
            }
        }

        public void StateSecondJumpStart()
        {
            II.InvokeFuncVoid("StateJumpStart");

            SecondJumpState.Ready = false; // it has just been used, so not ready anymore 

            I.Vec["AirMotionVelocity"] *= SecondJump.speedMult;
            II._Rigidbody.velocity *= SecondJump.speedMult;

            XEffects.Instance.CreateSecondJumpFX();
        }

        public void StateSecondJump()
        {
            II.InvokeFuncVoid("StateJump");
        }

        public void StateSecondJumpEnd()
        {
            II.InvokeFuncVoid("StateJumpEnd");
        }

    }
}
