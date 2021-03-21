using System.Text;
using ComputerInterface.ViewLib;

namespace ComputerInterface.Views.GameSettings
{
    internal class QueueSettingsView : ComputerView
    {
        private readonly UISelectionHandler _selectionHandler;

        public QueueSettingsView()
        {
            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down);
            _selectionHandler.ConfigureSelectionIndicator("", $"<color=#{PrimaryColor}> <</color>", "", "");
            _selectionHandler.MaxIdx = 1;
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            UpdateState();
            Redraw();
        }

        public void UpdateState()
        {
            _selectionHandler.CurrentSelectionIndex = (int)BaseGameInterface.GetQueueMode();
        }

        public void SetMode()
        {
            BaseGameInterface.SetQueueMode((BaseGameInterface.EQueueMode)_selectionHandler.CurrentSelectionIndex);
        }

        public void Redraw()
        {
            var str = new StringBuilder();

            str.AppendLines(3);

            DrawOptions(str);

            SetText(str);
        }

        public void DrawOptions(StringBuilder str)
        {
            str.Append(_selectionHandler.GetIndicatedText(0, "Default    ")).AppendLine();
            str.Append(_selectionHandler.GetIndicatedText(1, "Competitive")).AppendLine();
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
                case EKeyboardKey.Back:
                    ShowView<GameSettingsView>();
                    break;
            }
        }
    }
}