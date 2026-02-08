namespace P06X
{
    using Helpers;
    using UnityEngine;
    using UnityEngine.Assertions;

    public partial class XPlayerBase : MonoBehaviour
    {
        public class WaterRun
        {
            // Static configuration
            public static readonly float YWaterOffset = 0.5f;
            public static readonly float YMaxWaterRaycastDist = 0.501f;
            public static readonly float MinActivationSpeed = 8.5f;
            public static readonly float SpeedBoost = 1.5f;
            public static readonly float AccelTime = 0.65f;
            public static readonly float MinRunAnimationSpeed = 27f;
            public static readonly float RunningBrakeSpeed = 25f;
            public static readonly float RotationLerpSpeed = 5f;

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
            I.I.SetState("Ground");
            I.Boo["LockControls"] = true;
            WaterRunState.WSTime = Time.time;
            WaterRunState.FWSpeedBegin = I.Flt["CurSpeed"];
            WaterRunState.FWSpeedTarget = Mathf.Min(I.Flt["TopSpeed"] * 3f, I.Flt["CurSpeed"] * WaterRun.SpeedBoost);
            WaterRunState.WSpeed = I.Flt["CurSpeed"];
            PlayWaterRunFX();
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
                I.I.SetMachineState("StateAir");
                return;
            }
            if (isGrounded && !hasWater)
            {
                I.I.SetMachineState("StateGround");
                return;
            }
            if (isGrounded && hasWater)
            {
                float waterDist = Vector3.Distance(I.I.transform.position, waterPoint);
                if (Vector3.Distance(I.I.transform.position, groundPoint) < waterDist)
                {
                    I.I.SetMachineState("StateGround");
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
                I.I.SetMachineState("StateAir");
                return;
            }

            // Extra slowdown when not pushing forward
            if (WaterRunState.WSpeed > WaterRun.MinRunAnimationSpeed
                && Singleton<RInput>.Instance.Get<Rewired.Player>("P").GetAxis("Left Stick Y") <= 0f)
            {
                WaterRunState.WSpeed -= WaterRun.RunningBrakeSpeed * Time.fixedDeltaTime;
            }

            RotateIfSnowBoard();
            StateWaterRunSetAnimation();
            I.Flt["CurSpeed"] = WaterRunState.WSpeed;
            I.Qua["GeneralMeshRotation"] = Quaternion.LookRotation(I.Vec["ForwardMeshRotation"], I.Vec["UpMeshRotation"]);
            I.I._Rigidbody.velocity = I.I.transform.forward * WaterRunState.WSpeed;
        }

        public void RotateIfSnowBoard()
        {
            if (!I.I.GetPrefab("snow_board")) return;
            var snowboard = I.I as SnowBoard;
            Assert.IsNotNull(snowboard);
            var targetDirection = snowboard.Get<Vector3>("TargetDirection");
            if (targetDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection, snowboard.transform.up);
                snowboard.transform.rotation = Quaternion.Slerp(
                    snowboard.transform.rotation, targetRotation, Time.fixedDeltaTime * WaterRun.RotationLerpSpeed);
            }
        }

        public void StateWaterRunEnd()
        {
            Debug.Log($"Water Run End time : {Time.time}");
            WaterRunState.active = false;
            I.Flt["MaxRayLenght"] = 0.75f;
            I.Boo["LockControls"] = false;
            StopWaterRunFX();
        }

        private void StateWaterRunSetAnimationSnowboard()
        {
            if (WaterRunState.WSpeed <= 8f)
            {
                I.I.PlayAnimation("Grind", "On Grind");
            }
            else
            {
                I.I.PlayAnimation("Board", "On Board");
            }
        }

        private void StateWaterRunSetAnimation()
        {
            if (I.I.GetPrefab("snow_board"))
            {
                StateWaterRunSetAnimationSnowboard();
                return;
            }

            if (WaterRunState.WSpeed <= 8f)
            {
                I.I.PlayAnimation("Edge Danger", "On Edge Danger");
            }
            else if (WaterRunState.WSpeed <= WaterRun.MinRunAnimationSpeed)
            {
                I.I.Get<Animator>("Animator").CrossFadeInFixedTime("Brake", 0.04f);
            }
            else
            {
                I.I.PlayAnimation("Movement (Blend Tree)", "On Ground");
            }
        }
    }
}
