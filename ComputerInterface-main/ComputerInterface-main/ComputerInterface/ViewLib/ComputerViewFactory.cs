using System;
using ComputerInterface.Interfaces;
using Zenject;

namespace ComputerInterface.ViewLib
{
    public class ComputerViewFactory : IFactory<Type, IComputerView>
    {
        private readonly DiContainer _container;

        public ComputerViewFactory(DiContainer container)
        {
            _container = container;
        }

        public IComputerView Create(Type viewType)
        {
            return (IComputerView) _container.Instantiate(viewType);
        }
    }
}