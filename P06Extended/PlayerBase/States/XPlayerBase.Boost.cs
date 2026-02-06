namespace P06X
{
    using HarmonyLib;
    using UnityEngine;
    using Helpers;
    using UnityEngine.Assertions;
    using System;

    public partial class XPlayerBase : MonoBehaviour
    {
        public class Boost
        {
            public bool IsBoosting;
        }

        public Boost BoostState = new Boost();
    }
}
