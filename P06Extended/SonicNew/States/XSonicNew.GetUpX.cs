using P06X.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MelonLoader.MelonLogger;

namespace P06X
{
    public partial class XSonicNew
    {
        private void StateGetUpXStart()
        {
            //I.Set("PlayerState", SonicNew.State.Ground);
            //I.Set("GetUpTime", Time.time);

            I.Stt["PlayerState"] = SonicNew.State.Ground;
            I.Flt["GetUpTime"] = Time.time;

            //SN.W<SonicNew.State>()["PlayerState"] = SonicNew.State.Ground;
            //SN.W<float>()["GetUpTime"] = Time.time;
        }

        private void StateGetUpX()
        {
            I.Stt["PlayerState"] = SonicNew.State.Ground;
            II.PlayAnimation("Get Up A", "On Get Up A");
            I.Boo["LockControls"] = false;
            II._Rigidbody.velocity = Vector3.zero;
            I.Qua["GeneralMeshRotation"] = Quaternion.LookRotation(I.Vec["ForwardMeshRotation"], I.Vec["UpMeshRotation"]);
            I.Flt["CurSpeed"] = 0f;

            if (!II.IsGrounded() || Time.time - I.Flt["GetUpTime"] > 0.55f)
            {
                II.StateMachine.ChangeState(II.GetState("StateGround"));
            }
        }

        private void StateGetUpXEnd()
        {
        }
    }
}
