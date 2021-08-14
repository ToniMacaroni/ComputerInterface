using ComputerInterface.Interfaces;

namespace ComputerInterface.Queues
{
    internal class ModdedQueue : IQueueInfo
    {
        public string DisplayName => "Modded";
        public string QueueName => "MODDED";
        public string Description => "A queue for modded players only.";
    }
}