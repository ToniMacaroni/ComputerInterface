using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ComputerInterface
{
    public class CustomScreenInfo
    {
        public TextMeshProUGUI TextMeshProUgui;
        public Transform Transform;
        public Renderer Renderer;
        public Material[] Materials;
        public RawImage RawImage;

        public Color Color
        {
            get => RawImage.color;
            set => RawImage.color = value;
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

        public Texture Background
        {
            get => RawImage.texture;
            set => RawImage.texture = value;
        }
    }
}