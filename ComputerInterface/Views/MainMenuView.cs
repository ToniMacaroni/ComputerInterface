using System.Collections.Generic;
using System.Text;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;

namespace ComputerInterface.Views
{
    public class MainMenuView : ComputerView
    {
        private List<IComputerModEntry> _modEntries;

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
        }

        public void ShowEntries(List<IComputerModEntry> entries)
        {
            _modEntries = entries;
            _selectionHandler.MaxIdx = _modEntries.Count - 1;

            _pageHandler.SetElements(entries.ToArray());

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