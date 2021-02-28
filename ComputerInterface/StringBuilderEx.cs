using System.Text;

namespace ComputerInterface
{
    /// <summary>
    /// Bunch of extension methods for the <see cref="StringBuilder"/>
    /// </summary>
    public static class StringBuilderEx
    {
        public static StringBuilder AppendClr(this StringBuilder str, string text, string color)
        {
            return str.Append($"<color=#{color}>{text}</color>");
        }
    }
}