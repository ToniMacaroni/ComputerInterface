using System.Linq;
using System.Text;
using ComputerInterface.ViewLib;
using Photon.Pun;
using Photon.Realtime;

namespace ComputerInterface.RoomBrowser
{
    internal class RoomListView : ComputerView
    {
        private readonly CIRoomManager _ciRoomManager;
        private readonly UIElementPageHandler<RoomInfo> _pageHandler;
        private readonly UISelectionHandler _selectionHandler;

        private RoomInfo[] _rooms;

        public RoomListView(CIRoomManager ciRoomManager)
        {
            _ciRoomManager = ciRoomManager;
            _pageHandler = new UIElementPageHandler<RoomInfo>(EKeyboardKey.Left, EKeyboardKey.Right);
            _pageHandler.EntriesPerPage = 5;

            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter);
            _selectionHandler.ConfigureSelectionIndicator("<color=#ed6540>> </color>", "", "  ", "");
            _selectionHandler.OnSelected += OnRoomSelect;
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);

            RefreshPages();
            Redraw();
        }

        public void RefreshPages()
        {
            _rooms = _ciRoomManager.Rooms.Where(x => x.PlayerCount != x.MaxPlayers).ToArray();
            _pageHandler.SetElements(_rooms);
        }

        public void Redraw()
        {
            var str = new StringBuilder();

            DrawHeader(str);
            DrawRooms(str);

            SetText(str);
        }

        public void DrawHeader(StringBuilder str)
        {
            str.BeginColor("ffffff50").Append("[Option 1] Create Room").EndColor().AppendLines(2);
        }

        public void DrawRooms(StringBuilder str)
        {
            _selectionHandler.MaxIdx = _pageHandler.ItemsOnScreen - 1;

            if (_rooms.Length == 0 && PhotonNetwork.CurrentRoom == null)
            {
                str.Repeat(" ", 3).Append("-No Rooms-");
                return;
            }

            var isInRoom = PhotonNetwork.CurrentRoom != null;

            if (isInRoom)
            {
                var room = PhotonNetwork.CurrentRoom;
                str.Repeat(" ", 3).AppendClr("Joined: ", "ffffff50").Append(room.Name.PadRight(19));
                str.AppendClr($"{room.PlayerCount}/{room.MaxPlayers}", "ffffff50");
                str.AppendLine();
            }

            if (_rooms.Length == 0) return;

            _pageHandler.EnumarateElements((room, idx) =>
            {
                str.Repeat(" ", 3).Append(_selectionHandler.GetIndicatedText(idx, room.Name.PadRight(25)));
                str.AppendClr($"{room.PlayerCount}/{room.MaxPlayers}", "ffffff50");
                str.AppendLine();
            });

            str.AppendLine();

            if (!isInRoom) str.AppendLine();

            _pageHandler.AppendFooter(str);
        }

        private void OnRoomSelect(int idx)
        {
            if (_rooms.Length < 1) return;

            var roomIdx = _pageHandler.GetAbsoluteIndex(idx);
            var room = _rooms[roomIdx];
            ShowView<RoomDetailsView>(room);
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_pageHandler.HandleKeyPress(key))
            {
                _selectionHandler.CurrentSelectionIndex = 0;
                Redraw();
                return;
            }

            if (_selectionHandler.HandleKeypress(key))
            {
                Redraw();
                return;
            }

            switch (key)
            {
                case EKeyboardKey.Option1:
                    ShowView<CreateRoomView>();
                    break;
                case EKeyboardKey.Option2:
                    break;
                case EKeyboardKey.Option3:
                    RefreshPages();
                    Redraw();
                    break;
                case EKeyboardKey.Back:
                    ReturnToMainMenu();
                    break;
            }
        }
    }
}