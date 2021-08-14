using ComputerInterface.Interfaces;

namespace ComputerInterface.Queues
{
    internal class CasualQueue : IQueueInfo
    {
        public string DisplayName => "Casual";
        public string QueueName => "CASUAL";
        public string Description => "Tagging disabled, just have fun!";
    }
}