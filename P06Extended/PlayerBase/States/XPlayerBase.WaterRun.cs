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
            if (WaterRun.isWaterRunning) return false; ;
            //if (I.I.GetPrefab("snow_board")) return false;

            Vector3 vector = default(Vector3);
            bool is_falling_and_fast_enough = I.I._Rigidbody.velocity.y < 0f && I.Flt["CurSpeed"] > WaterRun.MinActivationSpeed;
            return is_falling_and_fast_enough && HasWaterBelow(WaterRun.YMaxWaterRaycastDist, ref vector);
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

        public static class WaterRun
        {
            public static bool isWaterRunning;

            public static float YWaterOffset = 0.5f;
            public static float YMaxWaterRaycastDist = 0.501f;

            public static float MinActivationSpeed = 8.5f;

            public static float SpeedBoost = 1.25f;
            public static float AccelTime = 0.65f;

            public static float MinRunAnimationSpeed = 27f;
            public static float RunningBrakeSpeed = 25f;
        }

        //private void X_StateFreeWaterSlideStart()
        //{
        //    this.SetState("WaterSlide");
        //    this.LockControls = true;
        //    this.X_WSTime = Time.time;
        //    this.X_FWSpeedBegin = this.CurSpeed;
        //    this.X_FWSpeedTarget = Mathf.Min(Sonic_New_Lua.c_run_speed_max * 3f, this.CurSpeed * XDebug.Cfg.FWS.SpeedBoost);
        //    this.X_WSpeed = this.CurSpeed;
        //    XDebug.Comment("maybe also lerp offset");
        //}

        //private void X_StateFreeWaterSlide()
        //{
        //    RaycastHit raycastHit;
        //    bool flag = this.X_HasWaterBelow(XDebug.Cfg.FWS.YMaxWaterRaycastDist, out raycastHit);
        //    Vector3 point = raycastHit.point;
        //    Vector3 normal = raycastHit.normal;
        //    bool flag2 = this.IsGrounded();
        //    Vector3 point2 = this.RaycastHit.point;
        //    if (!flag && !flag2)
        //    {
        //        this.X_SwitchToState("StateAir");
        //        return;
        //    }
        //    if (flag2 && !flag)
        //    {
        //        this.X_SwitchToState("StateGround");
        //        return;
        //    }
        //    if (flag2 && flag)
        //    {
        //        float num = Vector3.Distance(base.transform.position, point);
        //        if (Vector3.Distance(base.transform.position, point2) < num)
        //        {
        //            this.X_SwitchToState("StateGround");
        //            return;
        //        }
        //    }
        //    base.transform.position += new Vector3(0f, -base.transform.position.y + point.y + XDebug.Cfg.FWS.YWaterOffset, 0f);
        //    base.transform.rotation = Quaternion.FromToRotation(base.transform.up, normal) * base.transform.rotation;
        //    XDebug.Comment("if we somehow accelerated");
        //    if (this.CurSpeed > this.X_WSpeed)
        //    {
        //        this.X_FWSpeedTarget = (this.X_FWSpeedBegin = (this.X_WSpeed = this.CurSpeed));
        //    }
        //    if (Time.time - this.X_WSTime <= XDebug.Cfg.FWS.AccelTime)
        //    {
        //        float num2 = (Time.time - this.X_WSTime) / XDebug.Cfg.FWS.AccelTime;
        //        this.X_WSpeed = Mathf.Lerp(this.X_FWSpeedBegin, this.X_FWSpeedTarget, Mathf.Sqrt(num2));
        //    }
        //    else if (this.X_WSpeed > 0f)
        //    {
        //        this.X_WSpeed -= 2f * Time.fixedDeltaTime;
        //    }
        //    if (this.X_WSpeed <= 5f)
        //    {
        //        this.StateMachine.ChangeState((StateMachine.PlayerState)base.GetType().GetMethod("StateAir", BindingFlags.Instance | BindingFlags.NonPublic).CreateDelegate(typeof(StateMachine.PlayerState), this));
        //        return;
        //    }
        //    XDebug.Comment("Extra slowdown when 'running'");
        //    if (this.X_WSpeed > XDebug.Cfg.FWS.MinRunAnimationSpeed && Singleton<RInput>.Instance.P.GetAxis("Left Stick Y") <= 0f)
        //    {
        //        this.X_WSpeed -= XDebug.Cfg.FWS.RunningBrakeSpeed * Time.fixedDeltaTime;
        //    }
        //    this.X_StateFreeWaterSlideSetAnimation();
        //    this.CurSpeed = this.X_WSpeed;
        //    this.GeneralMeshRotation = Quaternion.LookRotation(this.ForwardMeshRotation, this.UpMeshRotation);
        //    this._Rigidbody.velocity = base.transform.forward * this.X_WSpeed;
        //}

        //private void X_StateFreeWaterSlideEnd()
        //{
        //    this.MaxRayLenght = 0.75f;
        //    this.LockControls = false;
        //}

        //private void X_StateFreeWaterSlideSetAnimation()
        //{
        //    if (this.X_WSpeed <= 8f)
        //    {
        //        this.PlayAnimation("Edge Danger", "On Edge Danger");
        //        return;
        //    }
        //    if (this.X_WSpeed <= XDebug.Cfg.FWS.MinRunAnimationSpeed)
        //    {
        //        this.Animator.CrossFadeInFixedTime("Brake", 0.04f);
        //        return;
        //    }
        //    this.PlayAnimation("Movement (Blend Tree)", "On Ground");
        //}


        public void StateWaterRunStart()
        {
            // rewrite the X_StateFreeWaterSlideStart() function here
            WaterRun.isWaterRunning = true;
            I.Boo["LockControls"] = true;

        }
        public void StateWaterRun()
        {

        }
        public void StateWaterRunEnd()
        {

        }
    }
}
