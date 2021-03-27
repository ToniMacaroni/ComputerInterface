using System.Text;
using ComputerInterface.ViewLib;

namespace ComputerInterface.Views.GameSettings
{
    internal class MicSettingsView : ComputerView
    {
        private readonly UISelectionHandler _selectionHandler;

        public MicSettingsView()
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
            _selectionHandler.CurrentSelectionIndex = (int) BaseGameInterface.GetPttMode();
        }

        public void SetMode()
        {
            BaseGameInterface.SetPttMode((BaseGameInterface.EPTTMode)_selectionHandler.CurrentSelectionIndex);
        }

        public void Redraw()
        {
            var str = new StringBuilder();

            str.AppendLines(5);

            DrawOptions(str);

            SetText(str);
        }

        public void DrawOptions(StringBuilder str)
        {
            str.Append(_selectionHandler.GetIndicatedText(0, "All Chat    ")).AppendLine();
            str.Append(_selectionHandler.GetIndicatedText(1, "Push To Talk")).AppendLine();
            str.Append(_selectionHandler.GetIndicatedText(2, "Push To Mute")).AppendLine();
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