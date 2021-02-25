using System.Collections.Generic;

namespace ComputerInterface
{
    public static class KeyExtension
    {
        public static readonly List<EKeyboardKey> FunctionKeys = new List<EKeyboardKey>
        {
            EKeyboardKey.RoomConfig,
            EKeyboardKey.ColorConfig,
            EKeyboardKey.TurnConfig,
            EKeyboardKey.NameConfig,
            EKeyboardKey.Enter,
            EKeyboardKey.Delete,
            EKeyboardKey.Option1,
            EKeyboardKey.Option2,
            EKeyboardKey.Option3
        };

        public static bool IsFunctionKey(this EKeyboardKey key)
        {
            return FunctionKeys.Contains(key);
        }

        public static bool IsNumberKey(this EKeyboardKey key)
        {
            return key.ToString().StartsWith("NUM");
        }

        public static bool TryParseNumber(this EKeyboardKey key, out int num)
        {
            num = 0;
            if (!key.IsNumberKey()) return false;
            num = int.Parse(key.ToString().Replace("NUM", ""));
            return true;
        }
    }
}