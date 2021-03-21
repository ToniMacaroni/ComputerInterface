using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using BepInEx;
using ComputerInterface.ViewLib;
using Photon.Pun;

namespace ComputerInterface.RoomBrowser
{
    internal class CreateRoomView : ComputerView
    {
        private readonly CIRoomManager _ciRoomManager;
        private readonly UISelectionHandler _settingSelector;
        private readonly UITextInputHandler _roomInputHandler;
        private readonly UITextInputHandler _descriptionInputHandler;

        private bool _isShowingMessage;

        public CreateRoomView(CIRoomManager ciRoomManager)
        {
            _ciRoomManager = ciRoomManager;

            _settingSelector = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down);
            _settingSelector.MaxIdx = 1;

            _roomInputHandler = new UITextInputHandler();
            _roomInputHandler.Validator = text => !text.IsNullOrWhiteSpace();

            _descriptionInputHandler = new UITextInputHandler();
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            Redraw();
        }

        private void Redraw()
        {
            var str = new StringBuilder();

            str.BeginCenter().AppendClr("▲ / ▼ to change input field", "ffffff50").EndAlign();

            str.AppendLines(3);
            str.Repeat(" ", 3);
            str.AppendClr("Name: ", "ffffff50").Append(_roomInputHandler.Text);
            if(_settingSelector.CurrentSelectionIndex==0) str.AppendClr("_", "ffffff50");

            str.AppendLine();
            str.Repeat(" ", 3);
            str.AppendClr("Description: ", "ffffff50").Append(_descriptionInputHandler.Text);
            if (_settingSelector.CurrentSelectionIndex == 1) str.AppendClr("_", "ffffff50");

            str.AppendLines(3);
            str.BeginCenter().AppendClr("[", PrimaryColor).Append("Create").AppendClr("]", PrimaryColor).EndAlign();

            SetText(str);
        }

        private async void CreateRoom()
        {
            if (!_roomInputHandler.IsValid) return;

            var desc = _descriptionInputHandler.Text.IsNullOrWhiteSpace()
                ? "No Description"
                : _descriptionInputHandler.Text;

            if (desc.Length > 200)
            {
                ShowDescriptionError();
                return;
            }

            if (PhotonNetwork.CurrentRoom != null)
            {
                _isShowingMessage = true;
                _ciRoomManager.RegisterConnectedToMasterCallback(CreateRoom);
                SetText(str =>
                {
                    str.AppendLines(3).BeginCenter();
                    str.Append("Disconnecting...").EndAlign();
                });
                BaseGameInterface.Disconnect();
                return;
            }


            _isShowingMessage = true;

            _ciRoomManager.CreateRoom(_roomInputHandler.Text, desc);

            SetText(str =>
            {
                str.AppendLines(3).BeginCenter();
                str.Append("Room Created!").EndAlign();
            });

            await Task.Delay(700);

            var timeout = 0;
            while (PhotonNetwork.CurrentRoom == null && timeout < 10)
            {
                await Task.Delay(500);
                timeout++;
            }

            ShowView<RoomListView>();

            _isShowingMessage = false;
        }

        private async void ShowDescriptionError()
        {
            _isShowingMessage = true;

            SetText(str =>
            {
                str.AppendLines(2);
                str.BeginCenter().Append("Description Over 200 chars long").EndAlign();
            });

            await Task.Delay(1000);

            _isShowingMessage = false;
            Redraw();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_isShowingMessage)
            {
                return;
            }

            if (_settingSelector.HandleKeypress(key))
            {
                Redraw();
                return;
            }

            if (_settingSelector.CurrentSelectionIndex == 0)
            {
                if (_roomInputHandler.HandleKey(key))
                {
                    Redraw();
                    return;
                }
            }
            else if (_settingSelector.CurrentSelectionIndex == 1)
            {
                if (_descriptionInputHandler.HandleKey(key))
                {
                    Redraw();
                    return;
                }
            }

            switch (key)
            {
                case EKeyboardKey.Enter:
                    CreateRoom();
                    break;
                case EKeyboardKey.Back:
                    ShowView<RoomListView>();
                    break;
            }
        }
    }
}