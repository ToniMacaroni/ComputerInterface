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
        private MeshFilter _meshFilter = null;
        private MeshFilter MeshFilter
        {
            get
            {
                if (_meshFilter == null)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    _meshFilter = cube.GetComponent<MeshFilter>();
                    cube.SetActive(false);
                }

                return _meshFilter;
            }
        }

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

            GameObject[] physcialComputers = { GameObject.Find("UI/PhysicalComputer"), GameObject.Find("goodigloo/PhysicalComputer (2)") };
            Vector3[] positions = { new Vector3(-67.95f, 11.53f, -85.36f) , new Vector3(-28.69f, 17.57f, -96.73f) };
            Vector3[] rotations = { Vector3.up * 342, Vector3.up * 38.81f };

            for (int i = 0; i < physcialComputers.Length; i++)
            {
                try {
                    await ReplaceKeys(physcialComputers[i]);
                    CustomScreenInfo screenInfo = await CreateMonitor(physcialComputers[i], positions[i], rotations[i]);
                    screenInfo.Color = _config.ScreenBackgroundColor.Value;
                    screenInfo.Background = _config.BackgroundTexture;
                    _customScreenInfos.Add(screenInfo);
                } catch (Exception e)
                {
                    Debug.LogError($"CI: computer {i} could not be initialized: {e}");
                }
            }

            BaseGameInterface.InitAll();

            enabled = true;

            ShowInitialView(_mainMenuView, computerModEntries);

            QueueManager.Queues = queues;
            QueueManager.Init();

            Debug.Log("Initialized Computer");
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

                    button.GetComponent<MeshFilter>().mesh = MeshFilter.mesh;
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

            Transform t = button.transform.parent?.parent?.Find("Text/" + name);
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

            return t.GetComponent<Text>();
        }

        private CustomKeyboardKey CreateKey(GameObject prefab, string goName, Vector3 offset, EKeyboardKey key,
            string label = null, Color? color = null)
        {
            var newKey = Instantiate(prefab.gameObject, prefab.transform.parent);
            newKey.name = goName;
            newKey.transform.localPosition += offset;
            newKey.GetComponent<MeshFilter>().mesh = MeshFilter.mesh;

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

        private async Task<CustomScreenInfo> CreateMonitor(GameObject computer, Vector3 position, Vector3 rotation)
        {
            RemoveMonitor(computer);

            var tmpSettings = await _assetsLoader.GetAsset<TMP_Settings>("TMP Settings");
            typeof(TMP_Settings).GetField(
                    "s_Instance",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?
                .SetValue(null, tmpSettings);

            var monitorAsset = await _assetsLoader.GetAsset<GameObject>("monitor");

            var newMonitor = Instantiate(monitorAsset);
            newMonitor.name = "Custom Monitor";
            //newMonitor.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            newMonitor.transform.eulerAngles = rotation;
            newMonitor.transform.position = position;

            foreach(RectTransform rect in newMonitor.GetComponentsInChildren<RectTransform>()) rect.gameObject.layer = 9;

            var info = new CustomScreenInfo();

            info.Transform = newMonitor.transform;
            info.TextMeshProUgui = newMonitor.GetComponentInChildren<TextMeshProUGUI>();
            info.Renderer = newMonitor.GetComponentsInChildren<MeshRenderer>().First(x=>x.name=="Main Monitor");
            info.RawImage = newMonitor.GetComponentInChildren<RawImage>();
            info.Materials = info.Renderer.materials;

            info.Color = new Color(0.05f, 0.05f, 0.05f);
            //info.FontSize = 80f;

            return info;
        }

        private void RemoveMonitor(GameObject computer)
		{
            GameObject monitor = computer.transform.Find("monitor")?.gameObject;
            bool forceRemoval = monitor != null;
            monitor ??= computer.transform.Find("monitor (1)")?.gameObject;

            monitor.SetActive(false);
            // Mountain computer is strewn across the heriarchy
            if (monitor.transform.Find("FunctionSelect") is null)
            {
                computer?.transform?.parent?.parent?.parent?.Find("UI/Text/FunctionSelect").gameObject.SetActive(false);
                computer?.transform?.parent?.parent?.parent?.Find("UI/Text/Data").gameObject.SetActive(false);
                computer?.transform?.parent?.parent?.parent?.Find("UI/Text/monitor").gameObject.SetActive(false);
            }

			// Monitor was baked into the scene, so we need to do all this jank to get rid of it
            if (forceRemoval)
			{
				var combinedScene = GameObject.Find("Level/forest/Uncover ForestCombined").GetComponentInChildren<MeshRenderer>().gameObject;
				Mesh combinedSceneMesh = combinedScene.GetComponent<MeshFilter>().mesh;

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
		}
    }
}
