using System;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Bootstrap;
using Bepinject;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;
using HarmonyLib;

namespace ComputerInterface.Views
{
    internal class ModListEntry : IComputerModEntry
    {
        public string EntryName => "Mod Status";
        public Type EntryViewType => typeof(ModListView);
    }

    internal class ModListView : ComputerView
    {
		internal class ModListItem
		{
            private readonly CIConfig _config;

			public BepInEx.PluginInfo PluginInfo { get; private set; }
			public bool Supported { get; private set; }

            public ModListItem(BepInEx.PluginInfo pluginInfo, CIConfig config)
			{
                _config = config;
                PluginInfo = pluginInfo;
                Supported = DoesModImplementFeature();
			}

			private bool DoesModImplementFeature()
			{
                var onEnable = AccessTools.Method(PluginInfo.Instance.GetType(), "OnEnable");
                var onDisable = AccessTools.Method(PluginInfo.Instance.GetType(), "OnDisable");
                return onEnable != null && onDisable != null;
			}

            public void EnableMod()
			{
                PluginInfo.Instance.enabled = true;
                _config.RemoveDisabledMod(PluginInfo.Metadata.GUID);
			}

            public void DisableMod()
			{
                PluginInfo.Instance.enabled = false;
                _config.AddDisabledMod(PluginInfo.Metadata.GUID);
			}

            public void ToggleMod()
			{
                if (PluginInfo.Instance.enabled)
				{
                    DisableMod();
				} else
				{
                    EnableMod();
				}
			}
		}

        private readonly CIConfig _config;

        private readonly ModListItem[] _plugins;

        private readonly UIElementPageHandler<ModListItem> _pageHandler;
        private readonly UISelectionHandler _selectionHandler;

        public ModListView(CIConfig config)
        {
            _config = config;

            var pluginInfos = Chainloader.PluginInfos.Values.Where(plugin => !plugin.Metadata.GUID.Contains(PluginInfo.Id));
            _plugins = pluginInfos.Select(plugin => new ModListItem(plugin, _config)).OrderBy(x => !x.Supported).ToArray();
            _selectionHandler =
                new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter);
            _selectionHandler.MaxIdx = _plugins.Length - 1;
            _selectionHandler.OnSelected += SelectMod;
            _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}>> </color>", "", "  ", "");

            _pageHandler = new UIElementPageHandler<ModListItem>();
            _pageHandler.EntriesPerPage = 10;

            _pageHandler.SetElements(_plugins);
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            Redraw();
        }

        private void Redraw()
        {
            var builder = new StringBuilder();

            RedrawHeader(builder);
            DrawMods(builder);

            Text = builder.ToString();
        }

        private void RedrawHeader(StringBuilder str)
        {
            str.BeginColor("ffffff50").Append("== ").EndColor();
            str.Append($"Mod Status").BeginColor("ffffff50").Append(" ==").EndColor().AppendLine();

            string labelContents = $"{_plugins.Length} mod{(_plugins.Length == 1 ? "" : "s")} loaded, {_plugins.Count(a => a.Supported)} toggleable mod{(_plugins.Count(a => a.Supported) == 1 ? "" : "s")} loaded";
            str.Append($"<size=40><margin=0.55em>{labelContents}</margin></size>").Append("\n<size=24> </size>");
        }

        private void DrawMods(StringBuilder str)
        {
            // var enabledPostfix = "<color=#00ff00>  E</color>";
            // var disabledPostfix = "<color=#ff0000>  D</color>";
            var enabledPrefix = "<color=#00ff00> + </color>";
            var disabledPrefix = "<color=#ff0000> - </color>";

            var unsupportedColor = "ffffff50";

            var lineIdx = _pageHandler.MovePageToIdx(_selectionHandler.CurrentSelectionIndex);

            _pageHandler.EnumarateElements((plugin, idx) =>
            {
                str.AppendLine();
                str.Append(plugin.PluginInfo.Instance.enabled ? enabledPrefix : disabledPrefix);
                if (!plugin.Supported) str.BeginColor(unsupportedColor);
                str.Append(_selectionHandler.GetIndicatedText(idx, lineIdx, plugin.PluginInfo.Metadata.Name));
                if (!plugin.Supported) str.EndColor();
                // str.Append(plugin.Instance.enabled ? enabledPostfix : disabledPostfix);
            });

            str.AppendLine();
            _pageHandler.AppendFooter(str);
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_selectionHandler.HandleKeypress(key))
            {
                Redraw();
                return;
            }

            switch (key)
            {
                case EKeyboardKey.Option1:
                    ShowView<ModView>(_plugins[_selectionHandler.CurrentSelectionIndex]);
                    break;
                case EKeyboardKey.Back:
                    ReturnToMainMenu();
                    break;
            }
        }

        private void SelectMod(int idx)
        {
            if (_plugins[idx].Supported)
			{
                _plugins[idx].ToggleMod();
			}
            Redraw();
        }
    }
}