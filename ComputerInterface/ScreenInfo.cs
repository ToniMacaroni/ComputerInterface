using UnityEngine;

namespace ComputerInterface
{
    public class ScreenInfo
    {
        public Transform Transform;
        public Renderer Renderer;
        public Material[] Materials;

        public Color Color
        {
            get => Materials[1].color;
            set => Materials[1].color = value;
        }
    }
}