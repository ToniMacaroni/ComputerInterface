using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerInterface.Interfaces
{
    public interface IQueueInfo
    {
        /// <summary>
        /// Name shown in Queue Config.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Brief description of the queue.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// The actual queue string. Usually in all capitals.
        /// </summary>
        string QueueName { get; }
    }
}
