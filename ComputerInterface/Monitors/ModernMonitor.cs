using ComputerInterface.Interfaces;
using UnityEngine;

namespace ComputerInterface.Monitors
{
    public class ModernMonitor : IMonitor
    {
        public string AssetName => "Modern Monitor";
        public Vector3 Position => new Vector3(2.1861f, -0.5944f, -0.0001f);
        public Vector3 EulerAngles => new Vector3(0f, 270f, 270.02f);
        public int Width => 41;
        public int Height => 12;
    }
}
