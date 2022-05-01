using System.Text;
using ComputerInterface.ViewLib;

namespace ComputerInterface.Views.GameSettings
{
    internal class GroupView : ComputerView
    {
        private readonly UISelectionHandler _selectionHandler;

        public GroupView()
        {
            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down);
            _selectionHandler.ConfigureSelectionIndicator("", $"<color=#{PrimaryColor}> <</color>", "", "");
            _selectionHandler.MaxIdx = 2;
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            UpdateState();
            Redraw();
        }

        public void UpdateState()
        {
            _selectionHandler.CurrentSelectionIndex = (int)BaseGameInterface.GetGroupMode();
        }

        public void SetMode()
        {
            BaseGameInterface.SetGroupMode((BaseGameInterface.EGroup)_selectionHandler.CurrentSelectionIndex);
        }

        public void Join()
        {
            BaseGameInterface.JoinAsGroup();
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
            str.Repeat("=", SCREEN_WIDTH).EndColor().EndAlign().AppendLine().AppendLine();
        }

        public void DrawOptions(StringBuilder str)
        {
            str.Append(_selectionHandler.GetIndicatedText(0, "Forest")).AppendLine();
            str.Append(_selectionHandler.GetIndicatedText(1, "Cave  ")).AppendLine();
            str.Append(_selectionHandler.GetIndicatedText(2, "Canyon")).AppendLine();
            str.Append(_selectionHandler.GetIndicatedText(3, "City")).AppendLine();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_selectionHandler.HandleKeypress(key))
            {
                SetMode();
                Redraw();
                return;
            }

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
            }
        }
    }
}