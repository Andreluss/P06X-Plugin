namespace P06X
{
    using HarmonyLib;
    using UnityEngine;
    using Helpers;
    using UnityEngine.Assertions;
    using System;

    public partial class XPlayerBase : MonoBehaviour
    {
        public static bool CanWaterRun()
        {
            if (!CheckGameState()) return false;
            if (WaterRun.isWaterRunning) return false;
            if (I.I.IsGrounded()) return false;
            //if (I.I.GetPrefab("snow_board")) return false;

            Vector3 vector = default(Vector3);
            bool is_falling_and_fast_enough = I.I._Rigidbody.velocity.y < 0f && I.Flt["CurSpeed"] > WaterRun.MinActivationSpeed;
            return is_falling_and_fast_enough && HasWaterBelow(WaterRun.YMaxWaterRaycastDist, ref vector);
        }

        // Can jump (when performing water run)?
        public static bool CanWaterRunJump()
        {
            return WaterRun.isWaterRunning && XInput.Controls.GetButtonDown("Button A");
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

        public static class WaterRun
        {
            public static bool isWaterRunning;

            // Configuration
            public static float YWaterOffset = 0.5f;
            public static float YMaxWaterRaycastDist = 0.501f;

            public static float MinActivationSpeed = 8.5f;

            public static float SpeedBoost = 1.25f;
            public static float AccelTime = 0.65f;

            public static float MinRunAnimationSpeed = 27f;
            public static float RunningBrakeSpeed = 25f;

            // Instance state
            public static float WSTime;
            public static float FWSpeedBegin;
            public static float FWSpeedTarget;
            public static float WSpeed;
        }

        public void StateWaterRunStart()
        {
            Debug.Log($"State Water Run Start Time : {Time.time}");
            WaterRun.isWaterRunning = true;
            I.I.SetState("Path");
            I.Boo["LockControls"] = true;
            WaterRun.WSTime = Time.time;
            WaterRun.FWSpeedBegin = I.Flt["CurSpeed"];
            WaterRun.FWSpeedTarget = Mathf.Min(I.Flt["TopSpeed"] * 3f, I.Flt["CurSpeed"] * WaterRun.SpeedBoost);
            WaterRun.WSpeed = I.Flt["CurSpeed"];
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
            if (I.Flt["CurSpeed"] > WaterRun.WSpeed)
            {
                WaterRun.FWSpeedTarget = (WaterRun.FWSpeedBegin = (WaterRun.WSpeed = I.Flt["CurSpeed"]));
            }

            // Speed: accelerate during AccelTime, then decelerate
            if (Time.time - WaterRun.WSTime <= WaterRun.AccelTime)
            {
                float t = (Time.time - WaterRun.WSTime) / WaterRun.AccelTime;
                WaterRun.WSpeed = Mathf.Lerp(WaterRun.FWSpeedBegin, WaterRun.FWSpeedTarget, Mathf.Sqrt(t));
            }
            else if (WaterRun.WSpeed > 0f)
            {
                WaterRun.WSpeed -= 2f * Time.fixedDeltaTime;
            }

            // Too slow — fall into water
            if (WaterRun.WSpeed <= 5f)
            {
                I.I.StateMachine.ChangeState(I.I.GetState("StateAir"));
                return;
            }

            // Extra slowdown when not pushing forward
            if (WaterRun.WSpeed > WaterRun.MinRunAnimationSpeed
                && Singleton<RInput>.Instance.Get<Rewired.Player>("P").GetAxis("Left Stick Y") <= 0f)
            {
                WaterRun.WSpeed -= WaterRun.RunningBrakeSpeed * Time.fixedDeltaTime;
            }

            StateWaterRunSetAnimation();
            I.Flt["CurSpeed"] = WaterRun.WSpeed;
            I.Qua["GeneralMeshRotation"] = Quaternion.LookRotation(I.Vec["ForwardMeshRotation"], I.Vec["UpMeshRotation"]);
            I.I._Rigidbody.velocity = I.I.transform.forward * WaterRun.WSpeed;
        }

        public void StateWaterRunEnd()
        {
            Debug.Log($"Water Run End time : {Time.time}");
            WaterRun.isWaterRunning = false;
            I.Flt["MaxRayLenght"] = 0.75f;
            I.Boo["LockControls"] = false;
        }

        private void StateWaterRunSetAnimation()
        {
            if (WaterRun.WSpeed <= 8f)
            {
                I.I.PlayAnimation("Edge Danger", "On Edge Danger");
                return;
            }
            if (WaterRun.WSpeed <= WaterRun.MinRunAnimationSpeed)
            {
                I.I.Get<Animator>("Animator").CrossFadeInFixedTime("Brake", 0.04f);
                return;
            }
            I.I.PlayAnimation("Movement (Blend Tree)", "On Ground");
        }
    }
}
