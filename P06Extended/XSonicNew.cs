namespace P06X
{
    using HarmonyLib;
    using System;
    using UnityEngine;
    using Helpers;
    using UnityEngine.Assertions;
    using Rewired;

    public class XSonicNew : MonoBehaviour // should be a generic character patch subclass (for the future) - todo
    {
        // Singleton-like pattern, but that can only be created with AddComponent by the game itself.
        public static XSonicNew XInstance;
        public static SonicNew II => I.SonicNew;
        public class ISonicNew
        {
            public class Tps<T>
            {
                /* this class is Beta version - not recommended to use 
                   problem: not tested invalidated _wrap references */ 
                private static ReflectionWrapper<T> _wrap;
                public static ReflectionWrapper<T> Wrap
                {
                    get
                    {
                        if (_wrap == null)
                        {
                            if (XSonicNew.I == null)
                            {
                                Debug.LogError("Trying to wrap when Instance is null!");
                                _wrap = null;
                                return null;
                            }
                            _wrap = new ReflectionWrapper<T>(XSonicNew.I.SonicNew);
                        }
                        return _wrap;
                    }
                }
            }
            public ReflectionWrapper<T> W<T>() => Tps<T>.Wrap;
            // ---------------------
            public SonicNew SonicNew;
            public ReflectionWrapper<int> Int;
            public ReflectionWrapper<float> Flt;
            public ReflectionWrapper<Boolean> Boo;
            public ReflectionWrapper<SonicNew.State> Stt;
            public ReflectionWrapper<Vector3> Vec;
            public ReflectionWrapper<Quaternion> Qua;
            public ReflectionWrapper<PlayerCamera> PCa;
            public ISonicNew(SonicNew sonicNew)
            {
                SonicNew = sonicNew;
                Int = new ReflectionWrapper<int>(sonicNew);
                Flt = new ReflectionWrapper<float>(sonicNew);
                Boo = new ReflectionWrapper<bool>(sonicNew);
                Stt = new ReflectionWrapper<SonicNew.State>(sonicNew);
                Vec = new ReflectionWrapper<Vector3>(sonicNew);
                Qua = new ReflectionWrapper<Quaternion>(sonicNew);
                PCa = new ReflectionWrapper<PlayerCamera>(sonicNew);
            }
        }
        public static ISonicNew I;


        // -------------- Attach to SonicNew whenever it's created --------------
        [HarmonyPatch(typeof(SonicNew), "Start")] // Note: always check if the og object has a Start method!
        public class SonicNew_Start
        {
            public static void Postfix(SonicNew __instance)
            {
                XInstance = __instance.gameObject.AddComponent<XSonicNew>();
                I = new ISonicNew(__instance);
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


        // ------- Stomp -------
        public static class Stomp {
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
                II.InvokeFunc<bool>("AttackSphere_Dir", II.transform.position, 1f * 2f, 30f, 1 );
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



        // -------- Dodge --------  
        // Fix the 3 StateDodge functions to use the new reflection wrapper,
        // but please note that variables beginning with X_ are not part of the original SonicNew class and should be changed to static variables from the Dodge class below.

        public static class Dodge
        {
            public static readonly float VelMult = 1f;
            public static readonly float RotDuration = 0.1f;
            public static readonly float Dist = 3f;
            public static readonly float Duration = 0.15f;
            public static readonly float Delay = 0.0f; // idk
            public static readonly Vector3 RotAngles = new Vector3(0f, 0f, -30f);
            public static readonly float RotBackDuration = RotDuration / 4f;
            public static readonly float Offset = 0.15f;
            public static readonly float Slow = 0f;
            public static readonly float FTime = 0.1f;

            public static float Time;
            public static float PreDodgeCurSpeed;
            public static Vector3 CurrSpeed;
            public static Vector3 BaseSpeed;
            public static int Dir;
            public static Quaternion RotA;
            public static Quaternion RotB;
            public static float NextTime;
            public static string _BumperName; // "Left Bumper" or "Right Bumper"
            public static bool _BumperReleased = true;
        }


        /* TODO: Write the same for the GetButton(int) version ? */
        [HarmonyPatch(typeof(Rewired.Player), "GetButton", new Type[] { typeof(string) })]
        public class Rewired_Player_GetButton
        {
            public static void Postfix(Rewired.Player __instance, ref bool __result, string actionName)
            {
                // Take away (hide from camera script) the button press if it's being used as a dodge trigger 
                if (actionName != Dodge._BumperName || Dodge._BumperReleased) return;

                // If the button have been released, stop blocking the button.
                if (!__result)
                {
                    Dodge._BumperReleased = true;
                    return;
                }

                // Otherwise (when the button is still pressed), block it.
                __result = false;
            }
        }

        public void StateDodgeStart()
        {
            // As long as the Right Bumper is pressed, override the return value of the Singleton<RInput>.Instance.P.GetButton("Left Bumper") to false
            Dodge._BumperReleased = false;

            I.Stt["PlayerState"] = SonicNew.State.Ground;
            Dodge.Time = Time.time;
            I.Boo["LockControls"] = true;
            I.Vec["AirMotionVelocity"] = II._Rigidbody.velocity;
            Dodge.PreDodgeCurSpeed = I.Flt["CurSpeed"];
            Dodge.CurrSpeed = Vector3.zero;
            Dodge.BaseSpeed = Dodge.Dist / Dodge.Duration * II.transform.right * Dodge.Dir;
            II._Rigidbody.velocity = I.Vec["AirMotionVelocity"] * Dodge.VelMult + Dodge.CurrSpeed / 4f;
            // XSingleton<XEffects>.Instance.CreateDodgeFX();
            II.Animator.CrossFadeInFixedTime("Light Dash", 0.04f);

            Dodge.RotA = I.Qua["GeneralMeshRotation"];
            Dodge.RotB = I.Qua["GeneralMeshRotation"] * Quaternion.Euler(Dodge.Dir * Dodge.RotAngles);
            I.PCa["Camera"].transform.position += II._Rigidbody.velocity * Time.deltaTime;

            Dodge.NextTime = Time.time + 9999f;
            //II.Get<AudioSource>("Audio").PlayOneShot(/* DodgeClip */, II.Get<AudioSource>("Audio").volume * 1.5f);
        }
        public void StateDodge()
        {
            float num = Dodge.Time + Dodge.Duration;
            if (Time.time - Dodge.Time <= Dodge.RotDuration)
            {
                I.Qua["GeneralMeshRotation"] = Quaternion.Slerp(Dodge.RotA, Dodge.RotB, (Time.time - Dodge.Time) / Dodge.RotDuration);
            }
            else if (num - Time.time <= Dodge.RotBackDuration)
            {
                I.Qua["GeneralMeshRotation"] = Quaternion.Slerp(Dodge.RotB, Dodge.RotA, 1f - (num - Time.time) / Dodge.RotBackDuration);
            }
            float num2 = (Time.time - Dodge.Time) / Dodge.Duration;
            if (num2 < Dodge.Offset)
            {
                Dodge.CurrSpeed = Vector3.Slerp(Dodge.BaseSpeed * Dodge.Slow, Dodge.BaseSpeed, num2 / Dodge.FTime);
            }
            else if (1f - Dodge.Offset < num2)
            {
                Dodge.CurrSpeed = Vector3.Slerp(Dodge.BaseSpeed, Dodge.BaseSpeed * Dodge.Slow, (num2 - (1f - Dodge.Offset)) / Dodge.Offset);
            }
            else
            {
                Dodge.CurrSpeed = Dodge.BaseSpeed;
            }

            II._Rigidbody.velocity = I.Vec["AirMotionVelocity"] * Dodge.VelMult + Dodge.CurrSpeed;
            I.PCa["Camera"].transform.position += Dodge.CurrSpeed * Time.deltaTime;
        
            if (Time.time - Dodge.Time >= Dodge.Duration || II.InvokeFunc<bool>("ShouldEdgeDanger") || Vector3.Dot(II.transform.right * Dodge.Dir, II._Rigidbody.velocity) < 0.1f)
            {
                II.StateMachine.ChangeState(II.GetState("StateGround"));
            }
        }
        private void StateDodgeEnd()
        {
            I.Boo["LockControls"] = false;
            II._Rigidbody.velocity = II.transform.forward * I.Vec["AirMotionVelocity"].magnitude;
            I.Flt["CurSpeed"] = Dodge.PreDodgeCurSpeed;
            // XSingleton<XEffects>.Instance.DestroyDodgeFX();
            Dodge.NextTime = Time.time + Dodge.Delay;
        }





        // -------------- Update patch --------------
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
            private static bool CanDodge(ref int dodgeDir, ref string buttonName)
            {
                // TODO: add original camera controls etc.
                bool cond1 = Singleton<GameManager>.Instance.GameState != GameManager.State.Paused &&
                             Singleton<GameManager>.Instance.GameState != GameManager.State.Result &&
                             II.Get<StageManager>("StageManager")
                               .Get<StageManager.State>("StageState") != StageManager.State.Event &&
                             !I.Boo["IsDead"] &&
                             I.Stt["PlayerState"] != SonicNew.State.Talk;
                bool cond2 = /*II.Get<SonicNew.State>("PlayerState") == SonicNew.State.Ground &&*/ Time.time >= Dodge.NextTime;
                if (!(cond1 && cond2)) return false;

                if (XInput.Controls.GetButtonDown(XInput.REWIRED_RIGHT_BUMPER))
                {
                    dodgeDir = 1;
                    buttonName = XInput.REWIRED_RIGHT_BUMPER;
                    return true;
                }
                else if (XInput.Controls.GetButtonDown(XInput.REWIRED_LEFT_BUMPER))
                {
                    dodgeDir = -1;
                    buttonName = XInput.REWIRED_LEFT_BUMPER;
                    return true;
                }
                return false;
            }
            public static void Postfix(SonicNew __instance)
            {
                //ensure the extension code is actually attached
                if (XInstance == null) return;
                Assert.IsTrue(__instance == II);
                //Debug.Log("sonicNew update");

                //check the possibile state changes
                if (CanStomp(__instance))
                {
                    __instance.StateMachine.ChangeState(XInstance.StateStomp);
                }
                if (CanDodge(ref Dodge.Dir, ref Dodge._BumperName))
                {
                    __instance.StateMachine.ChangeState(XInstance.StateDodge);
                }

            }
        }
    }
}
