namespace P06X
{
    using HarmonyLib;
    using System;
    using UnityEngine;
    using Helpers;
    using UnityEngine.Assertions;

    public partial class XSonicNew : MonoBehaviour // should be a generic character patch subclass (for the future) - todo
    {
        // Singleton-like pattern, but that can only be created with AddComponent by the game itself.
        public static XSonicNew XInstance;
        public static SonicNew II => I.SonicNew;
        public class ISonicNew : ReflectionAccessor<SonicNew>
        {
            public SonicNew SonicNew => I;
            public ReflectionWrapper<SonicNew.State> Stt;

            public ISonicNew(SonicNew sonicNew) : base(sonicNew)
            {
                Stt = new ReflectionWrapper<SonicNew.State>(sonicNew);
            }
        }
        public static ISonicNew I;
    }
}
