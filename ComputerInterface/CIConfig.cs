using BepInEx;
using BepInEx.Configuration;
using Bepinject;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ComputerInterface
{
    internal class CIConfig
    {
        public ConfigEntry<Color> ScreenBackgroundColor;
        public Texture BackgroundTexture;

        public ConfigEntry<string> _screenBackgroundPath;
        private readonly ConfigEntry<string> _disabledMods;
        private List<string> _disabledModsList;

        public CIConfig(BepInConfig config)
        {
            var file = config.Config;

            ScreenBackgroundColor = file.Bind("Colors", "ScreenBackgroundColor", new Color(0.08f, 0.08f, 0.08f), "The background color of the screen"); // increased value from 0.02 to 0.08 to make it look brighter
            _screenBackgroundPath = file.Bind("Textures", "ScreenBackgroundPath", "BepInEx/plugins/ComputerInterface/background.png", "Path to a custom screen background");
            _disabledMods = file.Bind("Mod Management", "DisabledMods", "", "List of disabled mods");

            BackgroundTexture = GetTexture(_screenBackgroundPath.Value);
            DeserializeDisabledMods();
        }

        public void AddDisabledMod(string guid)
        {
            if (!_disabledModsList.Contains(guid))
            {
                _disabledModsList.Add(guid);
            }
            SerializeDisabledMods();
        }

        public void RemoveDisabledMod(string guid)
        {
            _disabledModsList.Remove(guid);
            SerializeDisabledMods();
        }

        public bool IsModDisabled(string guid)
        {
            return _disabledModsList.Contains(guid);
        }

        private void DeserializeDisabledMods()
        {
            _disabledModsList = new List<string>();
            var modString = _disabledMods.Value;
            if (modString.StartsWith(";")) modString = modString.Substring(1);

            foreach (var guid in modString.Split(';'))
            {
                _disabledModsList.Add(guid);
            }
        }

        private void SerializeDisabledMods()
        {
            _disabledMods.Value = string.Join(";", _disabledModsList);
        }

        private Texture GetTexture(string path)
        {
            try
            {
                if (path.IsNullOrWhiteSpace()) return null;
                var file = new FileInfo(path);
                if (!file.Exists) return null;
                var tex = new Texture2D(2, 2);
                tex.LoadImage(File.ReadAllBytes(file.FullName));
                return tex;
            }
            catch (Exception)
            {
                Debug.LogError("Couldn't load CI background");
                return null;
            }
        }
    }
}