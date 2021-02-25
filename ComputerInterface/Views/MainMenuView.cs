using System.Collections.Generic;
using System.Text;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;

namespace ComputerInterface.Views
{
    public class MainMenuView : ComputerView
    {
        private List<IComputerModEntry> _modEntries;

        private int _currentCursorPosition = 0;

        public void ShowMods(List<IComputerModEntry> entries)
        {
            _modEntries = entries;

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
            str.Append("================================").AppendLine();
            str.Append("Computer Interface v").Append(PluginInfo.VERSION).AppendLine();
            str.Append("        by Toni Macaroni").AppendLine();
            str.Append("================================").AppendLine();
        }

        public void DrawMods(StringBuilder str)
        {
            for (var i = 0; i < _modEntries.Count; i++)
            {
                var entry = _modEntries[i];
                str.Append(_currentCursorPosition == i ? "> " : "  ");
                str.Append(entry.EntryName).Append("\n");
            }
        }

        public override void OnShow()
        {
            base.OnShow();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            switch (key)
            {
                case EKeyboardKey.Option1:
                    SelectPrevMod();
                    break;
                case EKeyboardKey.Option2:
                    SelectNextMod();
                    break;
                case EKeyboardKey.Enter:
                    ShowModView();
                    break;
            }
        }

        public void ShowModView()
        {
            if (_currentCursorPosition < 0 || _currentCursorPosition > _modEntries.Count - 1) return;

            ShowView(_modEntries[_currentCursorPosition].EntryViewType);
        }

        public void SelectPrevMod()
        {
            if (_modEntries.Count < 1) return;

            _currentCursorPosition--;

            ClampSelection();
            Redraw();
        }

        public void SelectNextMod()
        {
            if (_modEntries.Count < 1) return;

            _currentCursorPosition++;

            ClampSelection();
            Redraw();
        }

        public void ClampSelection()
        {
            if (_currentCursorPosition < 0)
            {
                _currentCursorPosition = 0;
            }
            else if (_currentCursorPosition > _modEntries.Count - 1)
            {
                _currentCursorPosition = _modEntries.Count - 1;
            }
        }
    }
}