using P06X.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MelonLoader.MelonLogger;
using static P06X.XSonicNew;

namespace P06X 
{
    public partial class XSonicFast
    {
        public class Stomp
        {
            public bool Destroyed;
            public static readonly float SpeedMult = 2.25f;
        };
        public Stomp StompState = new Stomp();

        public void StateStompStart()
        {
            I.Int["BoundState"] = 42;
            I.Stt["PlayerState"] = SonicFast.State.BoundAttack;

            Vector3 airMotionVelocity = II._Rigidbody.velocity;
            airMotionVelocity.y = ReflectionExtensions.GetLuaStruct("Sonic_New_Lua")
                .Get<float>("c_boundjump_jmp") * 1.35f;
            I.Vec["AirMotionVelocity"] = airMotionVelocity;
            II._Rigidbody.velocity = airMotionVelocity;

            II.Get<AudioSource>("Audio").PlayOneShot(II.BoundStart, 
                II.Get<AudioSource>("Audio").volume * 0.5f);

            XSingleton<XEffects>.Instance.CreateStompFX();

            StompState.Destroyed = false;

            Debug.Log("StateStompStart");
        }

        public void StateStomp()
        {
            I.Qua["GeneralMeshRotation"] = Quaternion.LookRotation(I.Vec["ForwardMeshRotation"], I.Vec["UpMeshRotation"]);
            II.transform.rotation *= Quaternion.FromToRotation(II.transform.up, Vector3.up);

            Vector3 vector = new Vector3(I.Vec["AirMotionVelocity"].x, 0f, I.Vec["AirMotionVelocity"].z);
            if (II._Rigidbody.velocity.magnitude != 0f)
            {
                vector = II.transform.forward * I.Flt["CurSpeed"];
                I.Vec["AirMotionVelocity"] = new Vector3(vector.x, I.Vec["AirMotionVelocity"].y, vector.z);
            }
            II.PlayAnimation("Falling", "On Fall");

            if (II.IsGrounded() && II.InvokeFunc<bool>("ShouldAlignOrFall", false))
            {
                II.Get<AudioSource>("Audio").PlayOneShot(XSingleton<XFiles>.Instance.StompLand,
                    II.Get<AudioSource>("Audio").volume * 0.7f);
                II.AttackSphere_Dir(II.transform.position,
                    ReflectionExtensions.GetLuaStruct("Sonic_New_Lua", "c_boundattack_collision").Get<float>("radius") * 2f, 30f, 1);

                II.StateMachine.ChangeState(II.GetState("StateGround"));
                XSingleton<XEffects>.Instance.DestroyStompFX(true);
                StompState.Destroyed = true;
            }
            else
            {
                II.AttackSphere_Dir(II.transform.position,
                    ReflectionExtensions.GetLuaStruct("Sonic_New_Lua", "c_boundattack_collision")
                    .Get<float>("radius") * 1.25f, 25f, 1);
                Vector3 amv = I.Vec["AirMotionVelocity"];
                amv.y -= 5f * Time.deltaTime;
                I.Vec["AirMotionVelocity"] = amv;
            }
            II._Rigidbody.velocity = I.Vec["AirMotionVelocity"];
            II.DoWallNormal();
        }

        public void StateStompEnd()
        {
            if (!StompState.Destroyed)
            {
                XEffects.Instance.DestroyStompFX(true);
            }
        }
    }
}
