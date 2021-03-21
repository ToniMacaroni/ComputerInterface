using System.Text;
using BepInEx;
using ComputerInterface.ViewLib;

namespace ComputerInterface.Views.GameSettings
{
    public class JoinRoomView : ComputerView
    {
        private readonly UITextInputHandler _textInputHandler;

        private string _joinedRoom;

        public JoinRoomView()
        {
            _textInputHandler = new UITextInputHandler();
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            Redraw();
        }

        private void Redraw()
        {
            var str = new StringBuilder();

            str.Repeat("=", SCREEN_WIDTH).AppendLine();
            str.BeginCenter().Append("Join Room").AppendLine();

            if (_joinedRoom.IsNullOrWhiteSpace())
            {
                str.AppendClr("Enter to join", "ffffff50").EndAlign().AppendLine();
            }
            else
            {
                str.AppendClr($"Joined room {_joinedRoom}", "ffffff50").EndAlign().AppendLine();
            }

            str.Repeat("=", SCREEN_WIDTH).AppendLine();
            str.AppendLine();
            str.BeginColor("ffffff50").Append("> ").EndColor().Append(_textInputHandler.Text).AppendClr("_", "ffffff50");

            Text = str.ToString();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_textInputHandler.HandleKey(key))
            {
                Redraw();
                return;
            }

            switch (key)
            {
                case EKeyboardKey.Back:
                    ShowView<GameSettingsView>();
                    break;
                case EKeyboardKey.Enter:
                    if (!_textInputHandler.Text.IsNullOrWhiteSpace())
                    {
                        _joinedRoom = _textInputHandler.Text.ToUpper();
                        BaseGameInterface.JoinRoom(_joinedRoom);
                        Redraw();
                    }
                    break;
                case EKeyboardKey.Option1:
                    BaseGameInterface.Disconnect();
                    break;
            }
        }
    }
}