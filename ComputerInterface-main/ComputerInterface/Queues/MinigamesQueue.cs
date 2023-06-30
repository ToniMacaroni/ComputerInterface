using ComputerInterface.Interfaces;

namespace ComputerInterface.Queues
{
    internal class MinigamesQueue : IQueueInfo
    {
        public string DisplayName => "Minigames";
        public string QueueName => "MINIGAMES";
        public string Description => "For people looking to play minigames.";
    }
}