using ComputerInterface.Interfaces;

namespace ComputerInterface.Queues
{
    internal class CompetitiveQueue : IQueueInfo
    {
        public string DisplayName => "Competitive";
        public string QueueName => "COMPETITIVE";
        public string Description => "For people who want more of a challenge.";
    }
}