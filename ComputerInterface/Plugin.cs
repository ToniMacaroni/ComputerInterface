using System.Reflection;
using BepInEx;
using Bepinject;
using HarmonyLib;
using UnityEngine;

namespace ComputerInterface
{
    [BepInDependency("dev.auros.bepinex.bepinject")]
    [BepInPlugin(PluginInfo.ID, PluginInfo.NAME, PluginInfo.VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        /// <summary>
        /// Specifies if the plugin is loaded
        /// </summary>
        public bool Loaded { get; private set; }

        public static Harmony _harmony;

        private void Awake()
        {
            _harmony = new Harmony(PluginInfo.ID);
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
