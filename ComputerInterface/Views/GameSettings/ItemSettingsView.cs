using ComputerInterface.ViewLib;
using System.Text;
using UnityEngine;

namespace ComputerInterface.Views.GameSettings
{
    public class ItemSettingsView : ComputerView
    {
        private readonly UISelectionHandler _selectionHandler;
        private float _insVolumeFloat = 0.1f;

        public ItemSettingsView()
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

		void UpdateState() => _insVolumeFloat = BaseGameInterface.GetInstrumentVolume();

        private void Redraw()
        {
            var str = new StringBuilder();

            str.BeginCenter().Repeat("=", SCREEN_WIDTH).AppendLine();
            str.Append("Item Tab").AppendLine();
            str.AppendClr("0 - 9 to set instrument volume", "ffffff50").AppendLine();
            str.Repeat("=", SCREEN_WIDTH).EndAlign().AppendLines(2);

            str.Append("Instrument Volume: ")
              .Append(Mathf.CeilToInt(_insVolumeFloat * 50f));

            str.AppendLines(3);

            str.Append("Item Particles:").AppendLine();
            str.Append(_selectionHandler.GetIndicatedText(0, "Enabled")).AppendLine();
            str.Append(_selectionHandler.GetIndicatedText(1, "Disabled")).AppendLine();

            SetText(str);
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            switch (key)
            {
                case EKeyboardKey.Back:
                    ShowView<GameSettingsView>();
                    break;
                default:
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
                    }
                    break;
            }
        }
    }
}