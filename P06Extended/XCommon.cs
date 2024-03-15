using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace P06X
{
    public class XCommon
    {
        public static bool Dbg = true;
        public static string Version = "2.0.dev";
        public static string ModFilesPath = Application.dataPath + "/../Plugins/P06Extended";

        public static void TeleportToSection(string section)
        {
            if (!String.IsNullOrEmpty(section))
            {
                section = section.Trim();
                if (SceneUtility.GetBuildIndexByScenePath(section) != -1)
                {
                    Log("Switching to: <color=#8f07f0>" + section + "</color>", 3.5f, 18f);
                    Game.ChangeArea(section, "");
                    return;
                }
            }
            Log("<color=#ee0000>Section</color> \"" + section + "\" <color=#ee0000>doesn't exist</color>!", 3f, 16f);
        }

        public static void Log(string message, float time = 3f, float size = 18f)
        {
            Debug.LogWarning($"mock log: {message}");
        }
    }
}
