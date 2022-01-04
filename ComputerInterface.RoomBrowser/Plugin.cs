using BepInEx;
using Bepinject;

namespace ComputerInterface.RoomBrowser
{
    [BepInDependency(PluginInfo.Id)]
    [BepInPlugin(PLUGIN_ID, PLUGIN_NAME, PluginInfo.Version)]
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
