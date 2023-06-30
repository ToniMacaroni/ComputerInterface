﻿using System;

namespace ComputerInterface.ViewLib
{
    public class UITextInputHandler
    {
        public string Text;

        public bool IsValid
        {
            get
            {
                if (Validator == null) return false;
                return Validator.Invoke(Text);
            }
        }

        public Func<string, bool> Validator;

        public bool HandleKey(EKeyboardKey key)
        {
            if (key == EKeyboardKey.Delete)
            {
                DeleteChar();
                return true;
            }

            if (key == EKeyboardKey.Space)
            {
                AddSpace();
                return true;
            }

            if (key.IsFunctionKey()) return false;

            TypeChar(key);
            return true;
        }

        private void TypeChar(EKeyboardKey key)
        {
            if (key.TryParseNumber(out var num))
            {
                Text += num;
                return;
            }

            Text += key;
        }

        public void AddSpace()
        {
            Text += " ";
        }

        public void DeleteChar()
        {
            if (Text.Length == 0) return;

            Text = Text.Substring(0, Text.Length - 1);
        }
    }
}