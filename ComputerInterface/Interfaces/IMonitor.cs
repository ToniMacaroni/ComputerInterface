using UnityEngine;

namespace ComputerInterface.Interfaces
{
    public interface IMonitor
    {
        /// <summary>
        /// The name of the monitor in the AssetBundle
        /// </summary>
        string AssetName { get; }

        /// <summary>
        /// The width of the screen used for the monitor
        /// </summary>
        int Width { get; }

        /// <summary>
        /// The height of the screen used for the monitor
        /// </summary>
        int Height { get; }

        /// <summary>
        /// The local position used for the monitor
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// The local euler angles (aka. rotation) used for the monitor
        /// </summary>
        Vector3 EulerAngles { get; }
    }
}
