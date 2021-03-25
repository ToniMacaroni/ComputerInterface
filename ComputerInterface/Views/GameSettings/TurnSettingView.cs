using ComputerInterface.ViewLib;
using System.Text;
using UnityEngine;

namespace ComputerInterface.Views.GameSettings
{
    public class TurnSettingView : ComputerView
    {
        private readonly UISelectionHandler _selectionHandler;

        private int _turnSpeed = 4;

        private TurnSettingView()
        {
            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down);
            _selectionHandler.MaxIdx = 2;
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            _selectionHandler.CurrentSelectionIndex = (int)BaseGameInterface.GetTurnMode();
            _turnSpeed = BaseGameInterface.GetTurnValue();
            Redraw();
        }

        private void SetTurnSpeed(int val)
        {
            _turnSpeed = val;
            BaseGameInterface.SetTurnValue(val);
        }

        private void Redraw()
        {
            var str = new StringBuilder();

            str.BeginCenter().Repeat("=", SCREEN_WIDTH).AppendLine();
            str.Append("Turn Mode").AppendLine();
            str.AppendClr("1 - 9 to change turn speed", "ffffff50").AppendLine();
            str.Repeat("=", SCREEN_WIDTH).EndAlign().AppendLines(2);

            str.Append("Snap".PadRight(6)).Append(GetSelector(0)).AppendLine();
            str.Append("Smooth".PadRight(6)).Append(GetSelector(1)).AppendLine();
            str.Append("None".PadRight(6)).Append(GetSelector(2)).AppendLines(2);
            str.Append("Speed".PadRight(6)).Append(_turnSpeed);

            Text = str.ToString();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_selectionHandler.HandleKeypress(key))
            {
                BaseGameInterface.SetTurnMode((BaseGameInterface.ETurnMode)_selectionHandler.CurrentSelectionIndex);
                Redraw();
                return;
            }

            if (key.TryParseNumber(out var num))
            {
                SetTurnSpeed(num);
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