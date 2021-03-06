using System.Text;
using UnityEngine;

namespace ComputerInterface.ViewLib
{
    public class CharacterMatrix
    {
        public char[,] CharMatrix;

        public CharacterMatrix()
        {
            CharMatrix = new char[ComputerView.SCREEN_HEIGHT, ComputerView.SCREEN_WIDTH];
        }

        public void InsertText(string text, int x, int y)
        {
            if (text.Length + x >= ComputerView.SCREEN_WIDTH)
            {
                Debug.LogError($"[{nameof(CharacterMatrix)}] Text is out of screen bounds");
            }

            for (int i = x; i < text.Length; i++)
            {
                InsertChar(text[i], x, y);
            }
        }

        public void InsertChar(char c, int x, int y)
        {
            CharMatrix[y, x] = c;
        }

        public void Clear()
        {
            for (int y = 0; y < CharMatrix.GetLength(0); y++)
            {
                for (int x = 0; x < CharMatrix.GetLength(1); x++)
                {
                    CharMatrix[y, x] = ' ';
                }
            }
        }

        public override string ToString()
        {
            var xLength = CharMatrix.GetLength(1);
            var yLength = CharMatrix.GetLength(0);

            var str = new StringBuilder();

            for (int y = 0; y < yLength; y++)
            {
                for (int x = 0; x < xLength; x++)
                {
                    str.Append(CharMatrix[y, x]);
                }

                if (y != yLength - 1)
                {
                    str.AppendLine();
                }
            }

            return str.ToString();
        }
    }
}