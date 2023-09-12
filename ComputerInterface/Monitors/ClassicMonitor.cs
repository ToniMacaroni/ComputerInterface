using ComputerInterface.Interfaces;
using UnityEngine;

namespace ComputerInterface.Monitors
{
    public class ClassicMonitor : IMonitor
    {
        public string AssetName => "Classic Monitor";
        public Vector3 Position => new Vector3(-0.0787f, -0.21f, 0.5344f);
        public Vector3 EulerAngles => Vector3.right * 90f;
        public int Width => 53;
        public int Height => 12;
    }
}
