using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;
using ComputerInterface.Views;
using UnityEngine;
using Zenject;

namespace ComputerInterface
{
    internal class MainInstaller : Installer
    {
        private const string COMPUTER_PATH = "Level/Pit/UI/GorillaComputer";

        public override void InstallBindings()
        {
            Container
                .BindFactory<Type, IComputerView, ComputerViewPlaceholderFactory>()
                .FromFactory<ComputerViewFactory>();

            Container.BindInterfacesAndSelfTo<CustomComputer>().FromNewComponentOn(ComputerGetter).AsSingle();

            Container.Bind<MainMenuView>().AsSingle();
            Container.Bind<IComputerModEntry>().To<CommandLineEntry>().AsSingle();
            Container.Bind<CommandHandler>().AsSingle();
        }

        private GameObject ComputerGetter(InjectContext ctx)
        {
            return GameObject.Find(COMPUTER_PATH);
        }
    }
}