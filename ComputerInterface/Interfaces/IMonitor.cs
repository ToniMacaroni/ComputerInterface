using UnityEngine;

namespace ComputerInterface.Interfaces
{
    public interface IMonitor
    {
        string AssetName { get; }
        Vector3 Position { get; }
        Vector3 EulerAngles { get; }
        int Width { get; }
        int Height { get; }
    }
}
