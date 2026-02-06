namespace P06X
{
    using HarmonyLib;
    using Helpers;
    using UnityEngine;
    using System;

    public partial class XPlayerBase
    {
        #region VDodge State

        public class VDodgeState
        {
            // Static configuration
            public static /*readonly*/ float RotAngles = 20f;
            public static /*readonly*/ float VelMult = 0.5f;
            public static /*readonly*/ float Speed = 22f;
            public static /*readonly*/ float RotDuration = 0.03f;
            public static /*readonly*/ float RotBackDuration = 0.07f;
            public static /*readonly*/ float AccDuration = 0.08f;
            public static /*readonly*/ float Dmin = 0.12f;
            public static /*readonly*/ float Dmax = 0.35f;
            public static /*readonly*/ int CscFixMode = 2;

            // Instance state
            public bool IsVDodging;
            public int Dir;
            public string ButtonName;
            public bool ButtonReleased;
            public float Time;
            public float EndTime;
            public bool Stopped;
            public float PreDodgeCurSpeed;
            public Vector3 MaxSideVel;
            public Vector3 SideCurVel;
            public Vector3 PreDodgeVel;
            public Quaternion RotA;
            public Quaternion RotB;
        }

        public VDodgeState VDodge = new VDodgeState();

        #endregion

        #region VDodge State Methods

        public void StateVDodgeStart()
        {
            I.I.SetState("Path");
            //I.Boo["LockControls"] = true;
            VDodge.IsVDodging = true;
            VDodge.ButtonReleased = false;
            VDodge.Time = Time.time;
            VDodge.EndTime = VDodge.Time + 99999f;
            VDodge.Stopped = false;
            VDodge.PreDodgeCurSpeed = I.Flt["CurSpeed"];
            VDodge.PreDodgeVel = I.I._Rigidbody.velocity;
            if (I.StageManager._Stage == StageManager.Stage.csc && I.StageManager.StageSection == StageManager.Section.E && I.I.GetPrefab("sonic_fast"))
            {
                if (VDodgeState.CscFixMode == 1)
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
                else if (VDodgeState.CscFixMode == 2)
                {
                    I.I.transform.forward = Vector3.ProjectOnPlane(new Vector3(-1f, 0f, 0f), I.RcH["RaycastHit"].normal).normalized;
                }
            }

            Vector3 normalized = Vector3.ProjectOnPlane(Vector3.Cross(I.Vec["UpMeshRotation"], I.Vec["ForwardMeshRotation"]), I.RcH["RaycastHit"].normal).normalized;
            VDodge.MaxSideVel = RealRight() * (float)VDodge.Dir * VDodgeState.Speed;
            I.Vec["AirMotionVelocity"] = I.I._Rigidbody.velocity;
            
            VDodge.RotA = I.Qua["GeneralMeshRotation"];
            //I.I.Animator.CrossFadeInFixedTime("Light Dash", 0.04f); // TODO: check for each player  
            VDodge.RotB = I.Qua["GeneralMeshRotation"] * Quaternion.Euler(0f, 0f, (float)(-VDodgeState.RotAngles * VDodge.Dir));
            XSingleton<XEffects>.Instance.CreateDodgeFX();
            //I.I.Audio.PlayOneShot(/*XSingleton<XDebug>.Instance.DodgeClipFull*/"DodgeClipFull", I.I.Audio.volume * 1.2f);
        }
        public void StateVDodge()
        {
            // this is managed by the Rewired_Player_GetButton patch
            //if (!XInput.Controls.GetButton(VDodge._ButtonName)) {
            //    VDodge._ButtonReleased = true;
            //}

            //VDodgeState.Dmin = XDebug.Instance.dbg_floats[0].Value;
            //VDodgeState.AccDuration = XDebug.Instance.dbg_floats[1].Value;
            //VDodgeState.RotDuration = XDebug.Instance.dbg_floats[2].Value;
            //VDodgeState.RotBackDuration = XDebug.Instance.dbg_floats[3].Value;
            //VDodgeState.Speed = XDebug.Instance.dbg_floats[4].Value;

            float elapsed = Time.time - VDodge.Time;
            if (!VDodge.Stopped)
            {
                if (elapsed >= VDodgeState.Dmax - VDodgeState.RotBackDuration ||
                   (elapsed >= VDodgeState.Dmin - VDodgeState.RotBackDuration && XI.VDodge.ButtonReleased))
                {
                    VDodge.Stopped = true;
                    VDodge.EndTime = Time.time + VDodgeState.RotBackDuration;
                }
            }

            I.Qua["GeneralMeshRotation"] = Quaternion.LookRotation(I.Vec["ForwardMeshRotation"], I.Vec["UpMeshRotation"]);
            //VDodge.RotA = I.Qua["GeneralMeshRotation"];
            //VDodge.RotB = I.Qua["GeneralMeshRotation"] * Quaternion.Euler(0f, 0f, -VDodge.RotAngles * (float)VDodge.Dir);
            // Start rotating to the side or back to original rotation before the dodge 
            // TODO: check above assignmetns, make no sense to me
            if (elapsed <= VDodgeState.RotDuration)
            {
                I.Qua["GeneralMeshRotation"] = Quaternion.Slerp(VDodge.RotA, VDodge.RotB, elapsed / VDodgeState.RotDuration);
            }
            else if (VDodge.EndTime - Time.time <= VDodgeState.RotBackDuration)
            {
                I.Qua["GeneralMeshRotation"] = Quaternion.Slerp(VDodge.RotB, VDodge.RotA, 1f - (XI.VDodge.EndTime - Time.time) / VDodgeState.RotBackDuration);
            }
            else
            {
                I.Qua["GeneralMeshRotation"] = VDodge.RotB;
            }

            VDodge.MaxSideVel = RealRight() * VDodge.Dir * VDodgeState.Speed;

            float num2;
            if (Time.time - VDodge.Time <= VDodgeState.AccDuration)
            {
                VDodge.SideCurVel = Vector3.Slerp(Vector3.zero, VDodge.MaxSideVel, (Time.time - VDodge.Time) / VDodgeState.AccDuration);
                num2 = Mathf.Lerp(1f, VDodgeState.VelMult, (Time.time - VDodge.Time) / VDodgeState.AccDuration);
            }
            else if (VDodge.EndTime - Time.time <= VDodgeState.AccDuration)
            {
                VDodge.SideCurVel = Vector3.Slerp(VDodge.MaxSideVel, Vector3.zero, 1f - (VDodge.EndTime - Time.time) / VDodgeState.AccDuration);
                num2 = Mathf.Lerp(VDodgeState.VelMult, 1f, 1f - (VDodge.EndTime - Time.time) / VDodgeState.AccDuration);
            }
            else
            {
                VDodge.SideCurVel = VDodge.MaxSideVel;
                num2 = VDodgeState.VelMult;
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

        #endregion
    }
}