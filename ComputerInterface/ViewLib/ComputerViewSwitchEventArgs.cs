using System;

namespace ComputerInterface.ViewLib
{
    public class ComputerViewSwitchEventArgs
    {
        public Type SourceType;
        public Type DestinationType;

        public ComputerViewSwitchEventArgs(Type sourceType, Type destinationType)
        {
            SourceType = sourceType;
            DestinationType = destinationType;
        }
    }
}