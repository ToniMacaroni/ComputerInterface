using ComputerInterface.ViewLib;
using System.Text;

namespace ComputerInterface.Views.GameSettings
{
    public class NameSettingView : ComputerView
    {
        private readonly UITextInputHandler _textInputHandler;
        private bool switchedName = false;
        private BaseGameInterface.WordCheckResult errorReason;

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

            str.AppendLines(6);

            if (switchedName)
            {
                str.AppendClr(errorReason switch {
                    BaseGameInterface.WordCheckResult.Allowed => $"Changed name to {BaseGameInterface.GetName()}",
                    _ => $"Error: {BaseGameInterface.WordCheckResultToMessage(errorReason)}.",
                }, "ffffff50").AppendLine();
            }
            else
            {
                str.AppendClr("Press Enter to update your name.", "ffffff50").AppendLine();
            }

            Text = str.ToString();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            switchedName = false;

            switch (key)
            {
                case EKeyboardKey.Enter:
                    errorReason = BaseGameInterface.SetName(_textInputHandler.Text);
                    switchedName = true;
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