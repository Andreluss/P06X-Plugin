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
        public static class Stomp
        {
            public static bool Destroyed;
            public static readonly float SpeedMult = 2.25f;
        };

        public void StateStompStart()
        {
            II.Set("BoundState", 42);
            II.Set("PlayerState", SonicNew.State.BoundAttack);

            Vector3 airMotionVelocity = II._Rigidbody.velocity;
            float lua_boundjump_jmp = ReflectionExtensions.GetLuaStruct("Sonic_New_Lua").Get<float>("c_boundjump_jmp");
            airMotionVelocity.y = lua_boundjump_jmp * 1.5f;
            II.Set("AirMotionVelocity", airMotionVelocity);
            II._Rigidbody.velocity = airMotionVelocity;

            II.Get<AudioSource>("Audio").PlayOneShot(II.Get<AudioClip>("SpinDashShoot"),
                II.Get<AudioSource>("Audio").volume * 0.5f);
            // XSingleton<XEffects>.Instance.CreateStompFX();

            Stomp.Destroyed = false;
            II.Set("ImmunityTime", Time.time + 9999999f);
            II.Set("BlinkTimer", -9999999f);

            Debug.Log("StateStompStart");
        }

        public void StateStomp()
        {
            II.Set("PlayerState", SonicNew.State.BoundAttack);
            II.Set("GeneralMeshRotation", Quaternion.LookRotation(II.Get<Vector3>("ForwardMeshRotation"), II.Get<Vector3>("UpMeshRotation")));
            II.transform.rotation *= Quaternion.FromToRotation(II.transform.up, Vector3.up);

            if (II._Rigidbody.velocity.magnitude != 0f)
            {
                Vector3 vector = II.transform.forward * II.Get<float>("CurSpeed");
                II.Set("AirMotionVelocity", new Vector3(vector.x, II.Get<Vector3>("AirMotionVelocity").y, vector.z));
            }
            II.PlayAnimation("Falling", "On Fall");

            if (II.IsGrounded() && II.InvokeFunc<bool>("ShouldAlignOrFall", false))
            {
                // audio (optional) todo
                II.InvokeFunc<bool>("AttackSphere_Dir", II.transform.position, 1f * 2f, 30f, 1);
                float axis = XInput.Controls.GetAxis("Left Stick Y");
                float axis2 = XInput.Controls.GetAxis("Left Stick X");
                if (axis != 0f || axis2 != 0f)
                {
                    float num = Mathf.Min(1f, Mathf.Abs(axis) + Mathf.Abs(axis2));
                    II.Set("CurSpeed", II.Get<float>("CurSpeed") * num * Stomp.SpeedMult);

                    II.StateMachine.ChangeState(II.GetState("StateSpinDash"));
                    // XSingleton<XEffects>.Instance.DestroyStompFX(true);
                    Stomp.Destroyed = true;
                }
                else
                {
                    Collider[] array = Physics.OverlapSphere(II.transform.position, 1.5f);
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
                        II.StateMachine.ChangeState(XInstance.StateGetUpX);
                    }
                    II.InvokeFunc<bool>("StunSphere", II.transform.position, 6f, false);
                    //XSingleton<XEffects>.Instance.CreateStompCrashFX(Instance.GetFV<RaycastHit>("RaycastHit"));
                }
            }
            else
            {
                II.InvokeFunc<bool>("AttackSphere_Dir", II.transform.position, 1f * 1.25f, 25f, 1);
                II.Set("AirMotionVelocity", new Vector3(II.Get<Vector3>("AirMotionVelocity").x,
                    II.Get<Vector3>("AirMotionVelocity").y - 5f * Time.deltaTime, II.Get<Vector3>("AirMotionVelocity").z));
            }
            II._Rigidbody.velocity = II.Get<Vector3>("AirMotionVelocity");
            II.DoWallNormal();

            Debug.Log("StateStomp");
        }

        public void StateStompEnd()
        {
            if (!Stomp.Destroyed)
            {
                // XSingleton<XEffects>.Instance.DestroyStompFX(false);
            }
            II.Set("BlinkTimer", -4.5f);
            II.Set("ImmunityTime", Time.time + 0.33f);

            Debug.Log("StateStompEnd");
        }
    }
}
