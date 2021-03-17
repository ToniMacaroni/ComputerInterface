using System.Text;
using ComputerInterface.ViewLib;
using Photon.Realtime;

namespace ComputerInterface.RoomBrowser
{
    internal class RoomDetailsView : ComputerView
    {
        private RoomInfo _room;

        public RoomDetailsView()
        {
            
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);

            if (args == null || args.Length < 1)
            {
                return;
            }

            _room = (RoomInfo) args[0];

            Redraw();
        }

        private void Redraw()
        {
            var str = new StringBuilder();

            str.AppendLine();
            str.BeginCenter().Append(_room.Name).AppendLine();

            str.AppendClr("Map: ", "ffffff50").Append(_room.CustomProperties["gameMode"]).AppendLine();
            str.AppendClr("IsOpen: ", "ffffff50").Append(_room.IsOpen).AppendLine();
            str.AppendClr("IsVisible: ", "ffffff50").Append(_room.IsVisible).AppendLine();
            str.AppendLines(3);
            str.AppendClr("[", PrimaryColor).Append("Join").AppendClr("]", PrimaryColor);

            SetText(str);
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            switch (key)
            {
                case EKeyboardKey.Enter:
                    BaseGameInterface.JoinRoom(_room.Name);
                    ShowView<RoomListView>();
                    break;
                case EKeyboardKey.Back:
                    ShowView<RoomListView>();
                    break;
            }
        }
    }
}