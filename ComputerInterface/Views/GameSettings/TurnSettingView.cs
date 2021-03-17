using ComputerInterface.ViewLib;
using System.Text;

namespace ComputerInterface.Views.GameSettings
{
    public class TurnSettingView : ComputerView
    {
        private readonly UISelectionHandler _selectionHandler;

        private TurnSettingView()
        {
            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down);
            _selectionHandler.MaxIdx = 2;
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            _selectionHandler.CurrentSelectionIndex = (int)BaseGameInterface.GetTurnMode();
            Redraw();
        }

        private void Redraw()
        {
            var str = new StringBuilder();

            str.Repeat("=", SCREEN_WIDTH).AppendLine();
            str.BeginCenter().Append("Turn Mode").EndAlign().AppendLine();
            str.Repeat("=", SCREEN_WIDTH).AppendLine();

            str.Append("\n\n");
            str.Append("Snap".PadRight(6)).Append(GetSelector(0)).AppendLine();
            str.Append("Smooth".PadRight(6)).Append(GetSelector(1)).AppendLine();
            str.Append("None".PadRight(6)).Append(GetSelector(2));

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