using HarmonyLib;
using System.IO;
using UnityEngine;

namespace P06X
{
    internal class XSplashScreens
    {
        public static float startTime;
        public static bool skipMenu = true;

        [HarmonyPatch(typeof(SplashScreens), "Start")]
        public class SplashScreens_Start
        {
            public static void Postfix(SplashScreens __instance)
            {
                startTime = Time.time;
            }
        }
        [HarmonyPatch(typeof(SplashScreens), "Update")]
        public class SplashScreens_Update
        {
            public static void Postfix(SplashScreens __instance)
            {
                if (!XCommon.Dbg) return;

                if (Input.GetKey(KeyCode.T)) skipMenu = false;

                if (skipMenu && Time.time - startTime >= 0.1f)
                {
                    string path = XCommon.ModFilesPath + "/next_area.txt";
                    try
                    {
                        string[] array = System.IO.File.ReadAllLines(path);
                        XCommon.TeleportToSection(array[0]);
                    }
                    catch (FileNotFoundException)
                    {
                        Debug.LogError($"File not found: next_area.txt ({path})");
                    }
                }
            }
        }
    }
}

