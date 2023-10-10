using ComputerInterface.Interfaces;
using ComputerInterface.Monitors;
using ComputerInterface.ViewLib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace ComputerInterface
{
    public class MonitorSettings : IInitializable
    {
        private CustomComputer _customComputer;
        private CIConfig _config;

        private List<IMonitor> _monitorList;

        [Inject]
        public void Construct(CustomComputer customComputer, CIConfig config, List<IMonitor> monitorList)
        {
            _customComputer = customComputer;
            _config = config;

            _monitorList = monitorList;
        }

        public void Initialize()
        {

        }

        public IMonitor GetCurrentMonitor() => GetCurrentMonitor(_monitorList);
        public IMonitor GetCurrentMonitor(List<IMonitor> monitorList) => monitorList[Mathf.Clamp((int)_config.SavedMonitorType.Value, 0, monitorList.Count)];

        public Vector2 GetComputerDimensions() => GetComputerDimensions(GetCurrentMonitor(_monitorList));
        public Vector2 GetComputerDimensions(IMonitor _currentMonitor) => new Vector2(_currentMonitor.Width, _currentMonitor.Height);

        public async Task SetCurrentMonitor(MonitorType monitorType)
        {
            _config.SavedMonitorType.Value = monitorType;
            await _customComputer.CreateMonitors(false);

            _customComputer.GetField<ComputerViewController>("_computerViewController").SetMonitor(GetCurrentMonitor());
        }
    }
}
