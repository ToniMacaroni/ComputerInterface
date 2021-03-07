using BepInEx;
using Bepinject;

namespace ComputerInterface.Commands
{
    [BepInDependency(PluginInfo.ID)]
    [BepInPlugin(PLUGIN_ID, PLUGIN_NAME, PluginInfo.VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_ID = "tonimacaroni.computerinterface.commands";
        public const string PLUGIN_NAME = "Computer Interface Commands";

        void Awake()
        {
            Zenjector.Install<MainInstaller>().OnProject();
        }
    }
}
