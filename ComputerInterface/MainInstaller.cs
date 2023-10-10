using System;
using ComputerInterface.Interfaces;
using ComputerInterface.Monitors;
using ComputerInterface.Queues;
using ComputerInterface.ViewLib;
using ComputerInterface.Views;
using GorillaNetworking;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace ComputerInterface
{
    internal class MainInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container
                .BindFactory<Type, ComputerView, ComputerViewPlaceholderFactory>()
                .FromFactory<ComputerViewFactory>();
            Container.Bind<CIConfig>().AsSingle();

            Container.BindInterfacesAndSelfTo<CustomComputer>().FromNewComponentOn(ComputerGetter).AsSingle();
            Container.BindInterfacesAndSelfTo<AssetsLoader>().AsSingle();
            Container.Bind<CommandHandler>().AsSingle();

            Container.Bind<MonitorSettings>().AsSingle();

            Container.Bind<MainMenuView>().AsSingle();
            Container.Bind<WarnView>().AsSingle();

            Container.Bind<IComputerModEntry>().To<GameSettingsEntry>().AsSingle();
            Container.Bind<IComputerModEntry>().To<ComputerSettingsEntry>().AsSingle();
            Container.Bind<IComputerModEntry>().To<CommandLineEntry>().AsSingle();
            Container.Bind<IComputerModEntry>().To<DetailsEntry>().AsSingle();
            Container.Bind<IComputerModEntry>().To<ModListEntry>().AsSingle();

            Container.Bind<IQueueInfo>().To<DefaultQueue>().AsSingle();
            Container.Bind<IQueueInfo>().To<CompetitiveQueue>().AsSingle();
            Container.Bind<IQueueInfo>().To<MinigamesQueue>().AsSingle();

            Container.Bind<IMonitor>().To<ModernMonitor>().AsSingle();
            Container.Bind<IMonitor>().To<ClassicMonitor>().AsSingle();
        }

        private GameObject ComputerGetter(InjectContext ctx)
        {
            return Object.FindObjectOfType<GorillaComputer>().gameObject;
        }
    }
}