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
            _selectionHandler.ConfigureSelectionIndicator("", $"<color=#{PrimaryColor}> <</color>", "", "");
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
			_selectionHandler.CurrentSelectionIndex = BaseGameInterface.GetItemMode() ? 0 : 1;
            _insVolumeFloat = BaseGameInterface.GetInstrumentVolume();
		}

        private void Redraw()
        {
            var str = new StringBuilder();

            str.BeginCenter().Repeat("=", SCREEN_WIDTH).AppendLine();
            str.Append("Item Mode").AppendLine();
            str.AppendClr("1 - 9 to set instrument volume", "ffffff50").AppendLine();
            str.Repeat("=", SCREEN_WIDTH).EndAlign().AppendLines(2);

            str.Append("Instrument Volume: ").Append(Mathf.CeilToInt(_insVolumeFloat * 50f));
            str.AppendLines(2);

            str.AppendClr("Hide item particles?", "ffffff60").AppendLine();
            str.Append(_selectionHandler.GetIndicatedText(0, "Yes  ")).AppendLine();
            str.Append(_selectionHandler.GetIndicatedText(1, "No   ")).AppendLine();

            SetText(str);
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_selectionHandler.HandleKeypress(key))
            {
				BaseGameInterface.SetItemMode(_selectionHandler.CurrentSelectionIndex == 0);
                UpdateState();
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