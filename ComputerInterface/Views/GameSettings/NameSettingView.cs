using ComputerInterface.ViewLib;
using System.Text;

namespace ComputerInterface.Views.GameSettings
{
    public class NameSettingView : ComputerView
    {
        private readonly UITextInputHandler _textInputHandler;
        private bool SwitchedName = false;
        private bool DisplayOutcome = false;
        private string ErrorReason = "";

        public NameSettingView()
        {
            _textInputHandler = new UITextInputHandler();
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            _textInputHandler.Text = BaseGameInterface.GetName();
            Redraw();
        }

        private void Redraw()
        {
            var str = new StringBuilder();
            var hasComputer = BaseGameInterface.CheckForComputer(out var computer);
            str.Repeat("=", SCREEN_WIDTH).AppendLine();
            str.BeginCenter().Append("Name Tab").AppendLine();
            str.AppendClr($"{(hasComputer ? $"Current name: {computer.savedName}" : "Current name not found")}", "ffffff50").EndAlign().AppendLine();
            str.Repeat("=", SCREEN_WIDTH).AppendLines(2);

            str.BeginColor("ffffff50").Append("> ").EndColor().AppendClr(_textInputHandler.Text, "ffffffff").AppendClr("_", "ffffff50");

            str.AppendLines(6)
                .AppendClr($" * {(!SwitchedName ? "Press Enter to update your name." : (!DisplayOutcome ? $"Error: {ErrorReason}." : $"Changed name to {BaseGameInterface.GetName()}"))}", "ffffff50").AppendLine();

            Text = str.ToString();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            SwitchedName = false;

            switch (key)
            {
                case EKeyboardKey.Enter:
                    BaseGameInterface.SetName(_textInputHandler.Text, out bool error, out ErrorReason);
                    SwitchedName = true;
                    DisplayOutcome = !error;
                    Redraw();
                    break;
                case EKeyboardKey.Back:
                    _textInputHandler.Text = BaseGameInterface.GetName();
                    ShowView<GameSettingsView>();
                    break;
                default:
                    if (_textInputHandler.HandleKey(key))
                    {
                        if (_textInputHandler.Text.Length > BaseGameInterface.MAX_NAME_LENGTH)
                        {
                            _textInputHandler.Text = _textInputHandler.Text.Substring(0, BaseGameInterface.MAX_NAME_LENGTH);
                        }

                        Redraw();
                        return;
                    }
                    break;
            }
        }
    }
}