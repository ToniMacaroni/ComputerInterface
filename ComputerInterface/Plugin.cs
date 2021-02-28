using System.Collections;
using System.Reflection;
using BepInEx;
using Bepinject;
using HarmonyLib;
using UnityEngine;
using Zenject;

namespace ComputerInterface
{
    [BepInPlugin(PluginInfo.ID, PluginInfo.NAME, PluginInfo.VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        /// <summary>
        /// Specifies if the plugin is loaded
        /// </summary>
        public bool Loaded { get; private set; }

        private void Awake()
        {
            Load();
        }

        private void Load()
        {
            if (Loaded) return;

            Debug.Log("Computer Interface loading");

            Zenjector.Install<MainInstaller>().OnProject().WithConfig(Config).WithLog(Logger);

            Loaded = true;
        }
    }
}
