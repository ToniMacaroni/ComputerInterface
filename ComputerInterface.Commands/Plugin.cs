using BepInEx;
using Bepinject;
namespace ComputerInterface.Commands
{
    [BepInDependency(PluginInfo.Id)]
    [BepInPlugin(PLUGIN_ID, PLUGIN_NAME, PluginInfo.Version)]
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
