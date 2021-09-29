using ComputerInterface.Interfaces;

namespace ComputerInterface.Queues
{
    internal class UnknownQueue : IQueueInfo
    {
        public string DisplayName => queueName;
        public string QueueName => queueName;
        public string Description => "Unkown queue found in player preferences.";

        private string queueName;

        public UnknownQueue(string qName) => queueName = qName;
        private UnknownQueue() { }
    }
}