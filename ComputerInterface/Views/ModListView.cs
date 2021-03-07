using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.Bootstrap;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;

namespace ComputerInterface.Views
{
    internal class ModListEntry : IComputerModEntry
    {
        public string EntryName => "Mods";
        public Type EntryViewType => typeof(ModListView);
    }

    internal class ModListView : ComputerView
    {
        private readonly List<BepInEx.PluginInfo> _plugins;

        private readonly UISelectionHandler _selectionHandler;

        public ModListView()
        {
            _plugins = Chainloader.PluginInfos.Values.ToList();
            _selectionHandler =
                new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter, true);
            _selectionHandler.Max = _plugins.Count - 1;
            _selectionHandler.OnSelected += SelectMod;
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
            RedrawMods(builder);

            Text = builder.ToString();
        }

        private void RedrawHeader(StringBuilder str)
        {
            str.Append("/// ").Append(_plugins.Count).Append(" Mods loaded ///").AppendLine();
        }

        private void RedrawMods(StringBuilder str)
        {
            for (int i = 0; i < _plugins.Count; i++)
            {
                var plugin = _plugins[i];
                str.Append(_selectionHandler.CurrentSelectionIndex == i ? "<color=#ed6540>></color> " : "  ")
                    .Append(plugin.Metadata.Name);
                str.Append("  ").Append(plugin.Instance.enabled ? "<color=#00ff00>E</color>" : "<color=#ff0000>D</color>").AppendLine();
            }
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