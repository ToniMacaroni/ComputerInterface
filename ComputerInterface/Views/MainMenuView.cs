using System.Collections.Generic;
using System.Text;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;

namespace ComputerInterface.Views
{
    public class MainMenuView : ComputerView
    {
        private List<IComputerModEntry> _modEntries;

        private readonly UIPageHandler _pageHandler;
        private readonly UISelectionHandler _selectionHandler;

        public MainMenuView()
        {
            _selectionHandler =
                new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter, true);
            _selectionHandler.OnSelected += ShowModView;
            _selectionHandler.ConfigureSelectionIndicator("<color=#ed6540>></color> ", "", "   ", "");

            _pageHandler = new UIPageHandler(EKeyboardKey.Left, EKeyboardKey.Right);
            _pageHandler.EntriesPerPage = 4;
        }

        public void ShowMods(List<IComputerModEntry> entries)
        {
            _modEntries = entries;
            _selectionHandler.Max = _modEntries.Count - 1;

            var lines = new string[entries.Count];
            for (int i = 0; i < entries.Count; i++)
            {
                lines[i] = entries[i].EntryName;
            }
            _pageHandler.SetLines(lines);

            Redraw();
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
            str.Repeat("=", SCREEN_WIDTH).AppendLine();
            str.BeginCenter().AppendClr("Computer Interface", "ed6540").EndColor().Append(" v")
                .Append(PluginInfo.VERSION).AppendLine();
            str.Append("by ").AppendClr("Toni Macaroni", "9be68a").EndAlign().AppendLine();
            str.Repeat("=", SCREEN_WIDTH).AppendLine();
        }

        public void DrawMods(StringBuilder str)
        {
            var lineIdx = _pageHandler.MovePageToLine(_selectionHandler.CurrentSelectionIndex);
            var lines = _pageHandler.GetLinesForCurrentPage();
            for (var i = 0; i < lines.Length; i++)
            {
                str.Append(_selectionHandler.GetIndicatedText(i, lineIdx, lines[i]));
                str.AppendLine();
            }

            str.AppendLine();
            _pageHandler.AppendFooter(str);
            str.AppendLine();
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
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