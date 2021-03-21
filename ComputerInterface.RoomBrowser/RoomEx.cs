using System.Collections.Generic;
using System.Linq;
using BepInEx;
using ExitGames.Client.Photon;
using Photon.Realtime;

namespace ComputerInterface.RoomBrowser
{
    internal static class RoomEx
    {
        public static bool HasProp(this RoomInfo room, string key)
        {
            return room.CustomProperties.ContainsKey(key);
        }

        public static string GetString(this RoomInfo room, RoomProps.EPropType propType)
        {
            var key = propType.ToString();
            if (!room.HasProp(key)) return null;
            return (string) room.CustomProperties[key];
        }

        public static bool IsString(this RoomInfo room, string key, string value)
        {
            if (room.CustomProperties.TryGetValue(key, out var val) &&
                val is string typeString &&
                typeString == value)
            {
                return true;
            }

            return false;
        }

        public static void SetString(this RoomInfo room, RoomProps.EPropType propType, string value)
        {
            room.CustomProperties.Add(propType.ToString(), value);
        }

        public static void SetMods(this RoomInfo room, IEnumerable<string> mods)
        {
            string modString = string.Join(";", mods);
            room.SetString(RoomProps.EPropType.Mods, modString);
        }

        public static string[] GetMods(this RoomInfo room)
        {
            var modString = room.GetString(RoomProps.EPropType.Mods);
            if (modString.IsNullOrWhiteSpace()) return null;
            return modString.Split(';');
        }

        public static string GetDescription(this RoomInfo room)
        {
            return room.GetString(RoomProps.EPropType.Description);
        }

        
    }

    internal class RoomProps
    {
        private readonly Hashtable _hashtable;

        public RoomProps()
        {
            _hashtable = new Hashtable();
        }

        public void SetString(EPropType propType, string value)
        {
            _hashtable.Add(propType.ToString(), value);
        }

        public void SetMods(IEnumerable<string> mods)
        {
            string modString = string.Join(";", mods);
            SetString(EPropType.Mods, modString);
        }

        public void SetDescription(string desc)
        {
            SetString(EPropType.Description, desc);
        }

        public void SetGameMode(string gameMode, string queue = "DEFAULT")
        {
            _hashtable.Add("gameMode", gameMode+queue);
        }

        public Hashtable GetHashTable()
        {
            return _hashtable;
        }

        public string[] GetKeys()
        {
            return _hashtable.Keys.Cast<string>().ToArray();
        }

        public enum EPropType
        {
            Mods,
            Description
        }
    }
}