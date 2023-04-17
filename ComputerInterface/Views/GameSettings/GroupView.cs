using ComputerInterface.ViewLib;
using System.Text;

namespace ComputerInterface.Views.GameSettings
{
    internal class GroupView : ComputerView
    {
        private readonly UISelectionHandler _selectionHandler;

        public GroupView()
        {
            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down);
            _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}> ></color> ", "", "   ", "");
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);

            _selectionHandler.MaxIdx = BaseGameInterface.GetGroupJoinMaps().Length - 1;
            _selectionHandler.CurrentSelectionIndex = 0;
            Redraw();
        }

        public void Join()
        {
            BaseGameInterface.JoinGroupMap(_selectionHandler.CurrentSelectionIndex);
            ShowView<JoinRoomView>();
        }

        public void Redraw()
        {
            var str = new StringBuilder();

            DrawHeader(str);
            DrawOptions(str);

            SetText(str);
        }

        public void DrawHeader(StringBuilder str)
        {
            str.BeginCenter().BeginColor("ffffff50").Repeat("=", SCREEN_WIDTH).AppendLine();
            str.Append("Press enter to join").AppendLine();
            str.Append("Option 1 for more info").AppendLine();
            str.Repeat("=", SCREEN_WIDTH).EndColor().EndAlign().AppendLines(2);
        }

        public void DrawOptions(StringBuilder str)
        {
            str.AppendLine("Available maps: ");
            var maps = BaseGameInterface.GetGroupJoinMaps();
            for (int i = 0; i < maps.Length; i++)
            {
                var formattedName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(maps[i]);
                str.Append(_selectionHandler.GetIndicatedText(i, formattedName)).AppendLine();
            }
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            switch (key)
            {
                case EKeyboardKey.Enter:
                    Join();
                    break;
                case EKeyboardKey.Option1:
                    ShowView<GroupInfoView>();
                    break;
                case EKeyboardKey.Back:
                    ShowView<GameSettingsView>();
                    break;
                default:
                    if (_selectionHandler.HandleKeypress(key))
                    {
                        Redraw();
                        return;
                    }
                    break;
            }
        }
    }
}
