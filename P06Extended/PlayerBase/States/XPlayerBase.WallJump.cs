namespace P06X
{
    using HarmonyLib;
    using UnityEngine;
    using Helpers;
    using UnityEngine.Assertions;
    using System;

    public partial class XPlayerBase : MonoBehaviour
    {
        public class WallJumpState
        {
            // Static configuration
            public static readonly float MaxWaitTime = 0.75f;
            public static readonly float MinDotNormal = -0.5f;
            public static readonly float MaxDotNormal = 0.5f;
            public static readonly float UpOffset = -0.25f;
            public static readonly float NormalOffset = 0.5f;
            public static readonly Vector3 MeshRotation = new Vector3(90f, 0f, 0f);
            public static readonly float JumpStrength = 25f;
            public static readonly float MinHeightAboveGround = 1f;

            // Instance state
            public bool IsWallJumping = false;
            public float Time;
            public bool IsWaiting;
            public Vector3 Normal;
            public bool OtherCharacter;
        }

        public WallJumpState WallJump = new WallJumpState();

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
                I.Qua["GeneralMeshRotation"] = Quaternion.LookRotation(I.I.transform.forward, I.I.transform.up) * Quaternion.Euler(WallJumpState.MeshRotation);
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

            I.I.transform.position = I.RcH["FrontalHit"].point + I.I.transform.up * WallJumpState.UpOffset + I.RcH["FrontalHit"].normal * ((!WallJump.OtherCharacter) ? WallJumpState.NormalOffset : 0f);
            XSingleton<XDebug>.Instance.DrawVectorFast(base.transform.position, base.transform.position + base.transform.up, Color.blue, 3);

            I.I._Rigidbody.velocity = Vector3.zero;
            I.Boo["LockControls"] = true;
            //I.I.Audio.PlayOneShot(/*"WallLand"*/, I.I.Audio.volume * 0.4f);
        }
        public void StateWallJump()
        {
            I.Boo["LockControls"] = true;
            if (Time.time - WallJump.Time > WallJumpState.MaxWaitTime)
            {
                if (WallJump.OtherCharacter)
                {
                    I.I.transform.position += WallJump.Normal * WallJumpState.NormalOffset;
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
    }
}
