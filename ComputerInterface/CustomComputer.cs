using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;
using ComputerInterface.Views;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace ComputerInterface
{
    public class CustomComputer : MonoBehaviour, IInitializable
    {
        private GorillaComputer _gorillaComputer;
        private ComputerViewController _computerViewController;

        private readonly Dictionary<Type, IComputerView> _cachedViews = new Dictionary<Type, IComputerView>();

        private ComputerViewPlaceholderFactory _viewFactory;

        private bool _initialized;

        private MainMenuView _mainMenuView;

        private ScreenInfo _screenInfo;

        void Awake()
        {
            enabled = false;
        }

        [Inject]
        public void Construct(MainMenuView mainMenuView, ComputerViewPlaceholderFactory viewFactory, List<IComputerModEntry> computerModEntries)
        {
            if (_initialized) return;

            Debug.Log($"Found {computerModEntries.Count} computer mod entries");

            _mainMenuView = mainMenuView;
            _cachedViews.Add(typeof(MainMenuView), _mainMenuView);

            _viewFactory = viewFactory;

            _gorillaComputer = GetComponent<GorillaComputer>();
            _gorillaComputer.enabled = false;
            GorillaComputer.instance = _gorillaComputer;

            _computerViewController = new ComputerViewController();
            _computerViewController.OnTextChanged += SetText;
            _computerViewController.OnSwitchView += SwitchView;

            ReplaceKeys();
            _screenInfo = GetScreenInfo();
            _screenInfo.Color = Helpers.ReadSavedColor("ScreenBackground", new Color(0.02f, 0.02f, 0.02f));
            BaseGameInterface.InitAll();

            enabled = true;
            _initialized = true;

            ShowInitialView(_mainMenuView, computerModEntries);

            Debug.Log("Initialized Computer");
        }

        private void ShowInitialView(MainMenuView view, List<IComputerModEntry> computerModEntries)
        {
            _computerViewController.SetView(view);
            view.ShowMods(computerModEntries);
        }

        public void Initialize()
        {
        }

        public ScreenInfo GetScreenInfo()
        {
            var info = new ScreenInfo();
            info.Transform = transform.Find("monitor");
            info.Renderer = info.Transform.GetComponent<MeshRenderer>();

            var materials = info.Renderer.materials;
            var newMaterial = new Material(materials[1].shader);
            materials[1] = newMaterial;
            info.Renderer.materials = materials;
            info.Materials = materials;

            return info;
        }

        public void Reposition()
        {
            var monitor = transform.Find("monitor");
            monitor.gameObject.SetActive(false);
        }

        public void SetText(string text)
        {
            _gorillaComputer.screenText.text = text;
        }

        public void SetBG(float r, float g, float b)
        {
            _screenInfo.Color = new Color(r, g, b);
            Helpers.SaveColor("ScreenBackground", _screenInfo.Color);
        }

        public void PressButton(CustomKeyboardKey key)
        {
            _computerViewController.NotifyOfKeyPress(key.KeyboardKey);
        }

        private void CreateMonitor()
        {
            var newMonitor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newMonitor.name = "Custom Monitor";
            newMonitor.GetComponent<MeshRenderer>().material.color = new Color(0.05f, 0.05f, 0.05f);
            newMonitor.transform.localScale = new Vector3(1, 0.6f, 0.02f);
            newMonitor.transform.eulerAngles = new Vector3(0, 90, 0);
            newMonitor.transform.position = new Vector3(-68.85f, 12.00f, -83.00f);
        }

        private void SwitchView(ComputerViewSwitchEventArgs args)
        {
            if (args.SourceType == args.DestinationType) return;

            var destinationView = GetOrCreateView(args.DestinationType);

            if (destinationView == null)
            {
                return;
            }

            destinationView.CallerViewType = args.SourceType;
            _computerViewController.SetView(destinationView);
        }

        private IComputerView GetOrCreateView(Type type)
        {
            if (_cachedViews.TryGetValue(type, out var view))
            {
                return view;
            }

            var newView = _viewFactory.Create(type);
            _cachedViews.Add(type, newView);
            return newView;
        }

        private void ReplaceKeys()
        {
            var nameToEnum = new Dictionary<string, EKeyboardKey>();

            foreach (var enumString in Enum.GetNames(typeof(EKeyboardKey)))
            {
                var key = enumString.Replace("NUM", "").ToLower();
                nameToEnum.Add(key, (EKeyboardKey)Enum.Parse(typeof(EKeyboardKey), enumString));
            }

            foreach(var button in GetComponentsInChildren<GorillaKeyboardButton>())
            {
                if (!nameToEnum.TryGetValue(button.characterString.ToLower(), out var key)) continue;

                var customButton = button.gameObject.AddComponent<CustomKeyboardKey>();
                customButton.pressTime = button.pressTime;
                customButton.functionKey = button.functionKey;
                customButton.sliderValues = button.sliderValues;

                DestroyImmediate(button);

                customButton.Init(this, key);
            }
        }
    }
}