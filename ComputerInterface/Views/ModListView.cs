using System;
using System.Linq;
using System.Text;
using BepInEx.Bootstrap;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;

namespace ComputerInterface.Views
{
    internal class ModListEntry : IComputerModEntry
    {
        public string EntryName => "Mod Status";
        public Type EntryViewType => typeof(ModListView);
    }

    internal class ModListView : ComputerView
    {
        private readonly BepInEx.PluginInfo[] _plugins;

        private readonly UIElementPageHandler<BepInEx.PluginInfo> _pageHandler;
        private readonly UISelectionHandler _selectionHandler;

        public ModListView()
        {
            _plugins = Chainloader.PluginInfos.Values.ToArray();
            _selectionHandler =
                new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter);
            _selectionHandler.MaxIdx = _plugins.Length - 1;
            _selectionHandler.OnSelected += SelectMod;
            _selectionHandler.ConfigureSelectionIndicator("<color=#ed6540>> </color>", "", "  ", "");

            _pageHandler = new UIElementPageHandler<BepInEx.PluginInfo>();
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
            str.BeginColor("ffffff50");
            str.Append("/// ").Append(_plugins.Length).Append(" Mods loaded ///");
            str.EndColor().AppendLine();
        }

        private void DrawMods(StringBuilder str)
        {
            var enabledPostfix = "<color=#00ff00>  E</color>";
            var disabledPostfix = "<color=#ff0000>  D</color>";

            var lineIdx = _pageHandler.MovePageToIdx(_selectionHandler.CurrentSelectionIndex);

            _pageHandler.EnumarateElements((plugin, idx) =>
            {
                str.Append(_selectionHandler.GetIndicatedText(idx, lineIdx, plugin.Metadata.Name));
                str.Append(plugin.Instance.enabled ? enabledPostfix : disabledPostfix);
                str.AppendLine();
            });

            str.AppendLine();

            _pageHandler.AppendFooter(str);
            str.AppendLine();
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
                case EKeyboardKey.Back:
                    ReturnToMainMenu();
                    break;
            }
        }

        private void SelectMod(int idx)
        {
            ShowView<ModView>(_plugins[idx]);
        }
    }
}