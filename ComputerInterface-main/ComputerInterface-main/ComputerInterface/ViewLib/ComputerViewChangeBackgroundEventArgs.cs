using UnityEngine;

namespace ComputerInterface.ViewLib
{
    public class ComputerViewChangeBackgroundEventArgs
    {
        public Texture Texture;
        public Color? ImageColor;

        public ComputerViewChangeBackgroundEventArgs(Texture texture, Color? imageColor = null)
        {
            Texture = texture;
            ImageColor = imageColor;
        }
    }
}