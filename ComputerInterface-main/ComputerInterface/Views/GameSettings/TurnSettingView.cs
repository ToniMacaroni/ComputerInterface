using ComputerInterface.ViewLib;
using System.Text;

namespace ComputerInterface.Views.GameSettings
{
    public class TurnSettingView : ComputerView
    {
        private readonly UISelectionHandler _selectionHandler;

        private int _turnSpeed = 4;

        private TurnSettingView()
        {
            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down);
            _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}> ></color> ", "", "   ", "");
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
            str.Append("Turn Tab").AppendLine();
            str.AppendClr("1 - 9 to change turn speed", "ffffff50").AppendLine();
            str.Repeat("=", SCREEN_WIDTH).EndAlign().AppendLines(2);

            str.AppendLine("Turn Mode: ");
            str.Append(_selectionHandler.GetIndicatedText(0, "Snap")).AppendLine()
                .Append(_selectionHandler.GetIndicatedText(1, "Smooth")).AppendLine()
                .Append(_selectionHandler.GetIndicatedText(2, "None")).AppendLines(2);

            str.Append("Speed: ")
                .Append(_turnSpeed);

            Text = str.ToString();
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
                    break;
            }
        }
    }
}
