using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Bootstrap;
using ComputerInterface.Interfaces;
using ComputerInterface.Monitors;
using ComputerInterface.ViewLib;
using ComputerInterface.Views;
using GorillaNetworking;
using HarmonyLib;
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
        private WarnView _warningView;

        private readonly List<CustomScreenInfo> _customScreenInfos = new List<CustomScreenInfo>();
        private readonly Dictionary<MonitorLocation, CustomScreenInfo> _customScreenDict = new Dictionary<MonitorLocation, CustomScreenInfo>();

        private List<CustomKeyboardKey> _keys;
        private GameObject _keyboard;

        private readonly Mesh CubeMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");

        private AssetsLoader _assetsLoader;

        private CIConfig _config;

        private readonly List<AudioSource> _keyboardAudios = new List<AudioSource>();

        public IMonitor Monitor
        {
            get => _monitor;
            set
            {
                _monitor = value;
                MonitorType = _monitorDict.FirstOrDefault(a => a.Value == _monitor).Key;
                MonitorScale = Tuple.Create(_monitor.Width, _monitor.Height);
            }
        }
        public static Tuple<int, int> MonitorScale = Tuple.Create(0, 0);

        private IMonitor _monitor;
        private List<IMonitor> _monitors = new List<IMonitor>();

        public MonitorType MonitorType;
        public Dictionary<MonitorType, IMonitor> _monitorDict = new Dictionary<MonitorType, IMonitor>();

        private bool _internetConnected => Application.internetReachability != NetworkReachability.NotReachable;
        private bool _connectionError;

        enum MonitorLocation
        {
            Stump,
            Igloo,
            Clouds,
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
            WarnView warningView,
            ComputerViewPlaceholderFactory viewFactory,
            List<IComputerModEntry> computerModEntries,
            List<IQueueInfo> queues,
            List<IMonitor> monitors)
        {
            if (_initialized) return;
            _initialized = true;

            Debug.Log($"Found {computerModEntries.Count} computer mod entries");

            _config = config;
            _assetsLoader = assetsLoader;

            _mainMenuView = mainMenuView;
            _warningView = warningView;
            _cachedViews.Add(typeof(MainMenuView), _mainMenuView);

            _viewFactory = viewFactory;

            _gorillaComputer = GetComponent<GorillaComputer>();
            _gorillaComputer.enabled = false;
            GorillaComputer.instance = _gorillaComputer;

            _monitors = monitors;
            _monitors.ForEach(a => _monitorDict.Add((MonitorType)_monitors.IndexOf(a), a));
            Monitor = _monitors[Mathf.Clamp((int)config.SavedMonitorType.Value, 0, _monitors.Count)];

            _computerViewController = new ComputerViewController();
            _computerViewController.OnTextChanged += SetText;
            _computerViewController.OnSwitchView += SwitchView;
            _computerViewController.OnSetBackground += SetBGImage;

            await CreateMonitors(); // Wait for the mod to finish creating the modified computers
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
        }

        private void ShowInitialView(MainMenuView view, List<IComputerModEntry> computerModEntries)
        {
            foreach (var pluginInfo in Chainloader.PluginInfos.Values)
            {
                if (!_config.IsModDisabled(pluginInfo.Metadata.GUID)) continue;
                pluginInfo.Instance.enabled = false;
            }

            if (PhotonNetworkController.Instance.wrongVersion)
            {
                _computerViewController.SetView(_warningView, new object[] { new WarnView.OutdatedWarning() });
                return;
            }
            _computerViewController.SetView(view, null);
            view.ShowEntries(computerModEntries);
        }

        public void Initialize() { }

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

            // Make sure the computer is ready
            if (_computerViewController.CurrentComputerView != null)
            {
                // Check to see if our connection is off
                if (!_internetConnected && !_connectionError)
                {
                    _connectionError = true;
                    _computerViewController.SetView(_warningView, new object[] { new WarnView.NoInternetWarning() });
                    _gorillaComputer.UpdateFailureText("NO WIFI OR LAN CONNECTION DETECTED.");
                }
               
                // Check to see if we're back online
                if (_internetConnected && _connectionError)
                {
                    _connectionError = false;
                    _computerViewController.SetView(_computerViewController.CurrentComputerView == _warningView ? _mainMenuView : _computerViewController.CurrentComputerView, null);
                    _gorillaComputer.InvokeMethod("RestoreFromFailureState", null);
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

        public async Task SetMonitorType(MonitorType monitorType)
        {
            Monitor = _monitors[Mathf.Clamp((int)monitorType, 0, _monitors.Count)];
            _config.SavedMonitorType.Value = monitorType;
            await CreateMonitors(false);

            // Re-open the current view as some of the width and height dependent stuff might look a bit off
            _computerViewController.CurrentComputerView.OnShow(new object[] { });
        }

        public async Task CreateMonitors(bool includeKeys = true)
        {
            // https://github.com/legoandmars/Utilla/blob/457bc612eda8e63b989dcdb219e04e8e7f06393a/Utilla/GamemodeManager.cs#L54
            ZoneManagement zoneManager = FindObjectOfType<ZoneManagement>();

            // https://github.com/legoandmars/Utilla/blob/457bc612eda8e63b989dcdb219e04e8e7f06393a/Utilla/GamemodeManager.cs#L56
            ZoneData FindZoneData(GTZone zone)
                => (ZoneData)AccessTools.Method(typeof(ZoneManagement), "GetZoneData").Invoke(zoneManager, new object[] { zone });

            Transform[] physicalComputers =
            {
                FindZoneData(GTZone.forest).rootGameObjects[1].transform.Find("TreeRoomInteractables/UI/-- PhysicalComputer UI --"),
                FindZoneData(GTZone.mountain).rootGameObjects[0].transform.Find("Geometry/goodigloo/PhysicalComputer (2)"),
                FindZoneData(GTZone.skyJungle).rootGameObjects[0].transform.Find("UI/-- Clouds PhysicalComputer UI --"),
                FindZoneData(GTZone.basement).rootGameObjects[0].transform.Find("DungeonRoomAnchor/BasementComputer/PhysicalComputer (2)"),
                FindZoneData(GTZone.beach).rootGameObjects[0].transform.Find("BeachComputer/PhysicalComputer (2)")
            };

            for (int i = 0; i < physicalComputers.Length; i++)
            {
                if (includeKeys)
                {
                    // Keys should pretty much always be done seperate from the computer so you can atleast see what you're doing
                    try { await ReplaceKeys(physicalComputers[i].gameObject); } // TODO: Update Clouds key texts
                    catch (Exception ex) { Debug.LogError($"CI: The keyboard for the {(MonitorLocation)i} computer couldn't be replaced: {ex}"); }
                }

                // Then load the computer screens
                try
                {
                    CustomScreenInfo screenInfo = await CreateMonitor(physicalComputers[i].gameObject, (MonitorLocation)i);
                    screenInfo.Color = _config.ScreenBackgroundColor.Value;
                    screenInfo.Background = _config.BackgroundTexture;
                    if (_customScreenDict.TryGetValue((MonitorLocation)i, out var _tempMonitor))
                    {
                        _customScreenInfos.Remove(_tempMonitor);
                        _customScreenDict.Remove((MonitorLocation)i);
                    }
                    _customScreenInfos.Add(screenInfo);
                    _customScreenDict.Add((MonitorLocation)i, screenInfo);
                }
                catch (Exception ex) { Debug.LogError($"CI: The monitor for the {(MonitorLocation)i} computer couldn't be created: {ex}"); }
            }
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

            foreach (var button in computer.GetComponentsInChildren<GorillaKeyboardButton>(true))
            {

                if (button.characterString == "up" || button.characterString == "down")
                {
                    button.GetComponentInChildren<MeshRenderer>(true).material.color = new Color(0.1f, 0.1f, 0.1f);
                    button.GetComponentInChildren<MeshFilter>().mesh = CubeMesh;
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
                //renderer.material.color = new Color(0.3f, 0.3f, 0.3f);
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
            else if (color.HasValue)
            {
                customKeyboardKey.Init(this, key, newKeyText, label, color.Value);
            }
            else
            {
                customKeyboardKey.Init(this, key, newKeyText, label);
            }

            _keys.Add(customKeyboardKey);
            return customKeyboardKey;
        }

        private async Task<CustomScreenInfo> CreateMonitor(GameObject computer, MonitorLocation location) // index used for removing the base game computer.
        {
            bool monitorExists = _customScreenDict.ContainsKey(location);
            if (!monitorExists) RemoveMonitor(computer, location);

            var monitorAsset = await _assetsLoader.GetAsset<GameObject>(Monitor.AssetName);
            var newMonitor = Instantiate(monitorAsset);
            newMonitor.name = $"{location} Custom Monitor";
            newMonitor.transform.SetParent(computer.transform.Find("monitor") ?? computer.transform.Find("monitor (1)"), false);
            newMonitor.transform.localPosition = Monitor.Position;
            newMonitor.transform.localEulerAngles = Monitor.EulerAngles;
            newMonitor.transform.SetParent(computer.transform.parent, true);

            var info = new CustomScreenInfo
            {
                Transform = newMonitor.transform,
                TextMeshProUgui = newMonitor.GetComponentInChildren<TextMeshProUGUI>(),
                Renderer = newMonitor.GetComponentsInChildren<MeshRenderer>().First(x => x.name == "Main Monitor"),
                RawImage = newMonitor.GetComponentInChildren<RawImage>()
            };
            info.Renderer.gameObject.AddComponent<GorillaSurfaceOverride>();
            info.Materials = info.Renderer.materials;
            info.Color = new Color(0.05f, 0.05f, 0.05f);

            if (monitorExists)
            {
                // Remove the existing monitor
                var oldInfo = _customScreenDict[location];
                Destroy(oldInfo.Transform.gameObject);

                // Sync the text
                info.TextMeshProUgui.text = _computerViewController.CurrentComputerView.Text;
            }

            return info;
        }

        private void RemoveMonitor(GameObject computer, MonitorLocation computerLocation)
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

            if (computerLocation == MonitorLocation.Stump)
            {
                var monitorTransform = computer.transform.parent.parent?.Find("Static/monitor") ?? null;
                monitorTransform?.gameObject?.SetActive(false);
            }

            try
            {
                // Some monitors were baked into the scene, so we need to do all this jank to get rid of them
                // Currently, This is broken as the combined mesh has isReadable set to false
                // so all the mesh info lives on the GPU, which makes it unaccessabel afaik

                // https://github.com/legoandmars/Utilla/blob/457bc612eda8e63b989dcdb219e04e8e7f06393a/Utilla/GamemodeManager.cs#L54
                ZoneManagement zoneManager = FindObjectOfType<ZoneManagement>();

                // https://github.com/legoandmars/Utilla/blob/457bc612eda8e63b989dcdb219e04e8e7f06393a/Utilla/GamemodeManager.cs#L56
                ZoneData FindZoneData(GTZone zone)
                    => (ZoneData)AccessTools.Method(typeof(ZoneManagement), "GetZoneData").Invoke(zoneManager, new object[] { zone });

                GameObject combinedScene = computerLocation switch
                {
                    MonitorLocation.Stump => FindZoneData(GTZone.forest).rootGameObjects[1].transform.Find("Terrain/Uncover ForestCombined/").GetComponentInChildren<MeshRenderer>(true).gameObject,
                    MonitorLocation.Igloo => FindZoneData(GTZone.mountain).rootGameObjects[0].transform.Find("Mountain Texture Baker/Uncover Mountain Lit/").GetComponentInChildren<MeshRenderer>(true).gameObject,
                    MonitorLocation.Beach => FindZoneData(GTZone.beach).rootGameObjects[0].transform.Find("Beach Texture Baker - ABOVE WATER/Uncover Beach Lit/").GetComponentInChildren<MeshRenderer>(true).gameObject,
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
