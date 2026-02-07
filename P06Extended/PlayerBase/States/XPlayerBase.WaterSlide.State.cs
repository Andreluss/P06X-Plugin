namespace P06X
{
    using HarmonyLib;
    using UnityEngine;
    using Helpers;
    using UnityEngine.Assertions;
    using System;

    public partial class XPlayerBase : MonoBehaviour
    {
        public class WaterSlide
        {
            public bool active = false;
            public float WSpeed;
            public float WSTime;
            public Vector3 WSDirection;
            public BezierCurve WSSpline;
            public float WSPositionShift;
            public float WSSmoothPos;
            internal string LaunchMode;
        }

        public WaterSlide WaterSlideState = new WaterSlide();
    }
}
