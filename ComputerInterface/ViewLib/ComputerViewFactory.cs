using System;
using ComputerInterface.Interfaces;
using Zenject;

namespace ComputerInterface.ViewLib
{
    public class ComputerViewFactory : IFactory<Type, ComputerView>
    {
        private readonly DiContainer _container;

        public ComputerViewFactory(DiContainer container)
        {
            _container = container;
        }

        public ComputerView Create(Type viewType)
        {
            return (ComputerView) _container.Instantiate(viewType);
        }
    }
}