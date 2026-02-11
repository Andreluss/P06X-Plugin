using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P06X
{
    using HarmonyLib;
    using System;
    using UnityEngine;
    using Helpers;
    using UnityEngine.Assertions;

    public partial class XShadow : MonoBehaviour
    {
        public class IShadow : ReflectionAccessor<Shadow>
        {
            public ReflectionWrapper<Shadow.State> Stt;

            public IShadow(Shadow shadow) : base(shadow)
            {
                Stt = new ReflectionWrapper<Shadow.State>(shadow);
            }
        }
        public static IShadow I;
        public static Shadow II => I.I;
        public static XShadow XI;

    }
}
