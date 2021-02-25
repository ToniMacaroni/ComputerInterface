using UnityEngine;

namespace ComputerInterface
{
    public static class Helpers
    {
        public static Color StringToColor(string str)
        {
            var split = str.Split(' ');
            return new Color(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));
        }

        public static string ColorToString(Color color)
        {
            return $"{color.r} {color.g} {color.b}";
        }

        public static void SaveColor(string key, Color color)
        {
            PlayerPrefs.SetFloat(key+"_r", color.r);
            PlayerPrefs.SetFloat(key+"_g", color.g);
            PlayerPrefs.SetFloat(key+"_b", color.b);
            PlayerPrefs.Save();
        }

        public static Color ReadSavedColor(string key, Color defaultColor)
        {
            if (!PlayerPrefs.HasKey(key+"_r")) return defaultColor;

            var r = PlayerPrefs.GetFloat(key + "_r", 0);
            var g = PlayerPrefs.GetFloat(key + "_g", 0);
            var b = PlayerPrefs.GetFloat(key + "_b", 0);
            return new Color(r, g, b);
        }
    }
}