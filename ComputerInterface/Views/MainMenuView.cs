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
                new UISelectionHandler(EKeyboardKey.Option1, EKeyboardKey.Option2, EKeyboardKey.Enter, true);
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

            DrawHeader(builder);
            DrawMods(builder);

            Text = builder.ToString();
        }

        public void DrawHeader(StringBuilder str)
        {
            str.Append("========================================").AppendLine();
            str.Append("    Computer Interface v").Append(PluginInfo.VERSION).AppendLine();
            str.Append("       by Toni Macaroni").AppendLine();
            str.Append("========================================").AppendLine();
        }

        public void DrawMods(StringBuilder str)
        {
            for (var i = 0; i < _modEntries.Count; i++)
            {
                var entry = _modEntries[i];
                str.Append(_selectionHandler.CurrentSelectionIndex == i ? "> " : "  ");
                str.Append(entry.EntryName).Append("\n");
            }
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            _selectionHandler.HandleKeypress(key);
            Redraw();
        }

        public void ShowModView(int idx)
        {
            ShowView(_modEntries[idx].EntryViewType);
        }
    }
}