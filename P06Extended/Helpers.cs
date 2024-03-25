namespace P06X.Helpers
{
    using Rewired;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class XInput
    {
        // Thanks to Beatz for the original (InputHelper) code (^^)!
        public const string REWIRED_A = "Button A";

        public const string REWIRED_B = "Button B";

        public const string REWIRED_X = "Button X";

        public const string REWIRED_Y = "Button Y";

        public const string REWIRED_LS_X = "Left Stick X";

        public const string REWIRED_LS_Y = "Left Stick Y";

        public const string REWIRED_RS_X = "Right Stick X";

        public const string REWIRED_RS_Y = "Right Stick Y";

        public const string REWIRED_DPAD_X = "D-Pad X";

        public const string REWIRED_DPAD_Y = "D-Pad Y";

        public const string REWIRED_START = "Start";

        public const string REWIRED_BACK = "Back";

        public const string REWIRED_RIGHT_TRIGGER = "Right Trigger";

        public const string REWIRED_RIGHT_BUMPER = "Right Bumper";

        public const string REWIRED_LEFT_TRIGGER = "Left Trigger";

        public const string REWIRED_LEFT_BUMPER = "Left Bumper";

        public static Player Controls => Singleton<RInput>.Instance.Get<Player>("P");

        public static bool IsControlAxisPastThreshold(string analog, string digital, double threshold)
        {
            Player rewiredPlayer = Controls;
            if (threshold > 0.0)
            {
                return (double)rewiredPlayer.GetAxis(analog) > threshold || (double)rewiredPlayer.GetAxis(digital) > threshold;
            }

            return (double)rewiredPlayer.GetAxis(analog) < threshold || (double)rewiredPlayer.GetAxis(digital) < threshold;
        }

        public static bool IsControlXAxisPastThreshold(double threshold)
        {
            return IsControlAxisPastThreshold("Left Stick X", "D-Pad X", threshold);
        }

        public static bool IsControlYAxisPastThreshold(double threshold)
        {
            return IsControlAxisPastThreshold("Left Stick Y", "D-Pad Y", threshold);
        }

        public static bool IsControlRightStickX(double threshold)
        {
            return IsControlAxisPastThreshold("Right Stick X", string.Empty, threshold);
        }
    }

    // Test version of the bindings for simpler use of reflection
    public class ReflectionWrapper<T>
    {
        /*
         * Usage:
         * ReflectionWrapper<int> IIntWrapper = new ReflectionWrapper<int>(I);
         * ReflectionWrapper<Vector3> IVector3Wrapper = new ReflectionWrapper<Vector3>(I);
         * 
         * IIntWrapper["BoundState"] = 42;
         * IIntWrapper["PlayerState"] = (int)SonicNew.State.BoundAttack;
         * 
         * Vector3 airMotionVelocity = I._Rigidbody.velocity;
         * float lua_boundjump_jmp = ((float)ReflectionExtensions.GetLuaStruct("Sonic_New_Lua")["c_boundjump_jmp"]);
         * airMotionVelocity.y = lua_boundjump_jmp * 1.5f;
         * IVector3Wrapper["AirMotionVelocity"] = airMotionVelocity;
         * I._Rigidbody.velocity = airMotionVelocity;
         * 
         */

        /* TODO - cache also the Type - and allow for separate instance reloading */

        private object _instance;
        private Dictionary<string, FieldInfo> _fieldsCache = new Dictionary<string, FieldInfo>();

        public ReflectionWrapper(object instance)
        {
            _instance = instance;
        }

        public T this[string name]
        {
            get
            {
                var field = GetField(_instance.GetType(), name);
                if (field == null)
                    throw new Exception($"Field '{name}' not found.");
                return (T)field.GetValue(_instance);
            }
            set
            {
                var field = GetField(_instance.GetType(), name);
                if (field == null)
                    throw new Exception($"Field '{name}' not found.");
                field.SetValue(_instance, value);
            }
        }

        public object Invoke(string methodName, params object[] parameters)
        {
            var method = _instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
                throw new Exception($"Method '{methodName}' not found.");
            return method.Invoke(_instance, parameters);
        }

        private FieldInfo GetField(Type type, string name)
        {
            if (!_fieldsCache.TryGetValue(name, out FieldInfo field))
            {
                while (type != null)
                {
                    field = type.GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field != null)
                    {
                        _fieldsCache[name] = field;
                        break;
                    }
                    type = type.BaseType;
                }
            }
            return field;
        }
    }

    public class MethodCache
    {
        /*
         * Usage:
         * 
         * MethodCache methodCache = new MethodCache(I);
         * object result = methodCache["SomeMethod", arg1, arg2];
         * 
         * or
         * 
         * MethodCache methodCache = new MethodCache(I);
         * methodCache.Invoke("SomeMethod", arg1, arg2);
         * 
         */
        private object _instance;
        private static Dictionary<string, MethodInfo> _methodsCache = new Dictionary<string, MethodInfo>();

        public MethodCache(object instance)
        {
            _instance = instance;
        }

        public object this[string methodName, params object[] parameters]
        {
            get
            {
                return Invoke(methodName, parameters);
            }
        }

        public object Invoke(string methodName, params object[] parameters)
        {
            var method = GetMethod(_instance.GetType(), methodName);
            if (method == null)
                throw new Exception($"Method '{methodName}' not found.");
            return method.Invoke(_instance, parameters);
        }

        private MethodInfo GetMethod(Type type, string name)
        {
            if (!_methodsCache.TryGetValue(name, out MethodInfo method))
            {
                while (type != null)
                {
                    method = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (method != null)
                    {
                        _methodsCache[name] = method;
                        break;
                    }
                    type = type.BaseType;
                }
            }
            return method;
        }

        public void ClearCache()
        {
            _methodsCache.Clear();
        }
    }


}
