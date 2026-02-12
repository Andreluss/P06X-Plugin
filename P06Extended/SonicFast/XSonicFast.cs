namespace P06X
{
    using HarmonyLib;
    using System;
    using UnityEngine;
    using Helpers;
    using UnityEngine.Assertions;

    public partial class XSonicFast : MonoBehaviour // should be a generic character patch subclass (for the future) - todo
    {
        public class ISonicFast : ReflectionAccessor<SonicFast>
        {
            public ReflectionWrapper<SonicFast.State> Stt;

            public ISonicFast(SonicFast sonicFast) : base(sonicFast)
            {
                Stt = new ReflectionWrapper<SonicFast.State>(sonicFast);
            }
        }
        public static ISonicFast I;
        public static SonicFast II => I.I;
        public static XSonicFast XI;

    }
}
