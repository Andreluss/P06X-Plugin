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
    public partial class XShadow
    {
        private void StateGetUpXStart()
        {
            I.Stt["PlayerState"] = Shadow.State.Ground;
            Stomp.GetUpTime = Time.time;
        }

        private void StateGetUpX()
        {
            II.PlayAnimation("Tornado Return", "On Tornado Return");
            I.Boo["LockControls"] = false;
            II._Rigidbody.velocity = Vector3.zero;
            I.Qua["GeneralMeshRotation"] = Quaternion.LookRotation(I.Vec["ForwardMeshRotation"], I.Vec["UpMeshRotation"]);
            I.Flt["CurSpeed"] = 0f;

            if (!II.IsGrounded() || Time.time - Stomp.GetUpTime > 0.55f)
            {
                II.StateMachine.ChangeState(II.GetState("StateGround"));
            }
        }

        private void StateGetUpXEnd()
        {
        }
    }
}
