using UnityEngine;

namespace ComputerInterface
{
    public class CustomScreenInfo
    {
        public Transform Transform;
        public Renderer Renderer;
        public Material Material;

        public Color Color
        {
            get => Material.color;
            set => Material.color = value;
        }
    }
}