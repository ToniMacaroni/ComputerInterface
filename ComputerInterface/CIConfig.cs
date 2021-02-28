using BepInEx.Configuration;
using Bepinject;
using UnityEngine;

namespace ComputerInterface
{
    internal class CIConfig
    {
        public ConfigEntry<Color> ScreenBackgroundColor;

        public CIConfig(BepInConfig config)
        {
            var file = config.Config;

            ScreenBackgroundColor = file.Bind("Colors", "ScreenBackgroundColor", new Color(0.02f, 0.02f, 0.02f), "The background color of the screen");
        }
    }
}