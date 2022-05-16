using ComputerInterface.ViewLib;
using System.Text;
using UnityEngine;

namespace ComputerInterface.Views.GameSettings
{
    public class ItemSettingsView : ComputerView
    {
        private readonly UISelectionHandler _selectionHandler;

        private float _insVolumeFloat = 0.10f;

        // 0 = 0.00
        // 9 = 0.18
        // 5 = 0.10

        private ItemSettingsView()
        {
            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down);
            _selectionHandler.MaxIdx = 1;
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            _selectionHandler.CurrentSelectionIndex = (int)BaseGameInterface.GetItemMode();
            _insVolumeFloat = BaseGameInterface.GetInstrumentVolume();
            Redraw();
        }

        private void Redraw()
        {
            var str = new StringBuilder();

            _insVolumeFloat = BaseGameInterface.GetInstrumentVolume(); // How in the world does this code work lol

            str.BeginCenter().Repeat("=", SCREEN_WIDTH).AppendLine();
            str.Append("Item Mode").AppendLine();
            str.AppendClr("1 - 9 to set instrument volume", "ffffff50").AppendLine();
            str.Repeat("=", SCREEN_WIDTH).EndAlign().AppendLines(2);

            str.Append("Enabled".PadRight(6)).Append(GetSelector(0)).AppendLine();
            str.Append("Disabled".PadRight(6)).Append(GetSelector(1)).AppendLines(2);
            str.Append("Instrument Volume: ".PadRight(6)).Append(_insVolumeFloat);

            Text = str.ToString();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_selectionHandler.HandleKeypress(key))
            {
                BaseGameInterface.SetItemMode((BaseGameInterface.EItemMode)_selectionHandler.CurrentSelectionIndex);
                Redraw();
                return;
            }

            if (key.TryParseNumber(out var num))
            {
                BaseGameInterface.SetInstrumentVolume(num);
                Redraw();
                return;
            }

            switch (key)
            {
                case EKeyboardKey.Back:
                    ShowView<GameSettingsView>();
                    break;
            }
        }

        private string GetSelector(int idx)
        {
            return idx == _selectionHandler.CurrentSelectionIndex ? "<color=#ed6540> <</color>" : "  ";
        }
    }
}