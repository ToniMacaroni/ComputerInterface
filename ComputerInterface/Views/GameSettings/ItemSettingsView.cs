using ComputerInterface.ViewLib;
using System.Text;
using UnityEngine;

namespace ComputerInterface.Views.GameSettings
{
    public class ItemSettingsView : ComputerView
    {
        private readonly UISelectionHandler _selectionHandler;

        private float _insVolumeFloat = 0.10f;

        private ItemSettingsView()
        {
            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down);
            _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}> ></color> ", "", "   ", "");
            _selectionHandler.MaxIdx = 1;
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            UpdateState();
            Redraw();
        }

		void UpdateState()
        {
			_selectionHandler.CurrentSelectionIndex = BaseGameInterface.GetItemMode() ? 1 : 0;
            _insVolumeFloat = BaseGameInterface.GetInstrumentVolume();
        }

        private void Redraw()
        {
            var str = new StringBuilder();

            str.BeginCenter().Repeat("=", SCREEN_WIDTH).AppendLine();
            str.Append("Item Tab").AppendLine();
            str.AppendClr("0 - 9 to set Instrument Volume", "ffffff50").AppendLine();
            str.Repeat("=", SCREEN_WIDTH).EndAlign().AppendLines(2);

            str.Append("Instrument Volume: ").Append(Mathf.CeilToInt(_insVolumeFloat * 50f));
            str.AppendLines(3);

            str.Append("Item Particles:").AppendLine();
            str.Append(_selectionHandler.GetIndicatedText(0, "Enabled")).AppendLine();
            str.Append(_selectionHandler.GetIndicatedText(1, "Disabled")).AppendLine();

            SetText(str);
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_selectionHandler.HandleKeypress(key))
            {
                BaseGameInterface.SetItemMode(_selectionHandler.CurrentSelectionIndex == 1);
                Redraw();
                return;
            }
            
            if (key.TryParseNumber(out var num))
            {
                BaseGameInterface.SetInstrumentVolume(num);
                UpdateState();
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
    }
}