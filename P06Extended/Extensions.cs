﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace P06X.Helpers
{
    public static class ReflectionExtensions
    {
        // Thanks to Beatz for the base code (^^)!
        public static T GetFieldValue<T>(this object obj, string name)
        {
            return (T)(obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(obj));
        }
        public static T GetFieldValue<T>(this Type obj, string name)
        {
            return (T)(obj.GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(obj));
        }

        public static Type GetLuaStruct(string name)
        {
            Assembly ass = Assembly.GetAssembly(typeof(SonicNew));
            return ass.GetType("STHLua." + name);
        }

        public static T SetFieldValue<T>(this object obj, string name, T value)
        {
            obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(obj, value);
            return obj.GetFieldValue<T>(name);
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

        public static T Invoke<T>(this object obj, string methodName, params object[] args)
        {
            MethodInfo method = obj.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method != null)
            {
                return (T)method.Invoke(obj, args);
            }

            return default(T);
        }

        public static bool IsInList<T>(this T obj, params T[] values)
        {
            return values.Contains(obj);
        }

        // write a variadic function for checking if an object is in a list of objects
        public static bool IsIn<T>(this T obj, params T[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i].Equals(obj))
                {
                    return true;
                }
            }
            return false;
        }

        public static async Task<T> Invoke<T>(this object obj, string methodName, TimeSpan time, params object[] args)
        {
            await Task.Delay(time);
            return obj.Invoke<T>(methodName, args);
        }

        public static bool IsNullOrDefault(object obj)
        {
            return obj == null || obj == null;
        }
    }
}
