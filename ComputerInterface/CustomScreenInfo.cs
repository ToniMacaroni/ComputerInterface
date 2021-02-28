using TMPro;
using UnityEngine;

namespace ComputerInterface
{
    public class CustomScreenInfo
    {
        public TextMeshProUGUI TextMeshProUgui;
        public Transform Transform;
        public Renderer Renderer;
        public Material[] Materials;

        public Color Color
        {
            get => Materials[1].color;
            set => Materials[1].color = value;
        }

        public string Text
        {
            get => TextMeshProUgui.text;
            set => TextMeshProUgui.text = value;
        }

        public float FontSize
        {
            get => TextMeshProUgui.fontSize;
            set => TextMeshProUgui.fontSize = value;
        }
    }
}