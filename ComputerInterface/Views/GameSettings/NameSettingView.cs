using System.Text;
using System.Xml.Linq;
using BepInEx;
using ComputerInterface.ViewLib;

namespace ComputerInterface.Views.GameSettings
{
    public class NameSettingView : ComputerView
    {
        private readonly UITextInputHandler _textInputHandler;

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

            str.Repeat("=", SCREEN_WIDTH).AppendLine();
            str.BeginCenter().Append("Name").AppendLine();
            str.AppendClr("Back to save", "ffffff50").EndAlign().AppendLine();
            str.Repeat("=", SCREEN_WIDTH).AppendLine();
            str.AppendLine();
            str.BeginColor("ffffff50").Append("> ").EndColor().Append(_textInputHandler.Text).AppendClr("_", "ffffff50");

            Text = str.ToString();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_textInputHandler.HandleKey(key))
            {
                if (_textInputHandler.Text.Length > BaseGameInterface.MAX_NAME_LENGTH)
                {
                    _textInputHandler.Text = _textInputHandler.Text.Substring(0, BaseGameInterface.MAX_NAME_LENGTH);
                }
                Redraw();
                return;
            }

            switch (key)
            {
                case EKeyboardKey.Back:
                    if (_textInputHandler.Text.IsNullOrWhiteSpace())
                    {
                        _textInputHandler.Text = BaseGameInterface.GetName();
                    }
                    else
                    {
                        BaseGameInterface.SetName(_textInputHandler.Text);
                    }
                    ShowView<GameSettingsView>();
                    break;
            }
        }
    }
}