namespace P06X
{
    using HarmonyLib;
    using UnityEngine;
    using Helpers;
    using UnityEngine.Assertions;

    public class XPlayerBase : MonoBehaviour
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
            Debug.DrawLine(I.I.transform.position, I.I.transform.position + I.I.transform.up, Color.blue, 3);
            // XSingleton<XDebug>.Instance.DrawVectorFast(base.transform.position, base.transform.position + base.transform.up, Color.blue, 3);

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


        // ------------------------------- Helpers -------------------------------
        public bool HasGroundBelow(float maxDist)
        {
            RaycastHit raycastHit;
            LayerMask layerMask = I.I.GetPropValue<LayerMask>("Collision_Mask");
            bool result = Physics.Raycast(I.I.transform.position, -I.I.transform.up, out raycastHit, maxDist, layerMask);
            return result;
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

                // TODO: Add check if the wall jump is enabled in the mod settings...

                if (I.I.GetPrefab("omega") && !I.Boo["FrontalCollision"])
                {
                    // Try to raycast further for Omega!
                    I.Boo["FrontalCollision"] = Physics.Raycast(I.I.transform.position + I.I.transform.up * 0.25f, I.I.transform.forward, out RaycastHit frontalHit, 0.4f, I.I.GetPropValue<LayerMask>("FrontalCol_Mask"));
                    I.RcH["FrontalHit"] = frontalHit;
                }

                if (I.I.GetState().IsInList("Jump", "Air", "AfterHoming", "Homing", "Fly", "Glide") &&
                    I.Boo["FrontalCollision"] && I.RcH["FrontalHit"].transform != null &&
                    !Boost.IsBoosting &&  !XI.HasGroundBelow(WallJump.MinHeightAboveGround))
                {
                    if (((I.I.GetPrefab("knuckles") || I.I.GetPrefab("rouge")) && I.RcH["FrontalHit"].transform && I.RcH["FrontalHit"].transform.tag == "ClimbableWall") ||
                        I.I.GetPrefab("sonic_fast") || I.I.GetPrefab("snow_board"))
                    {
                        // Don't switch to wall jump
                        // There's also CanClimb()
                        return false;
                    }
                    // XSingleton<XDebug>.Instance.DrawVectorFast(base.transform.position, base.transform.position + this.FrontalHit.normal, Color.red, 2);
                    Debug.DrawLine(I.I.transform.position, I.RcH["FrontalHit"].normal, Color.red, 2);
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

            public static void Postfix(PlayerBase __instance)
            {
                if (XI == null) return;
                Assert.IsTrue(__instance == I.I, "PlayerBase instance mismatch!");

                if (CanWallJumpJump())
                {
                    I.Flt["CurSpeed"] = WallJump.JumpStrength;
                    I.I.transform.forward = WallJump.Normal;
                    // og note: weird hack to keep vector for jumping in direction opposite to the wall
                    if (I.I._Rigidbody.velocity.y < 3f)
                    {
                        I.I._Rigidbody.velocity += Vector3.up * (3f - I.I._Rigidbody.velocity.y);
                    }
                    I.I.StateMachine.ChangeState(I.I.GetState("StateJump"));
                }
            }
        }
    }
}
