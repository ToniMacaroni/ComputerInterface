using System;

namespace ComputerInterface.ViewLib
{
    public class ComputerViewSwitchEventArgs
    {
        public Type SourceType;
        public Type DestinationType;
        public object[] Args;

        public ComputerViewSwitchEventArgs(Type sourceType, Type destinationType, object[] args)
        {
            SourceType = sourceType;
            DestinationType = destinationType;
            Args = args;
        }
    }
}