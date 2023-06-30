using ComputerInterface.Interfaces;

namespace ComputerInterface.Queues
{
    internal class DefaultQueue : IQueueInfo
    {
        public string DisplayName => "Default";
        public string QueueName => "DEFAULT";
        public string Description => "Default queue.";
    }
}