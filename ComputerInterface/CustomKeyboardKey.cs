using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ComputerInterface
{
    public class CustomKeyboardKey : GorillaTriggerBox
    {
        private const int PRESS_COOLDOWN = 150;
        private const float KEY_BUMP_AMOUNT = 0.2f;
        private readonly Color _pressedColor = new Color(0.5f, 0.5f, 0.5f);

        public EKeyboardKey KeyboardKey { get; private set; }

        public float pressTime;

        public bool functionKey;

        private CustomComputer _computer;

        private bool _isOnCooldown;

        private Material _material;
        private Color _originalColor;

        private void Awake()
        {
            enabled = false;
            _material = GetComponent<MeshRenderer>().material;
            _originalColor = _material.color;
        }

        public void Init(CustomComputer computer, EKeyboardKey key)
        {
            _computer = computer;
            KeyboardKey = key;

            enabled = true;
        }

        public void Init(CustomComputer computer, EKeyboardKey key, string text)
        {
            _computer = computer;
            KeyboardKey = key;
            GetComponentInChildren<Text>().text = text;

            enabled = true;
        }

        private async void OnTriggerEnter(Collider collider)
        {
            BumpIn();
            if (_isOnCooldown) return;
            _isOnCooldown = true;

            if (collider.GetComponentInParent<GorillaTriggerColliderHandIndicator>() != null)
            {
                GorillaTriggerColliderHandIndicator component = collider.GetComponent<GorillaTriggerColliderHandIndicator>();

                _computer.PressButton(this);

                if (component != null)
                {
                    GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
                }
            }

            await Task.Delay(PRESS_COOLDOWN);
            _isOnCooldown = false;
        }

        private void OnTriggerExit(Collider collider)
        {
            BumpOut();
        }

        private void BumpIn()
        {
            var pos = transform.localPosition;
            pos.y -= KEY_BUMP_AMOUNT;
            transform.localPosition = pos;

            _material.color = _pressedColor;
        }

        private void BumpOut()
        {
            var pos = transform.localPosition;
            pos.y += KEY_BUMP_AMOUNT;
            transform.localPosition = pos;

            _material.color = _originalColor;
        }
    }
}