using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

namespace ComputerInterface
{
    public class CustomKeyboardKey : GorillaTriggerBox
    {
        private const int PRESS_COOLDOWN = 80;
        private const float KEY_BUMP_AMOUNT = 0.2f;

        public static bool KeyDebuggerEnabled;

        private static Dictionary<EKeyboardKey, Key> _keyMap;

        public EKeyboardKey KeyboardKey { get; private set; }
        public Text KeyboardText { get; private set; }

        public float pressTime;

        public bool functionKey;

        private CustomComputer _computer;

        private bool _isOnCooldown;

        private Material _material;
        private Color _originalColor;
        private Color _pressedColor = new Color(0.5f, 0.5f, 0.5f);
        private KeyHandler _keyHandler;

        private BoxCollider collider;
        private bool _bumped;

        private void Awake()
        {
            enabled = false;
            _material = GetComponent<MeshRenderer>().material;
            _originalColor = _material.color;
            collider = GetComponent<BoxCollider>();

            CreateKeyMap();
        }

        /// <summary>
        /// Used for debugging keyboard feature
        /// </summary>
        public void Fetch()
        {
            _keyHandler?.Fetch();
        }

        public void Init(CustomComputer computer, EKeyboardKey key, Text keyboardText = null)
        {
            _computer = computer;
            KeyboardKey = key;
            KeyboardText = keyboardText;

            if (_keyHandler != null)
            {
                _keyHandler.OnClick -= OnISKeyPress;
            }

            if (_keyMap.TryGetValue(key, out var ISKey))
            {
                _keyHandler = new KeyHandler(Keyboard.current[ISKey]);
                _keyHandler.OnClick += OnISKeyPress;
            }

            enabled = true;
        }

        public void Init(CustomComputer computer, EKeyboardKey key, Text keyboardText, string text)
        {
            Init(computer, key, keyboardText);
            if (keyboardText != null)
            {
                keyboardText.text = text;
            }
        }

        public void Init(CustomComputer computer, EKeyboardKey key, Text keyboardText, string text, Color buttonColor)
        {
            Init(computer, key, keyboardText, text);
            _material.color = buttonColor;
            _originalColor = buttonColor;

            Color.RGBToHSV(buttonColor, out float H, out float S, out float _);
            _pressedColor = Color.HSVToRGB(H, S, 0.6f);
        }

        private async void OnTriggerEnter(Collider collider)
        {
            if (_isOnCooldown) return;
            _isOnCooldown = true;

            if (collider.TryGetComponent(out GorillaTriggerColliderHandIndicator component))
            {
                BumpIn();
                _computer.PressButton(this);
                GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
                if (PhotonNetwork.InRoom && GorillaTagger.Instance.myVRRig != null)
                    PhotonView.Get(GorillaTagger.Instance.myVRRig).RPC("PlayHandTap", RpcTarget.Others, 66, component.isLeftHand, 0.1f);
            }

            await Task.Delay(PRESS_COOLDOWN);
            _isOnCooldown = false;
        }

        private void OnTriggerExit(Collider collider)
        {
            if (collider.GetComponent<GorillaTriggerColliderHandIndicator>() == null) return;
            BumpOut();
        }

        private void BumpIn()
        {
            if (!_bumped)
            {
                _bumped = true;
                var pos = transform.localPosition;
                pos.y -= KEY_BUMP_AMOUNT;
                transform.localPosition = pos;
                collider.center -= new Vector3(0, 0, KEY_BUMP_AMOUNT / 1.125f);

                _material.color = _pressedColor;
            }
        }

        private void BumpOut()
        {
            if (_bumped)
            {
                _bumped = false;
                var pos = transform.localPosition;
                pos.y += KEY_BUMP_AMOUNT;
                transform.localPosition = pos;
                collider.center += new Vector3(0, 0, KEY_BUMP_AMOUNT / 1.125f);

                _material.color = _originalColor;
            }
        }

        private void OnISKeyPress()
        {
            _computer.PressButton(this);
        }

        private void CreateKeyMap()
        {
            if (_keyMap != null) return;

            _keyMap = new Dictionary<EKeyboardKey, Key>
            {
                { EKeyboardKey.Left, Key.LeftArrow },
                { EKeyboardKey.Right, Key.RightArrow },
                { EKeyboardKey.Up, Key.UpArrow },
                { EKeyboardKey.Down, Key.DownArrow },

                { EKeyboardKey.Back, Key.Escape },
                { EKeyboardKey.Delete, Key.Backspace },

                { EKeyboardKey.Option1, Key.Numpad1 },
                { EKeyboardKey.Option2, Key.Numpad2 },
                { EKeyboardKey.Option3, Key.Numpad3 }
            };

            // add num keys
            for (int i = 1; i < 9; i++)
            {
                var localKey = (EKeyboardKey)i;
                var key = (Key)40 + i;

                _keyMap.Add(localKey, key);
            }

            _keyMap.Add(EKeyboardKey.NUM0, Key.Digit0);

            // add keys that match in name like alphabet keys
            foreach (var gtKey in Enum.GetNames(typeof(EKeyboardKey)))
            {
                var val = (EKeyboardKey)Enum.Parse(typeof(EKeyboardKey), gtKey);
                if (_keyMap.ContainsKey(val)) continue;

                if (!Enum.TryParse(gtKey, true, out Key key)) continue;

                _keyMap.Add(val, key);
            }
        }

        internal class KeyHandler
        {
            public event Action OnClick;

            private readonly KeyControl _key;
            private bool _wasPressed;

            public KeyHandler(KeyControl key)
            {
                _key = key;
            }

            public void Fetch()
            {
                if (_key.isPressed && !_wasPressed)
                {
                    _wasPressed = true;
                    OnClick?.Invoke();
                }

                if (!_key.isPressed && _wasPressed)
                {
                    _wasPressed = false;
                }
            }
        }
    }
}