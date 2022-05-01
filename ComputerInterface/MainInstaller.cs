using System;
using ComputerInterface.Interfaces;
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
                .BindFactory<Type, IComputerView, ComputerViewPlaceholderFactory>()
                .FromFactory<ComputerViewFactory>();

            Container.BindInterfacesAndSelfTo<CustomComputer>().FromNewComponentOn(ComputerGetter).AsSingle();

            Container.Bind<MainMenuView>().AsSingle();
            Container.Bind<IComputerModEntry>().To<GameSettingsEntry>().AsSingle();
            Container.Bind<IComputerModEntry>().To<CommandLineEntry>().AsSingle();
            Container.Bind<IComputerModEntry>().To<DetailsEntry>().AsSingle();
            Container.Bind<IComputerModEntry>().To<ModListEntry>().AsSingle();
            Container.Bind<IComputerModEntry>().To<CustomQueuesEntry>().AsSingle();
            Container.Bind<CommandHandler>().AsSingle();
            Container.BindInterfacesAndSelfTo<AssetsLoader>().AsSingle();
            Container.Bind<CIConfig>().AsSingle();
            Container.Bind<IQueueInfo>().To<DefaultQueue>().AsSingle();
            Container.Bind<IQueueInfo>().To<CompetitiveQueue>().AsSingle();
            Container.Bind<IQueueInfo>().To<MinigamesQueue>().AsSingle();
        }

        private GameObject ComputerGetter(InjectContext ctx)
        {
            return Object.FindObjectOfType<GorillaComputer>().gameObject;
        }
    }
}