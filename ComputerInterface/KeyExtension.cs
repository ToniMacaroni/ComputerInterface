using System.Collections.Generic;

namespace ComputerInterface
{
    public static class KeyExtension
    {
        public static bool IsFunctionKey(this EKeyboardKey key)
        {
            var idx = (uint) key;
            return idx > 35 && idx < 51;
        }

        public static bool IsNumberKey(this EKeyboardKey key)
        {
            var idx = (uint) key;
            return idx <= 9;
        }

        public static bool TryParseNumber(this EKeyboardKey key, out int num)
        {
            num = 0;
            if (!key.IsNumberKey()) return false;
            num = (int) key;
            return true;
        }

        public static bool InRange(this EKeyboardKey key, char from, char to)
        {
            var chr = key.ToString().ToLower()[0];
            return chr >= from && chr <= to;
        }
    }
}