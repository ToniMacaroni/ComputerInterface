using System.Collections.Generic;
using BepInEx.Configuration;
using Bepinject;
using UnityEngine;

namespace ComputerInterface
{
    internal class CIConfig
    {
        public ConfigEntry<Color> ScreenBackgroundColor;
        public ConfigEntry<string> DisabledMods;

        private List<string> _disabledMods;

        public CIConfig(BepInConfig config)
        {
            var file = config.Config;

            ScreenBackgroundColor = file.Bind("Colors", "ScreenBackgroundColor", new Color(0.02f, 0.02f, 0.02f), "The background color of the screen");
            DisabledMods = file.Bind("Mod Management", "DisabledMods", "", "List of disabled mods");
            DeserializeDisabledMods();
        }

        public void AddDisabledMod(string guid)
        {
            if (!_disabledMods.Contains(guid))
            {
                _disabledMods.Add(guid);
            }
            SerializeDisabledMods();
        }

        public void RemoveDisabledMod(string guid)
        {
            _disabledMods.Remove(guid);
            SerializeDisabledMods();
        }

        public bool IsModDisabled(string guid)
        {
            return _disabledMods.Contains(guid);
        }

        private void DeserializeDisabledMods()
        {
            _disabledMods = new List<string>();
            var modString = DisabledMods.Value;
            if (modString.StartsWith(";")) modString = modString.Substring(1);

            foreach (var guid in modString.Split(';'))
            {
                _disabledMods.Add(guid);
            }
        }

        private void SerializeDisabledMods()
        {
            DisabledMods.Value = string.Join(";", _disabledMods);
        }
    }
}