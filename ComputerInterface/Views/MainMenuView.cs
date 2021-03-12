using System.Collections.Generic;
using System.Text;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;

namespace ComputerInterface.Views
{
    public class MainMenuView : ComputerView
    {
        private List<IComputerModEntry> _modEntries;

        private readonly UISelectionHandler _selectionHandler;

        public MainMenuView()
        {
            _selectionHandler =
                new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter, true);
            _selectionHandler.OnSelected += ShowModView;
        }

        public void ShowMods(List<IComputerModEntry> entries)
        {
            _modEntries = entries;
            _selectionHandler.Max = _modEntries.Count - 1;

            Redraw();
        }

        public void Redraw()
        {
            var builder = new StringBuilder();


            //var sb = new StringBuilder()
            //    .AppendLine("<noparse> << BACK            ENTER - LOAD MAP</noparse>")
            //    .AppendLine()
            //    .AppendLine("MAP DETAILS")
            //    .AppendLine()
            //    .Append("NAME:  <color=#00cc44>").Append("gkz_beginnerblock").AppendLine("</color>")
            //    .Append("AUTHOR:  <color=#00cc44>").Append("Graic").AppendLine("</color>")
            //    .Append("DESCRIPTION:  <color=#00cc44>").Append("A large tower.").AppendLine("</color>");
            //Text = sb.ToString();
            //return;

            DrawHeader(builder);
            DrawMods(builder);

            Text = builder.ToString();
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
            for (var i = 0; i < _modEntries.Count; i++)
            {
                var entry = _modEntries[i];
                str.Append(_selectionHandler.CurrentSelectionIndex == i ? "<color=#ed6540>></color> " : "  ");
                str.Append(entry.EntryName).Append("\n");
            }
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