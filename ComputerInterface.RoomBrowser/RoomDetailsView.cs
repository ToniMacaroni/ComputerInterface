using ComputerInterface.ViewLib;
using Photon.Pun;
using Photon.Realtime;
using System.Text;
using System.Threading.Tasks;

namespace ComputerInterface.RoomBrowser
{
    internal class RoomDetailsView : ComputerView
    {
        private readonly CIRoomManager _ciRoomManager;

        private RoomInfo _room;
        private bool _isShowingMessage;

        public RoomDetailsView(CIRoomManager ciRoomManager)
        {
            _ciRoomManager = ciRoomManager;
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);

            if (args == null || args.Length < 1)
            {
                return;
            }

            _room = (RoomInfo)args[0];

            Redraw();
        }

        private void Redraw()
        {
            var str = new StringBuilder();

            str.BeginCenter().Append($"<u>{_room.Name}</u>").EndAlign().AppendLine();
            str.AppendLine();

            //str.Append(" ").AppendClr("Map: ", "ffffff50").Append(_room.CustomProperties["gameMode"]).AppendLine();

            str.Append(" ").BeginColor("ffffff50").Append("Description: ").EndColor();

            var desc = _room.GetDescription() == null ? "Description not found." : _room.GetDescription().ToString();

            if (desc.Length < 50)
            {
                str.Append(desc).AppendLine();
            }
            else
            {
                str.AppendClr("[Option 1]", "ffffff50").AppendLine();
            }

            str.Append(" ").BeginColor("ffffff50").Append("Mods: [Option 2]").EndColor().AppendLine();

            str.AppendLines(4);
            str.BeginCenter().AppendClr("[", PrimaryColor).Append("Join").AppendClr("]", PrimaryColor).EndAlign();

            SetText(str);
        }

        public void ShowDescription()
        {
            ShowView<InfoView>(_room.GetDescription());
        }

        public void ShowMods()
        {
            ShowView<InfoView>(_room.GetMods());
        }

        public async void JoinRoom()
        {
            if (_room.PlayerCount == _room.MaxPlayers) return;

            if (PhotonNetwork.CurrentRoom != null)
            {
                _isShowingMessage = true;
                _ciRoomManager.RegisterConnectedToMasterCallback(JoinRoom);
                SetText(str =>
                {
                    str.AppendLines(3).BeginCenter();
                    str.Append("Disconnecting...").EndAlign();
                });
                BaseGameInterface.Disconnect();
                return;
            }

            BaseGameInterface.JoinRoom(_room.Name, out _, out _);

            _isShowingMessage = true;

            SetText(str =>
            {
                str.AppendLines(3).BeginCenter().Append("Connecting...").EndAlign();
            });

            var timeout = 0;

            while (PhotonNetwork.CurrentRoom == null && timeout < 10)
            {
                await Task.Delay(500);
                timeout++;
            }

            _isShowingMessage = false;

            ShowView<RoomListView>();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_isShowingMessage) return;

            switch (key)
            {
                case EKeyboardKey.Option1:
                    ShowDescription();
                    break;
                case EKeyboardKey.Option2:
                    ShowMods();
                    break;
                case EKeyboardKey.Enter:
                    JoinRoom();
                    break;
                case EKeyboardKey.Back:
                    ShowView<RoomListView>();
                    break;
            }
        }
    }
}