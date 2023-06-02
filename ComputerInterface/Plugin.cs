using BepInEx;
using Bepinject;
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

        private void Awake()
        {
            Load();
        }

        private void Load()
        {
            if (Loaded) return;

            Patches.HarmonyPatches.ApplyHarmonyPatches();

            Debug.Log("Computer Interface loading");

            Zenjector.Install<MainInstaller>().OnProject().WithConfig(Config).WithLog(Logger);

            Loaded = true;
        }
    }

}
