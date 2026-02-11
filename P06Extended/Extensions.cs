using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace P06X.Helpers
{
    public static class ReflectionExtensions
    {
        // Thanks to Beatz for the base code (^^)!
        public static T Get<T>(this object obj, string name)
        {
            return (T)(obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(obj));
        }
        public static T Get<T>(this Type obj, string name)
        {
            return (T)(obj.GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(obj));
        }

        /// <summary>
        /// Get e.g. STHLua.Sonic_New_Lua type
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Type GetLuaStruct(string name)
        {
            Assembly ass = Assembly.GetAssembly(typeof(SonicNew));
            return ass.GetType("STHLua." + name);
        }

        /// <summary>
        /// Get e.g. STHLua.Sonic_New_Lua.c_boundattack_collision type
        /// </summary>
        /// <param name="luaStructName"></param>
        /// <param name="nestedStructName"></param>
        /// <returns></returns>
        public static Type GetLuaStruct(string luaStructName, string nestedStructName)
        {
            Assembly ass = Assembly.GetAssembly(typeof(SonicNew));
            return ass.GetType("STHLua." + luaStructName).GetNestedType(nestedStructName);
        }

        public static T Set<T>(this Type typeObj, string name, T value)
        {
            typeObj.GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(typeObj, value);
            return typeObj.Get<T>(name);
        }

        public static T Set<T>(this object obj, string name, T value)
        {
            obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(obj, value);
            return obj.Get<T>(name);
        }
        public static T GetPropValue<T>(this object src, string propName)
        {
            return (T)src.GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(src, null);
        }

        public static MethodInfo GetMethod(this object obj, string methodName)
        {
            return obj.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public static StateMachine.PlayerState GetState(this PlayerBase playerBase, string stateName)
        {
            MethodInfo mi = playerBase.GetMethod(stateName);
            Delegate state = Delegate.CreateDelegate(typeof(StateMachine.PlayerState), playerBase, mi);
            return state as StateMachine.PlayerState;
        }

        public static T InvokeFunc<T>(this object obj, string methodName, params object[] args)
        {
            MethodInfo method = obj.GetType().GetMethod(
               methodName,
               BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
               null,
               args.Select(a => a.GetType()).ToArray(),
               null);

            if (method == null)
                throw new Exception($"Method '{methodName}' not found on type '{obj.GetType().FullName}'");

            return (T)method.Invoke(obj, args);
        }

        public static T InvokeStaticMethod<T>(this Type type, string methodName, params object[] args)
        {
            var argTypes = args.Select(a => a.GetType()).ToArray();

            var method = type.GetMethod(
                methodName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                argTypes,
                null) ?? throw new Exception($"Method '{methodName}' not found on type '{type.FullName}'");

            return (T)method.Invoke(null, args); // null for static method
        }

        public static bool IsInList<T>(this T obj, params T[] values)
        {
            return values.Contains(obj);
        }

        public static async Task<T> Invoke<T>(this object obj, string methodName, TimeSpan time, params object[] args)
        {
            await Task.Delay(time);
            return obj.InvokeFunc<T>(methodName, args);
        }
    }
}
