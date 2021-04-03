using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.Bootstrap;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;

namespace ComputerInterface.Views
{
    public class MainMenuView : ComputerView
    {
        private List<IComputerModEntry> _modEntries;
        private readonly List<IComputerModEntry> _shownEntries;
        private readonly Dictionary<IComputerModEntry, BepInEx.PluginInfo> _pluginInfoMap;

        private readonly UIElementPageHandler<IComputerModEntry> _pageHandler;
        private readonly UISelectionHandler _selectionHandler;

        public MainMenuView()
        {
            _selectionHandler =
                new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter);
            _selectionHandler.OnSelected += ShowModView;
            _selectionHandler.ConfigureSelectionIndicator("<color=#ed6540>></color> ", "", "  ", "");

            _pageHandler = new UIElementPageHandler<IComputerModEntry>(EKeyboardKey.Left, EKeyboardKey.Right);
            _pageHandler.Footer = "<color=#ffffff50>{0}{1}        <align=\"right\"><margin-right=2em>page {2}/{3}</margin></align></color>";
            _pageHandler.NextMark = "▼";
            _pageHandler.PrevMark = "▲";
            _pageHandler.EntriesPerPage = 8;

            _shownEntries = new List<IComputerModEntry>();
            _pluginInfoMap = new Dictionary<IComputerModEntry, BepInEx.PluginInfo>();
        }

        public void ShowEntries(List<IComputerModEntry> entries)
        {
            _modEntries = entries;

            // Map entries to plugin infos
            _pluginInfoMap.Clear();
            foreach (var entry in entries)
            {
                var asm = entry.GetType().Assembly;
                var pluginInfo =
                    Chainloader.PluginInfos.Values.FirstOrDefault(x => x.Instance.GetType().Assembly == asm);
                if (pluginInfo != null)
                {
                    _pluginInfoMap.Add(entry, pluginInfo);
                }
            }

            FilterEntries();

            Redraw();
        }

        public void FilterEntries()
        {
            _shownEntries.Clear();
            foreach (var entry in _modEntries)
            {
                if (!_pluginInfoMap.TryGetValue(entry, out var info)) continue;
                if(info.Instance.enabled) _shownEntries.Add(entry);
            }
            _selectionHandler.MaxIdx = _shownEntries.Count - 1;
            _pageHandler.SetElements(_shownEntries.ToArray());
        }

        public void Redraw()
        {
            var builder = new StringBuilder();

            DrawHeader(builder);
            DrawMods(builder);

            SetText(builder);
        }

        public void DrawHeader(StringBuilder str)
        {
            str.BeginCenter().MakeBar('-', SCREEN_WIDTH, 0, "ffffff10");
            str.AppendClr("Computer Interface", "ed6540")
                .EndColor()
                .Append(" v")
                .Append(PluginInfo.VERSION).AppendLine();

            str.Append("by ").AppendClr("Toni Macaroni", "9be68a").AppendLine();

            str.MakeBar('-', SCREEN_WIDTH, 0, "ffffff10").EndAlign().AppendLine();
        }

        public void DrawMods(StringBuilder str)
        {
            var lineIdx = _pageHandler.MovePageToIdx(_selectionHandler.CurrentSelectionIndex);

            _pageHandler.EnumarateElements((entry, idx) =>
            {
                str.Append(_selectionHandler.GetIndicatedText(idx, lineIdx, entry.EntryName));
                str.AppendLine();
            });

            _pageHandler.AppendFooter(str);
            str.AppendLine();
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            if (_modEntries == null) return;
            FilterEntries();
            Redraw();
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
                    BaseGameInterface.Disconnect();
                    break;
            }
        }

        public void ShowModView(int idx)
        {
            ShowView(_modEntries[idx].EntryViewType);
        }
    }
}