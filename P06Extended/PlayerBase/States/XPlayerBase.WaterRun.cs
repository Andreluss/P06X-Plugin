namespace P06X
{
    using HarmonyLib;
    using UnityEngine;
    using Helpers;
    using UnityEngine.Assertions;
    using System;

    public partial class XPlayerBase : MonoBehaviour
    {
        public class WaterRun
        {
            // Static configuration
            public static readonly float YWaterOffset = 0.5f;
            public static readonly float YMaxWaterRaycastDist = 0.501f;
            public static readonly float MinActivationSpeed = 8.5f;
            public static readonly float SpeedBoost = 1.25f;
            public static readonly float AccelTime = 0.65f;
            public static readonly float MinRunAnimationSpeed = 27f;
            public static readonly float RunningBrakeSpeed = 25f;

            // Instance state
            public bool active;
            public float WSTime;
            public float FWSpeedBegin;
            public float FWSpeedTarget;
            public float WSpeed;
        }

        public WaterRun WaterRunState = new WaterRun();

        public static bool CanWaterRun()
        {
            if (!CheckGameState()) return false;
            if (XI.WaterRunState.active) return false;
            if (I.I.IsGrounded()) return false;
            //if (I.I.GetPrefab("snow_board")) return false;

            Vector3 vector = default(Vector3);
            bool is_falling_and_fast_enough = I.I._Rigidbody.velocity.y < 0f && I.Flt["CurSpeed"] > WaterRun.MinActivationSpeed;
            return is_falling_and_fast_enough && HasWaterBelow(WaterRun.YMaxWaterRaycastDist, ref vector);
        }

        // Can jump (when performing water run)?
        public static bool CanWaterRunJump()
        {
            return XI.WaterRunState.active && XInput.Controls.GetButtonDown("Button A");
        }

        private static void logAllColliders(RaycastHit[] hits)
        {
            foreach (var hit in hits)
            {
                Debug.Log($"Hit collider: {hit.collider.name}, tag: {hit.transform.tag}");
            }
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
            if (flag) logAllColliders(array);
            return flag;
        }

        public void StateWaterRunStart()
        {
            Debug.Log($"State Water Run Start Time : {Time.time}");
            WaterRunState.active = true;
            I.I.SetState("Path");
            I.Boo["LockControls"] = true;
            WaterRunState.WSTime = Time.time;
            WaterRunState.FWSpeedBegin = I.Flt["CurSpeed"];
            // all characters need to be as fast when on water 
            float sonicRunSpeedMax = ReflectionExtensions.GetLuaStruct("Sonic_New_Lua").Get<float>("c_run_speed_max");
            WaterRunState.FWSpeedTarget = Mathf.Min(sonicRunSpeedMax * 3f, I.Flt["CurSpeed"] * WaterRun.SpeedBoost);
            WaterRunState.WSpeed = I.Flt["CurSpeed"];
        }

        public void StateWaterRun()
        {
            RaycastHit raycastHit;
            bool hasWater = HasWaterBelow(WaterRun.YMaxWaterRaycastDist, out raycastHit);
            Vector3 waterPoint = raycastHit.point;
            Vector3 waterNormal = raycastHit.normal;
            bool isGrounded = I.I.IsGrounded();
            Vector3 groundPoint = I.RcH["RaycastHit"].point;

            // Exit conditions
            if (!hasWater && !isGrounded)
            {
                I.I.StateMachine.ChangeState(I.I.GetState("StateAir"));
                return;
            }
            if (isGrounded && !hasWater)
            {
                I.I.StateMachine.ChangeState(I.I.GetState("StateGround"));
                return;
            }
            if (isGrounded && hasWater)
            {
                float waterDist = Vector3.Distance(I.I.transform.position, waterPoint);
                if (Vector3.Distance(I.I.transform.position, groundPoint) < waterDist)
                {
                    I.I.StateMachine.ChangeState(I.I.GetState("StateGround"));
                    return;
                }
            }

            // Snap to water surface
            I.I.transform.position += new Vector3(0f, -I.I.transform.position.y + waterPoint.y + WaterRun.YWaterOffset, 0f);
            I.I.transform.rotation = Quaternion.FromToRotation(I.I.transform.up, waterNormal) * I.I.transform.rotation;

            // If we somehow accelerated (e.g. from a booster)
            if (I.Flt["CurSpeed"] > WaterRunState.WSpeed)
            {
                WaterRunState.FWSpeedTarget = (WaterRunState.FWSpeedBegin = (WaterRunState.WSpeed = I.Flt["CurSpeed"]));
            }

            // Speed: accelerate during AccelTime, then decelerate
            if (Time.time - WaterRunState.WSTime <= WaterRun.AccelTime)
            {
                float t = (Time.time - WaterRunState.WSTime) / WaterRun.AccelTime;
                WaterRunState.WSpeed = Mathf.Lerp(WaterRunState.FWSpeedBegin, WaterRunState.FWSpeedTarget, Mathf.Sqrt(t));
            }
            else if (WaterRunState.WSpeed > 0f)
            {
                WaterRunState.WSpeed -= 2f * Time.fixedDeltaTime;
            }

            // Too slow — fall into water
            if (WaterRunState.WSpeed <= 5f)
            {
                I.I.StateMachine.ChangeState(I.I.GetState("StateAir"));
                return;
            }

            // Extra slowdown when not pushing forward
            if (WaterRunState.WSpeed > WaterRun.MinRunAnimationSpeed
                && Singleton<RInput>.Instance.Get<Rewired.Player>("P").GetAxis("Left Stick Y") <= 0f)
            {
                WaterRunState.WSpeed -= WaterRun.RunningBrakeSpeed * Time.fixedDeltaTime;
            }

            StateWaterRunSetAnimation();
            I.Flt["CurSpeed"] = WaterRunState.WSpeed;
            I.Qua["GeneralMeshRotation"] = Quaternion.LookRotation(I.Vec["ForwardMeshRotation"], I.Vec["UpMeshRotation"]);
            I.I._Rigidbody.velocity = I.I.transform.forward * WaterRunState.WSpeed;
        }

        public void StateWaterRunEnd()
        {
            Debug.Log($"Water Run End time : {Time.time}");
            WaterRunState.active = false;
            I.Flt["MaxRayLenght"] = 0.75f;
            I.Boo["LockControls"] = false;
        }

        private void StateWaterRunSetAnimation()
        {
            if (WaterRunState.WSpeed <= 8f)
            {
                I.I.PlayAnimation("Edge Danger", "On Edge Danger");
                return;
            }
            if (WaterRunState.WSpeed <= WaterRun.MinRunAnimationSpeed)
            {
                I.I.Get<Animator>("Animator").CrossFadeInFixedTime("Brake", 0.04f);
                return;
            }
            I.I.PlayAnimation("Movement (Blend Tree)", "On Ground");
        }
    }
}
