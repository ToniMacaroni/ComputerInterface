using System.Text;
using ComputerInterface.ViewLib;

namespace ComputerInterface.Views.GameSettings
{
    internal class VoiceSettingsView : ComputerView
    {
        private readonly UISelectionHandler _selectionHandler;

        public VoiceSettingsView()
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
            _selectionHandler.CurrentSelectionIndex = BaseGameInterface.GetVoiceMode()?0:1;
        }

        public void SetMode()
        {
            BaseGameInterface.SetVoiceMode(_selectionHandler.CurrentSelectionIndex == 0);
        }

        public void Redraw()
        {
            var str = new StringBuilder();

            str.BeginCenter().Repeat("=", SCREEN_WIDTH).AppendLine();
            str.Append("Voice Chat").AppendLine();
            str.AppendClr("Back to save", "ffffff50").AppendLine();
            str.Repeat("=", SCREEN_WIDTH).EndAlign().AppendLines(2);

            str.AppendClr("Hear other players?", "ffffff60").AppendLine();

            DrawOptions(str);

            SetText(str);
        }

        public void DrawOptions(StringBuilder str)
        {
            str.Append(_selectionHandler.GetIndicatedText(0, "Yes  ")).AppendLine();
            str.Append(_selectionHandler.GetIndicatedText(1, "No   ")).AppendLine();
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