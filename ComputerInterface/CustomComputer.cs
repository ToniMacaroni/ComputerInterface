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

            // Treehouse, Mountains, Sky, Beach                                                 // that igloo is NOT good!!
            GameObject[] physcialComputers = { GameObject.Find("UI/-- PhysicalComputer UI --"), GameObject.Find("goodigloo/PhysicalComputer (2)"), GameObject.Find("skyjungle/UI/-- Clouds PhysicalComputer UI --/"), GameObject.Find("BasementComputer/PhysicalComputer (2)"), GameObject.Find("beach/BeachComputer/PhysicalComputer (2)/") };

            for (int i = 0; i < physcialComputers.Length; i++)
            {
                try
                {
                    await ReplaceKeys(physcialComputers[i], false); // TODO: Update Clouds key texts
                    CustomScreenInfo screenInfo = await CreateMonitor(physcialComputers[i], i);
                    screenInfo.Color = _config.ScreenBackgroundColor.Value;
                    screenInfo.Background = _config.BackgroundTexture;
                    _customScreenInfos.Add(screenInfo);
                }
                catch (Exception e)
                {
                    Debug.LogError($"CI: computer {i} could not be initialized: {e}");
                }
            }

            BaseGameInterface.InitAll();

            enabled = true;

            ShowInitialView(_mainMenuView, computerModEntries);

            QueueManager.Queues = queues;
            QueueManager.Init();

            Debug.Log("Initialized computers");
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

        public void SetBG(float r, float g, float b)
        {
            foreach (CustomScreenInfo customScreenInfo in _customScreenInfos)
            {
                customScreenInfo.Color = new Color(r, g, b);
                _config.ScreenBackgroundColor.Value = customScreenInfo.Color;
            }
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

        public void PressButton(EKeyboardKey key) => _computerViewController.NotifyOfKeyPress(key);

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

        private async Task ReplaceKeys(GameObject computer, bool cloudsComputer)
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

            CreateKey(enterKey.gameObject, "Space", new Vector3(2.6f, 0, 3), EKeyboardKey.Space, "SPACE", cloudsOffsetMethod: cloudsComputer);
            CreateKey(deleteKey.gameObject, "Back", new Vector3(0, 0, -29.8f), EKeyboardKey.Back, "BACK", backButtonColor, cloudsOffsetMethod: cloudsComputer);

            ColorUtility.TryParseHtmlString("#abdbab", out var arrowKeyButtonColor);

            var leftKey = CreateKey(mKey.gameObject, "Left", new Vector3(0, 0, 5.6f), EKeyboardKey.Left, "<", arrowKeyButtonColor, cloudsOffsetMethod: cloudsComputer);
            var downKey = CreateKey(leftKey.gameObject, "Down", new Vector3(0, 0, 2.3f), EKeyboardKey.Down, ">", arrowKeyButtonColor, cloudsOffsetMethod: cloudsComputer);
            var rightKey = CreateKey(downKey.gameObject, "Right", new Vector3(0, 0, 2.3f), EKeyboardKey.Right, ">", arrowKeyButtonColor, cloudsOffsetMethod: cloudsComputer);
            var upKey = CreateKey(downKey.gameObject, "Up", new Vector3(-2.3f, 0, 0), EKeyboardKey.Up, ">", arrowKeyButtonColor, cloudsOffsetMethod: cloudsComputer);

            var downKeyText = FindText(downKey.gameObject).transform;
            downKeyText.localPosition += !cloudsComputer ? new Vector3(0, 0, 0.15f) : new Vector3(0.0022f, 0.001f, -0.002f); // Offset for Clouds isn't 100% correct, but it's extremely close
            downKeyText.localEulerAngles += new Vector3(0, 0, -90);

            var upKeyText = FindText(upKey.gameObject).transform;
            upKeyText.localPosition += !cloudsComputer ? new Vector3(0.15f, 0, 0.05f) : new Vector3(-0.0022f, 0, -0.0032f); // Offset for Clouds isn't 100% correct, but it's extremely close
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
            // if (t != null) {
            // 	Debug.Log($"Found key using Forest {t.gameObject.name}");
            // 	return t.GetComponent<Text>();
            // }

            // Sky
            /*
            t ??= button.transform
                ?.parent
                ?.parent
                ?.parent
                ?.parent
                ?.parent
                .Find("Text/" + name);
            t ??= button.transform
                ?.parent
                ?.parent
                ?.parent
                ?.parent
                ?.parent
                .Find("Text/" + name + " (1)");
            */
            
            // Clouds
            t ??= button.transform.parent?.parent?.parent?.parent?.Find("KeyboardUI/" + name);
            t ??= button.transform.parent?.parent?.parent?.parent?.Find("KeyboardUI/" + name + " (1)");
            //if (t != null) {
            // 	Debug.Log($"Found key using Sky {t.gameObject.name}");
            // 	return t.GetComponent<Text>();
            // }

            // Mountain
            if (t is null)
            {
                // bruh
                t = button.transform
                    ?.parent
                    ?.parent
                    ?.parent
                    ?.parent
                    ?.parent
                    ?.parent
                    ?.parent
                    .Find("UI/Text/" + name);
            }
            t ??= button.transform.parent?.parent?.Find("Text/" + name + " (1)");
            // if (t != null) {
            // 	Debug.Log($"Found key using Mountain {t.gameObject.name}");
            // 	return t.GetComponent<Text>();
            // }
            // if (t is null) {
            // 	Debug.Log($"Unable to find transform");
            // }

            // Basement
            if (t is null)
            {
                // bruh
                t = button.transform
                    ?.parent
                    ?.parent
                    ?.parent
                    ?.parent
                    ?.parent
                    .Find("UI FOR BASEMENT/Text/" + name);
            }
            t ??= button.transform.parent?.parent?.Find("Text/" + name + " (1)");
            // if (t != null) {
            // 	Debug.Log($"Found key using Mountain {t.gameObject.name}");
            // 	return t.GetComponent<Text>();
            // }
            // if (t is null) {
            // 	Debug.Log($"Unable to find transform");
            // }

            // Beach
            t ??= button.transform.parent?.parent?.parent?.parent?.parent?.Find("UI FOR BEACH COMPUTER/Text/" + name);

            return t.GetComponent<Text>();
        }

        private CustomKeyboardKey CreateKey(GameObject prefab, string goName, Vector3 offset, EKeyboardKey key,
            string label = null, Color? color = null, bool cloudsOffsetMethod = false)
        {
            var newKey = Instantiate(prefab.gameObject, prefab.transform.parent);
            newKey.name = goName;
            newKey.transform.localPosition += offset;
            newKey.GetComponent<MeshFilter>().mesh = CubeMesh;

            Text keyText = FindText(prefab, prefab.name);
            Text newKeyText = Instantiate(keyText.gameObject, keyText.gameObject.transform.parent).GetComponent<Text>();
            newKeyText.name = goName;
            if (cloudsOffsetMethod) newKeyText.transform.position = newKey.transform.position;
            else newKeyText.transform.localPosition += offset;
            newKeyText.enabled = true;
            newKeyText.transform.position += cloudsOffsetMethod ? newKeyText.transform.forward * -(newKey.transform.localScale.z / 96.7741935484f) : Vector3.zero;
            newKeyText.transform.position += cloudsOffsetMethod ? newKeyText.transform.up * 0.00166f : Vector3.zero;
            newKeyText.transform.position += cloudsOffsetMethod ? newKeyText.transform.right * -0.002255f : Vector3.zero;

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

        private async Task<CustomScreenInfo> CreateMonitor(GameObject computer, int computerIndex) // index used for removing the base game computer.
        {
            RemoveMonitor(computer, computerIndex);

            var tmpSettings = await _assetsLoader.GetAsset<TMP_Settings>("TMP Settings");
            typeof(TMP_Settings).GetField(
                    "s_Instance",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?
                .SetValue(null, tmpSettings);

            var monitorAsset = await _assetsLoader.GetAsset<GameObject>("Monitor");

            var newMonitor = Instantiate(monitorAsset);
            newMonitor.name = "Custom Monitor";
            newMonitor.transform.parent = computer.transform.Find("monitor") ?? computer.transform.Find("monitor (1)");
            newMonitor.transform.localPosition = new Vector3(0.0213f, -0.31f, 0.5344f);
            newMonitor.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
            newMonitor.transform.parent = null;

            foreach (RectTransform rect in newMonitor.GetComponentsInChildren<RectTransform>()) rect.gameObject.layer = 9;

            var info = new CustomScreenInfo();

            info.Transform = newMonitor.transform;
            info.TextMeshProUgui = newMonitor.GetComponentInChildren<TextMeshProUGUI>();
            info.Renderer = newMonitor.GetComponentsInChildren<MeshRenderer>().First();
            info.Materials = info.Renderer.materials;
            info.FontSize = 60f;

            // realtime
            var collider = info.Renderer.gameObject.AddComponent<BoxCollider>();
            collider.center = Vector3.zero;
            collider.size = Vector3.one * 0.02f;

            var image = new GameObject("RawImage", typeof(RectTransform), typeof(RawImage));
            image.transform.SetParent(info.TextMeshProUgui.transform.parent, false);
            image.transform.SetPositionAndRotation(info.TextMeshProUgui.transform.position, info.TextMeshProUgui.transform.rotation);
            image.transform.localScale = info.TextMeshProUgui.transform.localScale;
            image.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1090);
            image.transform.SetAsFirstSibling();

            info.RawImage = image.GetComponent<RawImage>();
            return info;
        }

        private void RemoveMonitor(GameObject computer, int monitorIndex)
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
                // Treehouse monitor was baked into the scene, so we need to do all this jank to get rid of it
                // Currently, This is broken as the combined mesh has isReadable set to false
                // so all the mesh info lives on the GPU, which makes it unaccessabel afaik

                GameObject combinedScene = null;
                switch (monitorIndex)
                {
                    case 0:
                        combinedScene = GameObject.Find("forest/ForestObjects/Uncover ForestCombined/").GetComponentInChildren<MeshRenderer>().gameObject;
                        break;
                    case 1:
                        combinedScene = GameObject.Find("mountain/Mountain Texture Baker/Uncover Mountain Lit/CombinedMesh-Uncover Mountain Lit-mesh/").GetComponentInChildren<MeshRenderer>().gameObject;
                        break;
                    case 5:
                        combinedScene = GameObject.Find("beach/Beach Texture Baker - ABOVE WATER/Uncover Beach Lit/").GetComponentInChildren<MeshRenderer>().gameObject;
                        break;
                    default:
                        break;
                }

                if (combinedScene == null) return;

                Mesh combinedSceneMesh = combinedScene.GetComponent<MeshFilter>().mesh;
                if (!combinedSceneMesh.isReadable) return;

                var bounds = monitor.GetComponent<Renderer>().bounds;

                Vector3[] combinedSceneVertices = combinedSceneMesh.vertices;
                int[] combinedSceneTriangles = combinedSceneMesh.triangles;

                // There are duplicate verticies, so we need to make a map to not miss any
                var duplicateVerticiesMap = new Dictionary<Vector3, HashSet<int>>();
                for (int i = 0; i < combinedSceneVertices.Length; i++)
                {
                    var vertex = combinedSceneVertices[i];
                    if (!duplicateVerticiesMap.ContainsKey(vertex))
                    {
                        duplicateVerticiesMap.Add(vertex, new HashSet<int>());
                    }
                    duplicateVerticiesMap[vertex].Add(i);
                }

                var connectedVerticiesMap = new Dictionary<int, HashSet<int>>();
                for (int i = 0; i < combinedSceneTriangles.Length; i += 3)
                {
                    int vertex1 = combinedSceneTriangles[i];
                    int vertex2 = combinedSceneTriangles[i + 1];
                    int vertex3 = combinedSceneTriangles[i + 2];

                    if (!connectedVerticiesMap.ContainsKey(vertex1))
                    {
                        connectedVerticiesMap.Add(vertex1, new HashSet<int>());
                    }

                    if (!connectedVerticiesMap.ContainsKey(vertex2))
                    {
                        connectedVerticiesMap.Add(vertex2, new HashSet<int>());
                    }

                    if (!connectedVerticiesMap.ContainsKey(vertex3))
                    {
                        connectedVerticiesMap.Add(vertex3, new HashSet<int>());
                    }

                    connectedVerticiesMap[vertex1].Add(vertex2);
                    connectedVerticiesMap[vertex1].Add(vertex3);

                    connectedVerticiesMap[vertex2].Add(vertex1);
                    connectedVerticiesMap[vertex2].Add(vertex3);

                    connectedVerticiesMap[vertex3].Add(vertex1);
                    connectedVerticiesMap[vertex3].Add(vertex2);
                }

                HashSet<int> monitorVerticies = new HashSet<int>();

                for (int i = 0; i < combinedSceneVertices.Length; i++)
                {
                    // if the vertex is contined in bounds, use it as a root point for finding all verticies
                    if (bounds.Contains(combinedSceneVertices[i]))
                    {
                        foreach (int vertex in duplicateVerticiesMap[combinedSceneVertices[i]])
                        {
                            FindConnectedVerticies(vertex, monitorVerticies);
                        }
                    }
                }

                void FindConnectedVerticies(int index, HashSet<int> connectedVerticies)
                {
                    if (!connectedVerticies.Contains(index))
                    {
                        connectedVerticies.Add(index);
                        foreach (var connectedVertex in connectedVerticiesMap[index])
                        {
                            foreach (int duplicateVertex in duplicateVerticiesMap[combinedSceneVertices[connectedVertex]])
                            {
                                FindConnectedVerticies(duplicateVertex, connectedVerticies);
                            }
                        }
                    }
                }

                // Remove the verticies that are not connected to the starting vertex
                foreach (int connectedVertex in monitorVerticies)
                {
                    combinedSceneVertices[connectedVertex] = new Vector3(combinedSceneVertices[connectedVertex].x, -100, combinedSceneVertices[connectedVertex].z);
                }

                // Getting the verticies returend a copy of them, so set the actual verticies
                combinedSceneMesh.vertices = combinedSceneVertices;
            }
            catch { }
        }
    }
}
