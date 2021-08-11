using System;
using System.Collections.Generic;
using System.Text;
using ComputerInterface.Interfaces;
using Zenject;
using UnityEngine;

namespace ComputerInterface
{
    public class QueueManager : MonoBehaviour
    {
        public static List<IQueueInfo> queues;

        public static IQueueInfo GetQueue()
        {
            return queues.Find(x => x.QueueName == PlayerPrefs.GetString("currentQueue"));
        }

        public static void SetQueue(IQueueInfo queue)
        {
            GorillaComputer.instance.currentQueue = queue.QueueName;
            PlayerPrefs.SetString("currentQueue", queue.QueueName);
            PlayerPrefs.Save();
        }
    }

    internal class DefaultQueue : IQueueInfo
    {
        public string DisplayName => "Default";
        public string QueueName => "DEFAULT";
        public string Description => "Default queue.";
    }

    internal class CompetitiveQueue : IQueueInfo
    {
        public string DisplayName => "Competitive";
        public string QueueName => "COMPETITIVE";
        public string Description => "For people who want more of a challenge.";
    }

    internal class CasualQueue : IQueueInfo
    {
        public string DisplayName => "Casual";
        public string QueueName => "CASUAL";
        public string Description => "Tagging disabled, just have fun!";
    }

    internal class ModdedQueue : IQueueInfo
    {
        public string DisplayName => "Modded";
        public string QueueName => "MODDED";
        public string Description => "A queue for modded players only.";
    }
}
