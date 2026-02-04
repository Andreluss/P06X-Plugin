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
        public class ISonicNew
        {
            public class Tps<T>
            {
                /* this class is Beta version - not recommended to use 
                   problem: not tested invalidated _wrap references */ 
                private static ReflectionWrapper<T> _wrap;
                public static ReflectionWrapper<T> Wrap
                {
                    get
                    {
                        if (_wrap == null)
                        {
                            if (XSonicNew.I == null)
                            {
                                Debug.LogError("Trying to wrap when Instance is null!");
                                _wrap = null;
                                return null;
                            }
                            _wrap = new ReflectionWrapper<T>(XSonicNew.I.SonicNew);
                        }
                        return _wrap;
                    }
                }
            }
            public ReflectionWrapper<T> W<T>() => Tps<T>.Wrap;
            // ---------------------
            public SonicNew SonicNew;
            public ReflectionWrapper<int> Int;
            public ReflectionWrapper<float> Flt;
            public ReflectionWrapper<Boolean> Boo;
            public ReflectionWrapper<SonicNew.State> Stt;
            public ReflectionWrapper<Vector3> Vec;
            public ReflectionWrapper<Quaternion> Qua;
            public ReflectionWrapper<PlayerCamera> PCa;
            public ISonicNew(SonicNew sonicNew)
            {
                SonicNew = sonicNew;
                Int = new ReflectionWrapper<int>(sonicNew);
                Flt = new ReflectionWrapper<float>(sonicNew);
                Boo = new ReflectionWrapper<bool>(sonicNew);
                Stt = new ReflectionWrapper<SonicNew.State>(sonicNew);
                Vec = new ReflectionWrapper<Vector3>(sonicNew);
                Qua = new ReflectionWrapper<Quaternion>(sonicNew);
                PCa = new ReflectionWrapper<PlayerCamera>(sonicNew);
            }
        }
        public static ISonicNew I;
    }
}
