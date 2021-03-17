using System.Linq;
using System.Text;
using ComputerInterface.ViewLib;
using Photon.Realtime;

namespace ComputerInterface.RoomBrowser
{
    internal class RoomListView : ComputerView
    {
        private readonly RoomExplorer _roomExplorer;
        private readonly UIElementPageHandler<RoomInfo> _pageHandler;
        private readonly UISelectionHandler _selectionHandler;

        private RoomInfo[] _rooms;

        public RoomListView(RoomExplorer roomExplorer)
        {
            _roomExplorer = roomExplorer;
            _pageHandler = new UIElementPageHandler<RoomInfo>(EKeyboardKey.Left, EKeyboardKey.Right);
            _pageHandler.EntriesPerPage = 6;

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
            _rooms = _roomExplorer.Rooms.Where(x => x.PlayerCount != x.MaxPlayers).ToArray();
            _pageHandler.SetElements(_rooms);
        }

        public void Redraw()
        {
            var str = new StringBuilder();

            DrawRooms(str);

            SetText(str);
        }

        public void DrawRooms(StringBuilder str)
        {
            _selectionHandler.MaxIdx = _pageHandler.ItemsOnScreen - 1;

            _pageHandler.EnumarateElements((room, idx) =>
            {
                str.Repeat(" ", 3).Append(_selectionHandler.GetIndicatedText(idx, room.Name.PadRight(10)));
                str.AppendClr($"{room.PlayerCount}/{room.MaxPlayers}", "ffffff50");
                str.AppendLine();
            });
        }

        private void OnRoomSelect(int idx)
        {
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
                    // Create Room
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