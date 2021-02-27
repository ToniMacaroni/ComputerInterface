using System;
using System.Collections.Generic;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;
using ComputerInterface.Views;
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
            Container.Bind<IComputerModEntry>().To<CommandLineEntry>().AsSingle();
            Container.Bind<IComputerModEntry>().To<ModListEntry>().AsSingle();
            Container.Bind<CommandHandler>().AsSingle();
        }

        private GameObject ComputerGetter(InjectContext ctx)
        {
            return Object.FindObjectOfType<GorillaComputer>().gameObject;
        }
    }
}