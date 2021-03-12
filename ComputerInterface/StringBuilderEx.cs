using System.Text;
using UnityEngine.Rendering;

namespace ComputerInterface
{
    /// <summary>
    /// Bunch of extension methods for the <see cref="StringBuilder"/>
    /// </summary>
    public static class StringBuilderEx
    {
        public static StringBuilder AppendClr(this StringBuilder str, string text, string color)
        {
            return str.BeginColor(color).Append(text).EndColor();
        }

        public static StringBuilder BeginColor(this StringBuilder str, string color)
        {
            return str.Append($"<color=#{color}>");
        }

        public static StringBuilder EndColor(this StringBuilder str)
        {
            return str.Append("</color>");
        }

        public static StringBuilder BeginAlign(this StringBuilder str, string align)
        {
            return str.Append($"<align=\"{align}\">");
        }

        public static StringBuilder EndAlign(this StringBuilder str)
        {
            return str.Append("</align>");
        }

        public static StringBuilder BeginCenter(this StringBuilder str)
        {
            return str.BeginAlign("center");
        }

        public static StringBuilder Repeat(this StringBuilder str, string toRepeat, int repeatNum)
        {
            for (int i = 0; i < repeatNum; i++)
            {
                str.Append(toRepeat);
            }

            return str;
        }
    }
}