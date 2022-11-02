using BepInEx;
using Bepinject;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace ComputerInterface
{
    [BepInDependency("dev.auros.bepinex.bepinject")]
    [BepInPlugin(PluginInfo.Id, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        /// <summary>
        /// Specifies if the plugin is loaded
        /// </summary>
        public bool Loaded { get; private set; }

        // public static Harmony _harmony;

        private void Awake()
        {
            // _harmony = new Harmony(PluginInfo.Id);
            Load();
        }

        private void Load()
        {
            if (Loaded) return;

            // _harmony.PatchAll(Assembly.GetExecutingAssembly());
            Patches.HarmonyPatches.ApplyHarmonyPatches();

            Debug.Log("Computer Interface loading");

            Zenjector.Install<MainInstaller>().OnProject().WithConfig(Config).WithLog(Logger);

            Loaded = true;
        }
    }

}
