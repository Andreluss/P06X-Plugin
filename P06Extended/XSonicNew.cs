namespace P06X
{
    using HarmonyLib;
    using System;
    using UnityEngine;
    using Helpers;
    using System.Runtime.CompilerServices;
    using UnityEngine.Rendering;

    public class XSonicNew : MonoBehaviour // should be a generic character patch subclass (for the future) - todo
    {
        // Singleton-like pattern, but that can only be created with AddComponent by the game itself.
        public static XSonicNew XInstance;
        public static SonicNew I;

        // -------------- Attach to SonicNew whenever it's created --------------
        [HarmonyPatch(typeof(SonicNew), "Start")] // Note: always check if the og object has a Start method!
        public class SonicNew_Start
        {
            public static void Postfix(SonicNew __instance)
            {
                XInstance = __instance.gameObject.AddComponent<XSonicNew>();
                I = __instance;
                Debug.Log("Added XSonicNew to SonicNew. Now you can use eXtended States!");
            }
        }

        // -------------- Clean up references when being destroyed (Unity Message) --------------
        public void OnDestroy()
        {
            XInstance = null;
            I = null;
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

        // ------- Get Up X (for stomp) -------
        private void StateGetUpXStart()
        {
            I.Set("PlayerState", SonicNew.State.Ground);
            I.Set("GetUpTime", Time.time);
        }

        private void StateGetUpX()
        {
            I.Set("PlayerState", SonicNew.State.Ground);
            I.PlayAnimation("Get Up A", "On Get Up A");
            I.Set("LockControls", false);
            I._Rigidbody.velocity = Vector3.zero;
            I.Set("GeneralMeshRotation", Quaternion.LookRotation(I.Get<Vector3>("ForwardMeshRotation"), I.Get<Vector3>("UpMeshRotation")));
            I.Set("CurSpeed", 0f);
            if (!I.IsGrounded() || Time.time - I.Get<float>("GetUpTime") > 0.55f)
            {
                I.StateMachine.ChangeState(I.GetState("StateGround"));
            }
        }

        private void StateGetUpXEnd()
        {
        }


        // ------- Stomp -------
        public static class Stomp {
            public static bool Destroyed;
            public static readonly float SpeedMult = 2.25f;
        };
        public void StateStompStart()
        {
            I.Set("BoundState", 42);
            I.Set("PlayerState", SonicNew.State.BoundAttack);

            Vector3 airMotionVelocity = I._Rigidbody.velocity;
            float lua_boundjump_jmp = ReflectionExtensions.GetLuaStruct("Sonic_New_Lua").Get<float>("c_boundjump_jmp");
            airMotionVelocity.y = lua_boundjump_jmp * 1.5f;
            I.Set("AirMotionVelocity", airMotionVelocity);
            I._Rigidbody.velocity = airMotionVelocity;
            
            I.Get<AudioSource>("Audio").PlayOneShot(I.Get<AudioClip>("SpinDashShoot"), 
                I.Get<AudioSource>("Audio").volume * 0.5f);
            // XSingleton<XEffects>.Instance.CreateStompFX();

            Stomp.Destroyed = false;
            I.Set("ImmunityTime", Time.time + 9999999f);
            I.Set("BlinkTimer", -9999999f);

            Debug.Log("StateStompStart");
        }
        public void StateStomp()
        {
            I.Set("PlayerState", SonicNew.State.BoundAttack);
            I.Set("GeneralMeshRotation", Quaternion.LookRotation(I.Get<Vector3>("ForwardMeshRotation"), I.Get<Vector3>("UpMeshRotation")));
            I.transform.rotation *= Quaternion.FromToRotation(I.transform.up, Vector3.up);

            if (I._Rigidbody.velocity.magnitude != 0f)
            {
                Vector3 vector = I.transform.forward * I.Get<float>("CurSpeed");
                I.Set("AirMotionVelocity", new Vector3(vector.x, I.Get<Vector3>("AirMotionVelocity").y, vector.z));
            }
            I.PlayAnimation("Falling", "On Fall");

            if (I.IsGrounded() && I.InvokeFunc<bool>("ShouldAlignOrFall", false))
            {
                I.InvokeFunc<bool>("AttackSphere_Dir", I.transform.position, 1f * 2f, 30f, 1 );
                float axis = XInput.Controls.GetAxis("Left Stick Y");
                float axis2 = XInput.Controls.GetAxis("Left Stick X");
                if (axis != 0f || axis2 != 0f)
                {
                    float num = Mathf.Min(1f, Mathf.Abs(axis) + Mathf.Abs(axis2));
                    I.Set("CurSpeed", I.Get<float>("CurSpeed") * num * Stomp.SpeedMult);
                    
                    I.StateMachine.ChangeState(I.GetState("StateSpinDash"));
                    // XSingleton<XEffects>.Instance.DestroyStompFX(true);
                    Stomp.Destroyed = true;
                }
                else
                {
                    Collider[] array = Physics.OverlapSphere(I.transform.position, 1.5f);
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
                        I.StateMachine.ChangeState(I.GetState("StateJump"));
                    }
                    else
                    {
                        I.StateMachine.ChangeState(XInstance.StateGetUpX);
                    }
                    I.InvokeFunc<bool>("StunSphere", I.transform.position, 6f, false);
                    //XSingleton<XEffects>.Instance.CreateStompCrashFX(Instance.GetFV<RaycastHit>("RaycastHit"));
                }
            }
            else
            {
                I.InvokeFunc<bool>("AttackSphere_Dir", I.transform.position, 1f * 1.25f, 25f, 1);
                I.Set("AirMotionVelocity", new Vector3(I.Get<Vector3>("AirMotionVelocity").x, 
                    I.Get<Vector3>("AirMotionVelocity").y - 5f * Time.deltaTime, I.Get<Vector3>("AirMotionVelocity").z));
            }
            I._Rigidbody.velocity = I.Get<Vector3>("AirMotionVelocity");
            I.DoWallNormal();

            Debug.Log("StateStomp");
        }
        public void StateStompEnd()
        {
            if (!Stomp.Destroyed)
            {
                // XSingleton<XEffects>.Instance.DestroyStompFX(false);
            }
            I.Set("BlinkTimer", -4.5f);
            I.Set("ImmunityTime", Time.time + 0.33f);

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
                             __instance.Get<StageManager>("StageManager")
                                       .Get<StageManager.State>("StageState") != StageManager.State.Event &&
                             !__instance.Get<bool>("IsDead") &&
                             __instance.Get<SonicNew.State>("PlayerState") != SonicNew.State.Talk;

                bool cond2 = __instance.Get<SonicNew.State>("PlayerState").IsInList(
                                SonicNew.State.Spring, SonicNew.State.WideSpring, SonicNew.State.JumpPanel,
                                SonicNew.State.RainbowRing, SonicNew.State.GunDriveMove, SonicNew.State.Pole,
                                SonicNew.State.Rope) && !__instance.Get<bool>("LockControls") ||
                             __instance.Get<SonicNew.State>("PlayerState").IsInList(
                                SonicNew.State.Jump, SonicNew.State.Air,
                                SonicNew.State.SlowFall, SonicNew.State.AfterHoming, SonicNew.State.TrickJump) ||
                             __instance.Get<SonicNew.State>("PlayerState") == SonicNew.State.BoundAttack 
                                && __instance.Get<int>("BoundState") != 42;

                return cond1 && cond2;
            }
            public static void Postfix(SonicNew __instance)
            {
                //ensure the extension code is actually attached
                if (XInstance == null) return;
                Debug.Log("sonicNew update");

                //check the possibile state changes
                if (CanStomp(__instance))
                {
                    __instance.StateMachine.ChangeState(XInstance.StateStomp);
                }
            }
        }
    }
}
