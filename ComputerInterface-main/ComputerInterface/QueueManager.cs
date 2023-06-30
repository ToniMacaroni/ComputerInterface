using System;
using System.Collections.Generic;
using System.Text;
using ComputerInterface.Interfaces;
using ComputerInterface.Queues;
using Zenject;
using UnityEngine;
using GorillaNetworking;

namespace ComputerInterface
{
    public class QueueManager : MonoBehaviour
    {
        public static List<IQueueInfo> Queues;

        public static void Init()
        {
            SetQueue(GetQueue());
        }

        public static IQueueInfo GetQueue()
        {
            if (Queues == null || Queues.Count < 1)
            {
                return new DefaultQueue();
            }

            var queueString = GetQueueString();
            return Queues.Find(x => x.QueueName == queueString);
        }

        static string GetQueueString()
        {
            string currentQueue = PlayerPrefs.GetString("currentQueue", "DEFAULT");

            return currentQueue;
        }

        public static void SetQueue(IQueueInfo queue)
        {
            GorillaComputer.instance.currentQueue = queue.QueueName;
            PlayerPrefs.SetString("currentQueue", queue.QueueName);
            PlayerPrefs.Save();
        }
    }
}
