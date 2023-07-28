using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Bootstrap;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;
using ComputerInterface.Views;
using GorillaNetworking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ComputerInterface
{
    public class CustomComputer : MonoBehaviour, IInitializable
    {
        private bool _initialized;

        private GorillaComputer _gorillaComputer;
        private ComputerViewController _computerViewController;

        private readonly Dictionary<Type, IComputerView> _cachedViews = new Dictionary<Type, IComputerView>();

        private ComputerViewPlaceholderFactory _viewFactory;


        private MainMenuView _mainMenuView;

        private List<CustomScreenInfo> _customScreenInfos = new List<CustomScreenInfo>();

        private List<CustomKeyboardKey> _keys;
        private GameObject _keyboard;

        private readonly Mesh CubeMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");

        private AssetsLoader _assetsLoader;

        private CIConfig _config;

        private List<AudioSource> _keyboardAudios = new List<AudioSource>();

		enum MonitorLocation
        {
            Treehouse,
            Mountains,
            Sky,
            Basement,
            Beach
        }

        void Awake()
        {
            enabled = false;
        }

        [Inject]
        internal async void Construct(
            CIConfig config,
            AssetsLoader assetsLoader,
            MainMenuView mainMenuView,
            ComputerViewPlaceholderFactory viewFactory,
            List<IComputerModEntry> computerModEntries,
            List<IQueueInfo> queues)
        {
            if (_initialized) return;
            _initialized = true;

            Debug.Log($"Found {computerModEntries.Count} computer mod entries");

            _config = config;
            _assetsLoader = assetsLoader;

            _mainMenuView = mainMenuView;
            _cachedViews.Add(typeof(MainMenuView), _mainMenuView);

            _viewFactory = viewFactory;

            _gorillaComputer = GetComponent<GorillaComputer>();
            _gorillaComputer.enabled = false;
            GorillaComputer.instance = _gorillaComputer;

            _computerViewController = new ComputerViewController();
            _computerViewController.OnTextChanged += SetText;
            _computerViewController.OnSwitchView += SwitchView;
            _computerViewController.OnSetBackground += SetBGImage;

            // Treehouse, Mountains, Sky, Basement, Beach
            GameObject[] physicalComputers = { GameObject.Find("UI/-- PhysicalComputer UI --"), GameObject.Find("goodigloo/PhysicalComputer (2)"), GameObject.Find("skyjungle/UI/-- Clouds PhysicalComputer UI --/"), GameObject.Find("BasementComputer/PhysicalComputer (2)"), GameObject.Find("Beach/BeachComputer/PhysicalComputer (2)/") };

            for (int i = 0; i < physicalComputers.Length; i++)
            {
                // Keys should pretty much always be done seperate from the computer so you can atleast see what you're doing
                try { await ReplaceKeys(physicalComputers[i]); } // TODO: Update Clouds key texts
                catch (Exception ex) { Debug.LogError($"CI: The keyboard for the {(MonitorLocation)i} computer couldn't be replaced: {ex}"); }

                // Then load the computer screens
                try
                {
                    CustomScreenInfo screenInfo = await CreateMonitor(physicalComputers[i], (MonitorLocation)i);
                    screenInfo.Color = _config.ScreenBackgroundColor.Value;
                    screenInfo.Background = _config.BackgroundTexture;
                    _customScreenInfos.Add(screenInfo);
                }
                catch (Exception ex) { Debug.LogError($"CI: The monitor for the {(MonitorLocation)i} computer couldn't be created: {ex}"); }
            }

            try
            {
                BaseGameInterface.InitAll();
                ShowInitialView(_mainMenuView, computerModEntries);

                QueueManager.Queues = queues;
                QueueManager.Init();
            }
            catch (Exception ex) { Debug.LogError($"CI: Failed to successfully end initalizing the mod: {ex}"); }

            enabled = true;
            Debug.Log("Initialized computers");

            await Task.Delay(200); // Wait for other mods such as Utilla to load, this is here just to be safe

            // Then load the start zone (which disables the other zones), this is done since I've had issues with computers with CI
            // not being able to have their keyboards in specific not load in due to their map being disabled
            ZoneManagement.SetActiveZone(PhotonNetworkController.Instance.StartZone);
        }

        private void ShowInitialView(MainMenuView view, List<IComputerModEntry> computerModEntries)
        {
            _computerViewController.SetView(view, null);
            view.ShowEntries(computerModEntries);
        }

        public void Initialize()
        {
            foreach (var pluginInfo in Chainloader.PluginInfos.Values)
            {
                if (_config.IsModDisabled(pluginInfo.Metadata.GUID))
                {
                    pluginInfo.Instance.enabled = false;
                }
            }
        }

        private void Update()
        {
            // get key state for the key debugging feature
            if (CustomKeyboardKey.KeyDebuggerEnabled && _keys != null)
            {
                foreach (var key in _keys)
                {
                    key.Fetch();
                }
            }
        }

        public void SetText(string text)
        {
            foreach (CustomScreenInfo customScreenInfo in _customScreenInfos)
            {
                customScreenInfo.Text = text;
            }
        }

        public void SetBG(float r, float g, float b) => SetBG(new Color(r, g, b));
        public void SetBG(Color color) 
        {
            foreach (CustomScreenInfo customScreenInfo in _customScreenInfos)
            {
                customScreenInfo.Color = color;
                _config.ScreenBackgroundColor.Value = customScreenInfo.Color;
            }
        }

        public Color GetBG()
        {
            return _config.ScreenBackgroundColor.Value;
        }

        public void SetBGImage(ComputerViewChangeBackgroundEventArgs args)
        {
            foreach (CustomScreenInfo customScreenInfo in _customScreenInfos)
            {
                if (args == null || args.Texture == null)
                {
                    customScreenInfo.Background = _config.BackgroundTexture;
                    customScreenInfo.Color = _config.ScreenBackgroundColor.Value;
                    return;
                }

                customScreenInfo.Color = args.ImageColor ?? _config.ScreenBackgroundColor.Value;
                customScreenInfo.Background = args.Texture;
            }
        }

        public void PressButton(CustomKeyboardKey key)
        {
            foreach (var audio in _keyboardAudios)
            {
                if (audio.isActiveAndEnabled)
                {
                    audio.Play();
                }
            }
            _computerViewController.NotifyOfKeyPress(key.KeyboardKey);
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
            _computerViewController.SetView(destinationView, args.Args);
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

        private async Task ReplaceKeys(GameObject computer)
        {
            _keys = new List<CustomKeyboardKey>();

            var nameToEnum = new Dictionary<string, EKeyboardKey>();

            foreach (var enumString in Enum.GetNames(typeof(EKeyboardKey)))
            {
                var key = enumString.Replace("NUM", "").ToLower();
                nameToEnum.Add(key, (EKeyboardKey)Enum.Parse(typeof(EKeyboardKey), enumString));
            }

            foreach (var button in computer.GetComponentsInChildren<GorillaKeyboardButton>())
            {

                if (button.characterString == "up" || button.characterString == "down")
                {
                    button.GetComponentInChildren<MeshRenderer>().material.color = new Color(0.1f, 0.1f, 0.1f);
                    button.transform.localPosition -= new Vector3(0, 0.6f, 0);
                    DestroyImmediate(button.GetComponent<BoxCollider>());
                    if(FindText(button.gameObject, button.name + "text")?.GetComponent<Text>() is Text arrowBtnText)
                    {
                        DestroyImmediate(arrowBtnText);
                    }
                    continue;
                }

                if (!nameToEnum.TryGetValue(button.characterString.ToLower(), out var key)) continue;

                if (FindText(button.gameObject) is Text buttonText)
                {
                    var customButton = button.gameObject.AddComponent<CustomKeyboardKey>();
                    customButton.pressTime = button.pressTime;
                    customButton.functionKey = button.functionKey;

                    button.GetComponent<MeshFilter>().mesh = CubeMesh;
                    DestroyImmediate(button);

                    customButton.Init(this, key, buttonText);
                    _keys.Add(customButton);
                }
            }

            _keyboard = _keys[0].transform.parent.parent.parent.gameObject;

            var clickSound = await _assetsLoader.GetAsset<AudioClip>("ClickSound");

            var audioSource = _keyboard.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.clip = clickSound;

            _keyboardAudios.Add(audioSource);

            if (_keyboard.GetComponent<MeshRenderer>() is MeshRenderer renderer) {
                renderer.material.color = new Color(0.3f, 0.3f, 0.3f);
            }

            var enterKey = _keys.Last(x => x.KeyboardKey == EKeyboardKey.Enter);
            var mKey = _keys.Last(x => x.KeyboardKey == EKeyboardKey.M);
            var deleteKey = _keys.Last(x => x.KeyboardKey == EKeyboardKey.Delete);

            ColorUtility.TryParseHtmlString("#8787e0", out var backButtonColor);

            CreateKey(enterKey.gameObject, "Space", new Vector3(2.6f, 0, 3), EKeyboardKey.Space, "SPACE");
            CreateKey(deleteKey.gameObject, "Back", new Vector3(0, 0, -29.8f), EKeyboardKey.Back, "BACK", backButtonColor);

            ColorUtility.TryParseHtmlString("#abdbab", out var arrowKeyButtonColor);

            var leftKey = CreateKey(mKey.gameObject, "Left", new Vector3(0, 0, 5.6f), EKeyboardKey.Left, "<", arrowKeyButtonColor);
            var downKey = CreateKey(leftKey.gameObject, "Down", new Vector3(0, 0, 2.3f), EKeyboardKey.Down, ">", arrowKeyButtonColor);
            CreateKey(downKey.gameObject, "Right", new Vector3(0, 0, 2.3f), EKeyboardKey.Right, ">", arrowKeyButtonColor);
            var upKey = CreateKey(downKey.gameObject, "Up", new Vector3(-2.3f, 0, 0), EKeyboardKey.Up, ">", arrowKeyButtonColor);

            var downKeyText = FindText(downKey.gameObject).transform;
            downKeyText.localPosition += new Vector3(0, 0, 0.15f);
            downKeyText.localEulerAngles += new Vector3(0, 0, -90);

            var upKeyText = FindText(upKey.gameObject).transform;
            upKeyText.localPosition += new Vector3(0.15f, 0, 0.05f);
            upKeyText.localEulerAngles += new Vector3(0, 0, 90);

        }

        private static Text FindText(GameObject button, string name = null)
        {
            // Debug.Log($"Replacing key {button.name} / {name}");
            if (button.GetComponent<Text>() is Text text)
            {
                return text;
            }

            if (name.IsNullOrWhiteSpace())
            {
                name = button.name.Replace(" ", "");
            }

            if (name.Contains("enter"))
            {
                name = "enter";
            }

            // Forest
            Transform t = button.transform.parent?.parent?.Find("Text/" + name);
            
            // Mountain
            t ??= button.transform
                ?.parent
                ?.parent
                ?.parent
                ?.parent
                ?.parent
                ?.parent
                ?.parent
                .Find("UI/Text/" + name);
            t ??= button.transform.parent?.parent?.Find("Text/" + name + " (1)");

            // Clouds
            t ??= button.transform.parent?.parent?.parent?.parent?.Find("KeyboardUI/" + name);
            t ??= button.transform.parent?.parent?.parent?.parent?.Find("KeyboardUI/" + name + " (1)");

            // Basement
            t ??= button.transform
                ?.parent
                ?.parent
                ?.parent
                ?.parent
                ?.parent
                .Find("UI FOR BASEMENT/Text/" + name);
            t ??= button.transform.parent?.parent?.Find("Text/" + name + " (1)");

            // Beach
            t ??= button.transform.parent?.parent?.parent?.parent?.parent?.Find("UI FOR BEACH COMPUTER/Text/" + name);

            return t.GetComponent<Text>();
        }

        private CustomKeyboardKey CreateKey(GameObject prefab, string goName, Vector3 offset, EKeyboardKey key,
            string label = null, Color? color = null)
        {
            var newKey = Instantiate(prefab.gameObject, prefab.transform.parent);
            newKey.name = goName;
            newKey.transform.localPosition += offset;
            newKey.GetComponent<MeshFilter>().mesh = CubeMesh;

            Text keyText = FindText(prefab, prefab.name);
            Text newKeyText = Instantiate(keyText.gameObject, keyText.gameObject.transform.parent).GetComponent<Text>();
            newKeyText.name = goName;
            newKeyText.transform.localPosition += offset;

            var customKeyboardKey = newKey.GetComponent<CustomKeyboardKey>();
            if (label.IsNullOrWhiteSpace())
            {
                customKeyboardKey.Init(this, key);
            }
            else
            {
                if (color.HasValue)
                {
                    customKeyboardKey.Init(this, key, newKeyText, label, color.Value);
                }
                else
                {
                    customKeyboardKey.Init(this, key, newKeyText, label);
                }
            }
            _keys.Add(customKeyboardKey);

            return customKeyboardKey;
        }

        private async Task<CustomScreenInfo> CreateMonitor(GameObject computer, MonitorLocation location) // index used for removing the base game computer.
        {
            RemoveMonitor(computer, location);

            var tmpSettings = await _assetsLoader.GetAsset<TMP_Settings>("TMP Settings");
            typeof(TMP_Settings).GetField(
                    "s_Instance",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?
                .SetValue(null, tmpSettings);

            var monitorAsset = await _assetsLoader.GetAsset<GameObject>("Monitor");

            var newMonitor = Instantiate(monitorAsset);
            newMonitor.name = $"{location} Custom Monitor";
            newMonitor.transform.SetParent(computer.transform.Find("monitor") ?? computer.transform.Find("monitor (1)"), false);
            newMonitor.transform.localPosition = new Vector3(2.28f, -0.72f, 0.0f);
            newMonitor.transform.localEulerAngles = new Vector3(0.0f, 270.0f, 270.02f);
            newMonitor.transform.SetParent(computer.transform.parent, true);

            foreach (RectTransform rect in newMonitor.GetComponentsInChildren<RectTransform>()) rect.gameObject.layer = 9;
            newMonitor.GetComponentInChildren<ReflectionProbe>().enabled = false;

            var info = new CustomScreenInfo
            {
                Transform = newMonitor.transform,
                TextMeshProUgui = newMonitor.GetComponentInChildren<TextMeshProUGUI>(),
                Renderer = newMonitor.GetComponentsInChildren<MeshRenderer>().First(x => x.name == "Main Monitor"),
                RawImage = newMonitor.GetComponentInChildren<RawImage>()
            };
            info.RawImage.color = new Color(0.05f, 0.05f, 0.05f);
            info.Materials = info.Renderer.materials;
            info.Color = new Color(0.05f, 0.05f, 0.05f);

            return info;
        }

        private void RemoveMonitor(GameObject computer, MonitorLocation monitorIndex)
        {
            GameObject monitor = null;
            foreach (Transform child in computer.transform)
            {
                if (child.name.StartsWith("monitor"))
                {
                    monitor = child.gameObject;
                    monitor.SetActive(false);
                }
            }

            if (monitor is null)
            {
                Debug.Log("Unable to find monitor");
                return;
            }

            // Stable for now 
            if (computer.TryGetComponent(out GorillaComputerTerminal terminal))
            {
                terminal.monitorMesh?.gameObject?.SetActive(false);
                terminal.myFunctionText?.gameObject?.SetActive(false);
                terminal.myScreenText?.gameObject?.SetActive(false);
            }

            try
            {
                // Some monitors were baked into the scene, so we need to do all this jank to get rid of them
                // Currently, This is broken as the combined mesh has isReadable set to false
                // so all the mesh info lives on the GPU, which makes it unaccessabel afaik

                GameObject combinedScene = monitorIndex switch
                {
                    MonitorLocation.Treehouse => GameObject.Find("LocalObjects_Prefab/Forest/Terrain/Uncover ForestCombined/").GetComponentInChildren<MeshRenderer>().gameObject,
                    MonitorLocation.Mountains => GameObject.Find("Mountain/Mountain Texture Baker/Uncover Mountain Lit/").GetComponentInChildren<MeshRenderer>().gameObject,
                    MonitorLocation.Beach => GameObject.Find("Beach/Beach Texture Baker - ABOVE WATER/Uncover Beach Lit/").GetComponentInChildren<MeshRenderer>().gameObject,
                    _ => null,
                };

                if (combinedScene == null) return;

                Mesh combinedSceneMesh = combinedScene.GetComponent<MeshFilter>().mesh;
                if (!combinedSceneMesh.isReadable) return;

                var bounds = monitor.GetComponent<Renderer>().bounds;

                Vector3[] combinedSceneVertices = combinedSceneMesh.vertices;
                int[] combinedSceneTriangles = combinedSceneMesh.triangles;

                // There are duplicate vertices, so we need to make a map to not miss any
                var duplicateVerticesMap = new Dictionary<Vector3, HashSet<int>>();
                for (int i = 0; i < combinedSceneVertices.Length; i++)
                {
                    var vertex = combinedSceneVertices[i];
                    if (!duplicateVerticesMap.ContainsKey(vertex))
                    {
                        duplicateVerticesMap.Add(vertex, new HashSet<int>());
                    }
                    duplicateVerticesMap[vertex].Add(i);
                }

                var connectedVerticesMap = new Dictionary<int, HashSet<int>>();
                for (int i = 0; i < combinedSceneTriangles.Length; i += 3)
                {
                    int vertex1 = combinedSceneTriangles[i];
                    int vertex2 = combinedSceneTriangles[i + 1];
                    int vertex3 = combinedSceneTriangles[i + 2];

                    if (!connectedVerticesMap.ContainsKey(vertex1))
                    {
                        connectedVerticesMap.Add(vertex1, new HashSet<int>());
                    }

                    if (!connectedVerticesMap.ContainsKey(vertex2))
                    {
                        connectedVerticesMap.Add(vertex2, new HashSet<int>());
                    }

                    if (!connectedVerticesMap.ContainsKey(vertex3))
                    {
                        connectedVerticesMap.Add(vertex3, new HashSet<int>());
                    }

                    connectedVerticesMap[vertex1].Add(vertex2);
                    connectedVerticesMap[vertex1].Add(vertex3);

                    connectedVerticesMap[vertex2].Add(vertex1);
                    connectedVerticesMap[vertex2].Add(vertex3);

                    connectedVerticesMap[vertex3].Add(vertex1);
                    connectedVerticesMap[vertex3].Add(vertex2);
                }

                HashSet<int> monitorVertices = new HashSet<int>();

                for (int i = 0; i < combinedSceneVertices.Length; i++)
                {
                    // if the vertex is contained in bounds, use it as a root point for finding all vertices
                    if (bounds.Contains(combinedSceneVertices[i]))
                    {
                        foreach (int vertex in duplicateVerticesMap[combinedSceneVertices[i]])
                        {
                            FindConnectedVertices(vertex, monitorVertices);
                        }
                    }
                }

                void FindConnectedVertices(int index, HashSet<int> connectedVertices)
                {
                    if (!connectedVertices.Contains(index))
                    {
                        connectedVertices.Add(index);
                        foreach (var connectedVertex in connectedVerticesMap[index])
                        {
                            foreach (int duplicateVertex in duplicateVerticesMap[combinedSceneVertices[connectedVertex]])
                            {
                                FindConnectedVertices(duplicateVertex, connectedVertices);
                            }
                        }
                    }
                }

                // Remove the vertices that are not connected to the starting vertex
                foreach (int connectedVertex in monitorVertices)
                {
                    combinedSceneVertices[connectedVertex] = new Vector3(combinedSceneVertices[connectedVertex].x, -100, combinedSceneVertices[connectedVertex].z);
                }

                // Getting the vertices returned a copy of them, so set the actual vertices
                combinedSceneMesh.vertices = combinedSceneVertices;
            }
            catch { }
        }
    }
}
