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

        private readonly UIPageHandler _pageHandler;
        private readonly UISelectionHandler _selectionHandler;

        public ModListView()
        {
            _plugins = Chainloader.PluginInfos.Values.ToList();
            _selectionHandler =
                new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter, true);
            _selectionHandler.Max = _plugins.Count - 1;
            _selectionHandler.OnSelected += SelectMod;
            _selectionHandler.ConfigureSelectionIndicator("<color=#ed6540>></color> ", "", "   ", "");

            _pageHandler = new UIPageHandler();
            _pageHandler.EntriesPerPage = 8;

            var lines = new string[_plugins.Count];

            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = _plugins[i].Metadata.Name;
                if (_plugins[i].Instance.enabled)
                {
                    lines[i] += "<color=#00ff00>  E</color>";
                }
                else
                {
                    lines[i] += "<color=#ff0000>  D</color>";
                }
            }
            _pageHandler.SetLines(lines);
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
            var lineIdx = _pageHandler.MovePageToLine(_selectionHandler.CurrentSelectionIndex);
            var lines = _pageHandler.GetLinesForCurrentPage();
            for (var i = 0; i < lines.Length; i++)
            {
                str.Append(_selectionHandler.GetIndicatedText(i, lineIdx, lines[i]));
                str.AppendLine();
            }

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