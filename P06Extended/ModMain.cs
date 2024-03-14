using MelonLoader;
using UnityEngine;

namespace P06X
{
    public class ModMain : MelonPlugin
    {
        public override void OnLateInitializeMelon()
        {
            base.OnLateInitializeMelon();
            Debug.Log("P-06 Extended (plugin) has been loaded!");
        }
    }
}
