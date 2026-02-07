namespace P06X
{
    using HarmonyLib;
    using UnityEngine;
    using Helpers;
    using UnityEngine.Assertions;
    using System;

    public partial class XPlayerBase : MonoBehaviour
    {
        public void StateWaterSlideStart()
        {
            // combination of settings to achieve closest behavior
            // to introducing a new enum state `WaterSlide` for all characters (impossible with Harmony)
            XI.WaterSlideState.active = true;
            I.I.SetState("Ground");

            // same behaviour as vanilla (SonicNew's WaterSlide) implementation
            XI.WaterSlideState.WSpeed = Mathf.Min(I.Flt["CurSpeed"], I.Flt["TopSpeed"] * 1.5f);
            XI.WaterSlideState.WSTime = 0f;
            XI.WaterSlideState.WSDirection = transform.forward;
            XI.WaterSlideState.WSSpline = I.I.GetSpline(XI.WaterSlideState.LaunchMode);
            XI.WaterSlideState.WSPositionShift = 0f;
            I.Flt["MaxRayLenght"] = 0.55f;
        }

        public void StateWaterSlide()
        {
            I.I.PlayAnimation("Water Slide", "On Water Slide");
            I.Boo["LockControls"] = true;

            // Decelerate (no IsSuper check for non-Sonic characters)
            if (XI.WaterSlideState.WSpeed > 0f)
            {
                XI.WaterSlideState.WSpeed -= 7.5f * Time.fixedDeltaTime;
            }

            I.Flt["CurSpeed"] = XI.WaterSlideState.WSpeed;

            // Lateral movement along the spline
            float num = Vector3.Dot(I.I.transform.forward, I.Camera.transform.forward);
            float c_waterslider_lr = (float)ReflectionExtensions.GetLuaStruct("Common_Lua").Get<float>("c_waterslider_lr");
            XI.WaterSlideState.WSSmoothPos = Mathf.Lerp(
                XI.WaterSlideState.WSSmoothPos,
                -RInput.Instance.Get<Rewired.Player>("P").GetAxis("Left Stick X") * ((num > 0f) ? 1f : -1f),
                Time.fixedDeltaTime * c_waterslider_lr * 2f
            );
            XI.WaterSlideState.WSPositionShift = Mathf.Clamp(
                XI.WaterSlideState.WSPositionShift + XI.WaterSlideState.WSSmoothPos * Time.fixedDeltaTime,
                -1f, 1f
            );

            // Advance along the spline
            XI.WaterSlideState.WSTime += I.Flt["CurSpeed"] / XI.WaterSlideState.WSSpline.Length() * Time.fixedDeltaTime;

            // Exit conditions
            if (XI.WaterSlideState.WSTime > 1f || (XI.WaterSlideState.WSTime > 0.25f && I.I.IsGrounded()))
            {
                I.I.StateMachine.ChangeState(I.I.GetState("StateGround"));
                return;
            }
            if (XI.WaterSlideState.WSpeed <= 4f)
            {
                I.I.StateMachine.ChangeState(I.I.GetState("StateAir"));
                return;
            }

            // Position and rotation along the spline
            XI.WaterSlideState.WSDirection = XI.WaterSlideState.WSSpline.GetTangent(XI.WaterSlideState.WSTime, true).normalized;
            Vector3 normalized = Vector3.Cross(XI.WaterSlideState.WSDirection, Vector3.up).normalized;
            I.I._Rigidbody.MovePosition(
                XI.WaterSlideState.WSSpline.GetPosition(XI.WaterSlideState.WSTime, true)
                + I.I.transform.up * 0.25f
                + normalized * XI.WaterSlideState.WSPositionShift * 3f
            );

            I.Qua["GeneralMeshRotation"] = Quaternion.LookRotation(I.Vec["ForwardMeshRotation"], I.Vec["UpMeshRotation"]);
            I.I.transform.rotation = Quaternion.LookRotation(XI.WaterSlideState.WSDirection.MakePlanar());
            I.I._Rigidbody.velocity = Vector3.zero;
        }

        public void StateWaterSlideEnd()
        {
            XI.WaterSlideState.active = false;
            I.Flt["MaxRayLenght"] = 0.75f;
        }

    }
}
