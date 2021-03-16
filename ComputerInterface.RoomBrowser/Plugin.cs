using System;
using BepInEx;
using Bepinject;

namespace ComputerInterface.RoomBrowser
{
    [BepInDependency(PluginInfo.ID)]
    [BepInPlugin(PLUGIN_ID, PLUGIN_NAME, PluginInfo.VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_ID = "tonimacaroni.computerinterface.serverbrowser";
        public const string PLUGIN_NAME = "CI Server Browser";

        private void Awake()
        {
            Zenjector.Install<MainInstaller>().OnProject();
        }
    }
}
