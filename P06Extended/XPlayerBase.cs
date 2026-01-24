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

        // ------------ Attach this script to the PlayerBase object ------------
        [HarmonyPatch(typeof(PlayerBase), nameof(PlayerBase.Start))]
        public class PlayerBase_Start
        {
            public static void Postfix(PlayerBase __instance)
            {
                XI = __instance.gameObject.AddComponent<XPlayerBase>();
                I = new IPlayerBase(__instance);
                Debug.Log("Added XPlayerBase to PlayerBase object!");
            }
        }
        public void OnDestroy()
        {
            // decided not to delete, since new playerbase will overwrite the references earlier
            // in the case when there is no new playerbase, the references will not be used anyway? 
            //XI = null;
            //I = null;
            //Debug.Log("Removed reference to XPlayerBase.");
        }


        // ------------------------------- States -------------------------------
        // -------------------- Boost --------------------
        public static class Boost
        {
            public static bool IsBoosting;
        }


        // ------------------ Wall Jump ------------------
        public static class WallJump
        {
            public static bool IsWallJumping = false;

            public static float MaxWaitTime = 0.75f;

            public static float MinDotNormal = -0.5f;

            public static float MaxDotNormal = 0.5f;

            public static float UpOffset = -0.25f;

            public static float NormalOffset = 0.5f;

            public static Vector3 MeshRotation = new Vector3(90f, 0f, 0f);

            public static float JumpStrength = 25f;

            public static float MinHeightAboveGround = 1f;

            public static float Time;
            public static bool IsWaiting;
            public static Vector3 Normal;
            public static bool OtherCharacter;
        }
        public void StateWallJumpStart()
        {
            I.I.SetState("Path"); // so you can't jump dash
            WallJump.IsWallJumping = true;

            WallJump.Time = Time.time;
            WallJump.IsWaiting = true;
            WallJump.Normal = I.RcH["FrontalHit"].normal;
            I.I.transform.up = Vector3.up;
            I.I.transform.forward = I.RcH["FrontalHit"].normal;
            
            if (I.I.GetPrefab("sonic_new") || I.I.GetPrefab("shadow") || I.I.GetPrefab("sonic_fast") || I.I.GetPrefab("princess"))
            {
                I.I.PlayAnimation("Chain Jump Wall Wait", "On Chain Jump Wall Wait");
                I.Qua["GeneralMeshRotation"] = Quaternion.LookRotation(I.I.transform.forward, I.I.transform.up) * Quaternion.Euler(WallJump.MeshRotation);
            }
            else if (I.I.GetPrefab("rouge"))
            {
                I.I.PlayAnimation("Crouch", "On Crouch");
                I.Qua["GeneralMeshRotation"] = Quaternion.LookRotation(I.I.transform.forward, I.I.transform.up) * Quaternion.Euler(-90f, 180f, 0f);
            }
            else if (I.I.GetPrefab("omega"))
            {
                I.I.PlayAnimation("Edge Danger", "On Edge Danger");
                I.Qua["GeneralMeshRotation"] = Quaternion.LookRotation(I.I.transform.forward, I.I.transform.up) * Quaternion.Euler(180f, 180f, 180f);
            }
            else
            {
                I.I.PlayAnimation("Up Reel", "On Up Reel");
                I.Qua["GeneralMeshRotation"] = Quaternion.LookRotation(I.I.transform.forward, I.I.transform.up) * Quaternion.Euler(0f, 180f, 0f);
                WallJump.OtherCharacter = true;
            }

            I.I.transform.position = I.RcH["FrontalHit"].point + I.I.transform.up * WallJump.UpOffset + I.RcH["FrontalHit"].normal * ((!WallJump.OtherCharacter) ? WallJump.NormalOffset : 0f);
            XSingleton<XDebug>.Instance.DrawVectorFast(base.transform.position, base.transform.position + base.transform.up, Color.blue, 3);

            I.I._Rigidbody.velocity = Vector3.zero;
            I.Boo["LockControls"] = true;
            //I.I.Audio.PlayOneShot(/*"WallLand"*/, I.I.Audio.volume * 0.4f);
        }
        public void StateWallJump()
        {
            I.Boo["LockControls"] = true;
            if (Time.time - WallJump.Time > WallJump.MaxWaitTime)
            {
                if (WallJump.OtherCharacter)
                {
                    I.I.transform.position += WallJump.Normal * WallJump.NormalOffset;
                }
                I.I.StateMachine.ChangeState(I.I.GetState("StateAir"));
                return;
            }
        }
        public void StateWallJumpEnd()
        {
            WallJump.IsWaiting = false;
            I.Boo["LockControls"] = false;
            WallJump.IsWallJumping = false;
            WallJump.OtherCharacter = false;
        }


        // -------------------- V-Dodge --------------------
        public static class VDodge
        {
            public static bool IsVDodging;
            
            public static int Dir;
            public static string _ButtonName;
            public static bool _ButtonReleased;

            public static float Time;
            public static float EndTime;

            public static bool Stopped;
            internal static float PreDodgeCurSpeed;

            public static int _csc_fix_mode = 2;
            public static Vector3 MaxSideVel;
            public static Vector3 SideCurVel;
            public static Vector3 PreDodgeVel;

            public static Quaternion RotA;
            public static Quaternion RotB;

            public static float RotAngles = 20f;
            public static bool _fwdacc = true;
            public static float VelMult = 0.5f;
            public static float Speed = 22f;
            public static float RotDuration = 0.03f;// 0.1f;
            public static float RotBackDuration = 0.07f; // 0.03f;
            public static float AccDuration = 0.07f;
            public static float Dmin = 0.10f;
            public static float Dmax = 0.4f;
        }
        public Vector3 RealRight()
        {
            Vector3 groundNormal = I.RcH["RaycastHit"].normal;
            if (groundNormal == Vector3.zero) {
                return I.I.transform.right;
            } 
            return Vector3.ProjectOnPlane(Vector3.Cross(groundNormal, I.I.transform.forward), groundNormal).normalized;
        }
        [HarmonyPatch(typeof(Rewired.Player), "GetButton", new Type[] { typeof(string) })]
        public class Rewired_Player_GetButton
        {
            public static void Postfix(Rewired.Player __instance, ref bool __result, string actionName)
            {
                // Take away (hide from camera script) the button press if it's being used as a dodge trigger 
                if (actionName != VDodge._ButtonName || VDodge._ButtonReleased) return;

                // If the button have been released, stop blocking the button.
                if (!__result)
                {
                    VDodge._ButtonReleased = true;
                    return;
                }

                // Otherwise (when the button is still pressed), block it.
                __result = false;
            }
        }
        public void StateVDodgeStart()
        {
            I.I.SetState("Path");
            //I.Boo["LockControls"] = true;
            VDodge.IsVDodging = true;
            VDodge._ButtonReleased = false;
            VDodge.Time = Time.time;
            VDodge.EndTime = VDodge.Time + 99999f;
            VDodge.Stopped = false;
            VDodge.PreDodgeCurSpeed = I.Flt["CurSpeed"];
            VDodge.PreDodgeVel = I.I._Rigidbody.velocity;
            if (I.StageManager._Stage == StageManager.Stage.csc && I.StageManager.StageSection == StageManager.Section.E && I.I.GetPrefab("sonic_fast"))
            {
                if (VDodge._csc_fix_mode == 1)
                {
                    bool flag = Vector3.Dot(I.Camera.transform.forward, I.I._Rigidbody.velocity) < 0f;
                    Vector3 vector = Vector3.ProjectOnPlane(I.Camera.transform.forward * (flag ? -1 : 1), I.RcH["RaycastHit"].normal);
                    I.I.transform.forward = vector;

                    XDebug.Comment("Adjust forward rotation to camera automatically");
                    XDebug.Comment("This works only in Crisis City E --> to prevent sudden flying off the road");
                    if (Vector3.Angle(vector, Vector3.ProjectOnPlane(base.transform.forward, I.RcH["RaycastHit"].normal)) > 5f) {
                        XDebug.Comment(string.Format("<color=#ee6600>Adjusted direction by {0} deg</color>", Vector3.Angle(vector, Vector3.ProjectOnPlane(base.transform.forward, I.RcH["RaycastHit"].normal))));
                    }
                }
                else if (VDodge._csc_fix_mode == 2)
                {
                    I.I.transform.forward = Vector3.ProjectOnPlane(new Vector3(-1f, 0f, 0f), I.RcH["RaycastHit"].normal).normalized;
                }
            }

            Vector3 normalized = Vector3.ProjectOnPlane(Vector3.Cross(I.Vec["UpMeshRotation"], I.Vec["ForwardMeshRotation"]), I.RcH["RaycastHit"].normal).normalized;
            VDodge.MaxSideVel = RealRight() * (float)VDodge.Dir * VDodge.Speed;
            I.Vec["AirMotionVelocity"] = I.I._Rigidbody.velocity;
            
            VDodge.RotA = I.Qua["GeneralMeshRotation"];
            VDodge.RotB = I.Qua["GeneralMeshRotation"] * Quaternion.Euler(0f, 0f, (float)(-VDodge.RotAngles * VDodge.Dir));
            //I.I.Animator.CrossFadeInFixedTime("Light Dash", 0.04f); // TODO: check for each player  
            XSingleton<XEffects>.Instance.CreateDodgeFX();
            //I.I.Audio.PlayOneShot(/*XSingleton<XDebug>.Instance.DodgeClipFull*/"DodgeClipFull", I.I.Audio.volume * 1.2f);
        }
        public void StateVDodge()
        {
            // this is managed by the Rewired_Player_GetButton patch
            //if (!XInput.Controls.GetButton(VDodge._ButtonName)) {
            //    VDodge._ButtonReleased = true;
            //}


            VDodge.Dmin = XDebug.Instance.dbg_floats[0].Value;
            VDodge.AccDuration = XDebug.Instance.dbg_floats[1].Value;
            VDodge.RotDuration = XDebug.Instance.dbg_floats[2].Value;
            VDodge.RotBackDuration = XDebug.Instance.dbg_floats[3].Value;
            VDodge.Speed = XDebug.Instance.dbg_floats[4].Value;

            float elapsed = Time.time - VDodge.Time;
            if (!VDodge.Stopped)
            {
                if (elapsed >= VDodge.Dmax - VDodge.RotBackDuration || 
                   (elapsed >= VDodge.Dmin - VDodge.RotBackDuration && VDodge._ButtonReleased))
                {
                    VDodge.Stopped = true;
                    VDodge.EndTime = Time.time + VDodge.RotBackDuration;
                }
            }

            I.Qua["GeneralMeshRotation"] = Quaternion.LookRotation(I.Vec["ForwardMeshRotation"], I.Vec["UpMeshRotation"]);
            //VDodge.RotA = I.Qua["GeneralMeshRotation"];
            //VDodge.RotB = I.Qua["GeneralMeshRotation"] * Quaternion.Euler(0f, 0f, -VDodge.RotAngles * (float)VDodge.Dir);
            // Start rotating to the side or back to original rotation before the dodge 
            // TODO: check above assignmetns, make no sense to me
            if (elapsed <= VDodge.RotDuration)
            {
                I.Qua["GeneralMeshRotation"] = Quaternion.Slerp(VDodge.RotA, VDodge.RotB, elapsed / VDodge.RotDuration);
            }
            else if (VDodge.EndTime - Time.time <= VDodge.RotBackDuration)
            {
                I.Qua["GeneralMeshRotation"] = Quaternion.Slerp(VDodge.RotB, VDodge.RotA, 1f - (VDodge.EndTime - Time.time) / VDodge.RotBackDuration);
            }
            else
            {
                I.Qua["GeneralMeshRotation"] = VDodge.RotB;
            }
            
            VDodge.MaxSideVel = RealRight() * VDodge.Dir * VDodge.Speed;
            
            float num2; 
            if (Time.time - VDodge.Time <= VDodge.AccDuration)
            {
                VDodge.SideCurVel = Vector3.Slerp(Vector3.zero, VDodge.MaxSideVel, (Time.time - VDodge.Time) / VDodge.AccDuration);
                num2 = Mathf.Lerp(1f, VDodge.VelMult, (Time.time - VDodge.Time) / VDodge.AccDuration);
            }
            else if (VDodge.EndTime - Time.time <= VDodge.AccDuration)
            {
                VDodge.SideCurVel = Vector3.Slerp(VDodge.MaxSideVel, Vector3.zero, 1f - (VDodge.EndTime - Time.time) / VDodge.AccDuration);
                num2 = Mathf.Lerp(VDodge.VelMult, 1f, 1f - (VDodge.EndTime - Time.time) / VDodge.AccDuration);
            }
            else
            {
                VDodge.SideCurVel = VDodge.MaxSideVel;
                num2 = VDodge.VelMult;
            }

            I.I.transform.rotation = Quaternion.FromToRotation(I.I.transform.up, I.RcH["RaycastHit"].normal) * I.I.transform.rotation;

            if (XDebug.Instance.dbg_toggles[0].Value)
            {
                I.Vec["AirMotionVelocity"] = Vector3.ProjectOnPlane(I.I.transform.forward, I.RcH["RaycastHit"].normal) * VDodge.PreDodgeCurSpeed * num2;
            }

            I.I._Rigidbody.velocity = I.Vec["AirMotionVelocity"] + VDodge.SideCurVel;
            I.Camera.transform.position += VDodge.SideCurVel * Time.deltaTime;
            if (Time.time >= VDodge.EndTime)
            {
                XDebug.Comment("|| Vector3.Dot(base.transform.right * (float)this.X_DodgeDir, this._Rigidbody.velocity) < 0.1f)");
                if (I.I.IsGrounded())
                {
                    I.I.StateMachine.ChangeState(I.I.GetState("StateGround"));
                }
                else
                {
                    I.I.StateMachine.ChangeState(I.I.GetState("StateAir"));
                }
            }       
        }
        public void StateVDodgeEnd()
        {
            VDodge.IsVDodging = false;
            I.Boo["LockControls"] = false;
            // here's the original code: (please rewrite it to use the VDodge class members and reflection wrappers and I.I. instance instead of base. ... etc.)
            // rewrite it:
            VDodge.EndTime = Time.time;
            I.Flt["CurSpeed"] = VDodge.PreDodgeCurSpeed;
            if (XDebug.Instance.dbg_toggles[2].Value)
            {
                I.I._Rigidbody.velocity = I.I.transform.forward * I.Vec["AirMotionVelocity"].magnitude;
            }
            else
            {
                I.I._Rigidbody.velocity = VDodge.PreDodgeVel;
            }
            XSingleton<XEffects>.Instance.DestroyDodgeFX();
        }



        // ------------------------------- Helpers -------------------------------
        public bool HasGroundBelow(float maxDist)
        {
            RaycastHit raycastHit;
            LayerMask layerMask = I.I.GetPropValue<LayerMask>("Collision_Mask");
            bool result = Physics.Raycast(I.I.transform.position, -I.I.transform.up, out raycastHit, maxDist, layerMask);
            return result;
        }
        public static bool HasWaterBelow(float maxDist, out RaycastHit waterHit)
        {
            waterHit = default(RaycastHit);
            foreach (RaycastHit raycastHit in Physics.RaycastAll(I.I.transform.position, -Vector3.up, maxDist))
            {
                if (raycastHit.transform.tag == "Water")
                {
                    waterHit = raycastHit;
                    return true;
                }
            }
            return false;
        }
        public static bool HasWaterBelow(float maxDist, ref Vector3 waterPosition)
        {
            RaycastHit[] array = Physics.RaycastAll(I.I.transform.position, -Vector3.up, maxDist);
            bool flag = false;
            foreach (RaycastHit raycastHit in array)
            {
                if (raycastHit.transform.tag == "Water")
                {
                    flag = true;
                    waterPosition = raycastHit.point;
                    break;
                }
            }
            return flag;
        }
        private static bool CheckGameState()
        {
            return GameManager.Instance.GameState != GameManager.State.Paused &&
                   I.I.Get<StageManager>("StageManager")
                       .Get<StageManager.State>("StageState") != StageManager.State.Event &&
                   !I.Boo["IsDead"] && I.I.GetState() != "Talk";
        }


        // -------------------------- FixedUpdate Patch ---------------------------
        [HarmonyPatch(typeof(PlayerBase), nameof(PlayerBase.FixedUpdate))]
        public class PlayerBase_FixedUpdate
        {
            public static bool CanWallJumpStick()
            {
                if (!CheckGameState()) return false;
                if (I.I.GetPrefab("knuckles") || I.I.GetPrefab("rouge")) return false; // this will be done separately in Knuckles and Rouge classes

                // If is wall jumping already - I can't extend the state enum ...
                if (WallJump.IsWallJumping) return false;

                // Check if the wall jump is enabled in the mod settings.
                if (!XDebug.Instance.Moveset_WallJumping.Value) return false;

                if (I.I.GetPrefab("omega") && !I.Boo["FrontalCollision"])
                {
                    // Try to raycast further for Omega!
                    I.Boo["FrontalCollision"] = Physics.Raycast(I.I.transform.position + I.I.transform.up * 0.25f, I.I.transform.forward, out RaycastHit frontalHit, 0.4f, I.I.GetPropValue<LayerMask>("FrontalCol_Mask"));
                    I.RcH["FrontalHit"] = frontalHit;
                }

                if (I.I.GetState().IsInList("Jump", "Air", "AfterHoming", "Homing", "Fly", "Glide") &&
                    I.Boo["FrontalCollision"] && I.RcH["FrontalHit"].transform != null &&
                    !Boost.IsBoosting && !XI.HasGroundBelow(WallJump.MinHeightAboveGround))
                {
                    if (((I.I.GetPrefab("knuckles") || I.I.GetPrefab("rouge")) && I.RcH["FrontalHit"].transform && I.RcH["FrontalHit"].transform.tag == "ClimbableWall") ||
                        I.I.GetPrefab("sonic_fast") || I.I.GetPrefab("snow_board"))
                    {
                        // Don't switch to wall jump
                        // There's also CanClimb()
                        return false;
                    }
                    XSingleton<XDebug>.Instance.DrawVectorFast(I.I.transform.position, I.I.transform.position + I.RcH["FrontalHit"].normal, Color.red, 2);
                    float dot = Vector3.Dot(I.RcH["FrontalHit"].normal, Vector3.up);
                    if (WallJump.MinDotNormal <= dot && I.I._Rigidbody.velocity.y < 0f && dot < WallJump.MaxDotNormal)
                    {
                        return true;
                    }
                }
                return false;
            }


            public static void Postfix(PlayerBase __instance)
            {
                if (XI == null) return;
                Assert.IsTrue(__instance == I.I, "PlayerBase instance mismatch!");

                // Check the possible state changes:
                if (CanWallJumpStick())
                {
                    I.I.StateMachine.ChangeState(XI.StateWallJump);
                }
            }
        }

        // -------------------------- Update Patch ---------------------------
        [HarmonyPatch(typeof(PlayerBase), nameof(PlayerBase.Update))]
        public class PlayerBase_Update
        {
            public static bool CanWallJumpJump()
            {
                if (!WallJump.IsWallJumping) return false;
                if (!CheckGameState()) return false;

                return XInput.Controls.GetButtonDown("Button A");
            }

            public static bool CanVDodge(ref int direction, ref string buttonName)
            {
                if (!CheckGameState()) return false;
                if (VDodge.IsVDodging || Time.time <= VDodge.EndTime) return false;
                if (I.Boo["LockControls"]) return false;
                // SonicNew has its own dodge, so skip. 
                if (I.I.GetPrefab("sonic_new")) return false; 

                // Optionally ensure the player is on the ground. 
                //if (I.I.GetState() != "Ground") return false;

                // Set the dodge direction info, based on the button pressed & camera direction.
                float dot = Vector3.Dot(I.I.transform.forward, I.Camera.transform.forward);
                if (XInput.Controls.GetButtonDown(XInput.REWIRED_RIGHT_BUMPER))
                {
                    direction = ((dot >= 0f) ? 1 : (-1));
                    buttonName = XInput.REWIRED_RIGHT_BUMPER;
                    // XSingleton<XDebug>.Instance.JustUsedLeftTrigger = true;
                    return true;
                }
                else if (XInput.Controls.GetButtonDown(XInput.REWIRED_LEFT_BUMPER))
                {
                    direction = ((dot >= 0f) ? (-1) : 1);
                    buttonName = XInput.REWIRED_LEFT_BUMPER;
                    // XSingleton<XDebug>.Instance.JustUsedLeftTrigger = true;
                    return true;
                }
                return false;
            }

            public static bool CanWaterRun()
            {
                if (!CheckGameState()) return false;
                if (I.I.GetState() == "WaterRun") return false;
                //if (I.I.GetPrefab("snow_board")) return false;

                Vector3 vector = default(Vector3);
                bool is_falling_and_fast_enough = I.I._Rigidbody.velocity.y < 0f && I.Flt["CurSpeed"] > WaterRun.MinActivationSpeed;
                return is_falling_and_fast_enough && HasWaterBelow(WaterRun.YMaxWaterRaycastDist, ref vector);
            }

            public static void Postfix(PlayerBase __instance)
            {
                if (XI == null) return;
                Assert.IsTrue(__instance == I.I, "PlayerBase instance mismatch!");

                if (CanWallJumpJump())
                {
                    I.Flt["CurSpeed"] = WallJump.JumpStrength;
                    I.I.transform.forward = WallJump.Normal;
                    // og note: weird hack to keep vector for jumping in direction opposite to the wall
                    if (I.I._Rigidbody.velocity.y < 3f) {
                        I.I._Rigidbody.velocity += Vector3.up * (3f - I.I._Rigidbody.velocity.y);
                    }
                    I.I.StateMachine.ChangeState(I.I.GetState("StateJump"));
                }

                if (CanVDodge(ref VDodge.Dir, ref VDodge._ButtonName))
                {
                    I.I.StateMachine.ChangeState(XI.StateVDodge);
                }

                if (CanWaterRun())
                {
                    I.I.StateMachine.ChangeState(XI.StateWaterRun);
                }
            }
        }
    }
}
