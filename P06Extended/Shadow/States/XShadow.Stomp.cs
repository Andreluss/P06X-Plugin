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
        public class StompState
        {
            // Static configuration
            public static readonly float SpeedMult = 2.25f;

            // Instance state
            public bool IsStomping;
            public bool Destroyed;
            public float GetUpTime;
        }

        public StompState Stomp = new StompState();

        public void StateStompStart()
        {
            I.Stt["PlayerState"] = Shadow.State.Air;
            I.Vec["AirMotionVelocity"] = II._Rigidbody.velocity;

            Vector3 airMotionVelocity = I.Vec["AirMotionVelocity"];
            airMotionVelocity.y = ReflectionExtensions.GetLuaStruct("Sonic_New_Lua").Get<float>("c_boundjump_jmp") * 1.5f;
            I.Vec["AirMotionVelocity"] = airMotionVelocity;
            II._Rigidbody.velocity = airMotionVelocity;

            II.Get<AudioSource>("Audio").PlayOneShot(II.SpinDashShoot, II.Get<AudioSource>("Audio").volume
                                                                       * 0.5f);
            XSingleton<XEffects>.Instance.CreateStompShadowFX();

            Stomp.Destroyed = false;
            Stomp.IsStomping = true;

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
                    ReflectionExtensions.GetLuaStruct("Sonic_New_Lua", "c_boundattack_collision").Get<float>("radius") * 3f, 30f, 1);

                float axisX = XInput.Controls.GetAxis("Left Stick X");
                float axisY = XInput.Controls.GetAxis("Left Stick Y");
                if (axisY != 0f || axisX != 0f)
                {
                    if (XDebug.FASTER_STOMPDASH)
                    {
                        float num = Mathf.Min(1f, Mathf.Abs(axisY) + Mathf.Abs(axisX));
                        I.Flt["CurSpeed"] *= num * StompState.SpeedMult;
                    }

                    II.StateMachine.ChangeState(II.GetState("StateSpinDash"));
                    XSingleton<XEffects>.Instance.DestroyStompShadowFX(true);
                    Stomp.Destroyed = true;
                }
                else
                {
                    Collider[] array = Physics.OverlapSphere(II.transform.position, 1.7f);
                    bool enemiesHit = false;
                    foreach (Collider collider in array)
                    {
                        if (collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                        {
                            Debug.Log(string.Format("We collided with enemy {0} [tot colliders: {1}", collider, array.Length));
                            enemiesHit = true;
                            break;
                        }
                    }
                    if (enemiesHit)
                    {
                        II.StateMachine.ChangeState(II.GetState("StateJump"));
                    }
                    else
                    {
                        II.StateMachine.ChangeState(XI.StateGetUpX);
                    }
                    II.StunSphere(II.transform.position, 6f, false);
                    XSingleton<XEffects>.Instance.CreateStompCrashShadowFX(I.I.Get<RaycastHit>("RaycastHit"));
                }
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

            Debug.Log("StateStomp");
        }

        public void StateStompEnd()
        {
            Stomp.IsStomping = false;
            if (!Stomp.Destroyed)
            {
                XSingleton<XEffects>.Instance.DestroyStompShadowFX(false);
            }

            Debug.Log("StateStompEnd");
        }
    }
}
