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

        private Harmony _harmony;

        private void Awake()
        {
            Load();
        }

        private void Load()
        {
            if (Loaded) return;

            Debug.Log("Computer Interface loading");

            Zenjector.Install<MainInstaller>().OnProject();

            Patch();

            Loaded = true;
        }

        private void Unload()
        {
            if (!Loaded) return;

            Unpatch();

            Loaded = false;
        }

        private void Patch()
        {
            if (_harmony != null)
            {
                return;
            }

            _harmony = new Harmony(PluginInfo.ID);
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void Unpatch()
        {
            if (_harmony == null)
            {
                return;
            }

            _harmony.UnpatchAll();
            _harmony = null;
        }
    }
}
