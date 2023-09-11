using Zenject;

namespace ComputerInterface.Commands
{
    internal class MainInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<CommandRegistrar>().AsSingle();
        }
    }
}