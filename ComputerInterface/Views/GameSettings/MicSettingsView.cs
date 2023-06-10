using System;
using System.Text;
using ComputerInterface.ViewLib;
using static ComputerInterface.BaseGameInterface;

namespace ComputerInterface.Views.GameSettings
{
    internal class MicSettingsView : ComputerView
    {
        private readonly UISelectionHandler _selectionHandler;

        public MicSettingsView()
        {
            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down);
            _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}> ></color> ", "", "   ", "");
            _selectionHandler.MaxIdx = 2;
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            _selectionHandler.CurrentSelectionIndex = (int) BaseGameInterface.GetPttMode();
            Redraw();
        }

        public void Redraw()
        {
            var str = new StringBuilder();

            str.BeginCenter().Repeat("=", SCREEN_WIDTH).AppendLine();
            str.Append("Mic Tab").AppendLine();
            str.Repeat("=", SCREEN_WIDTH).EndAlign().AppendLines(2);

            str.AppendLine("Mic Mode: ");
            str.AppendLine(_selectionHandler.GetIndicatedText(0, "All Chat"));
            str.AppendLine(_selectionHandler.GetIndicatedText(1, "Push To Talk"));
            str.AppendLine(_selectionHandler.GetIndicatedText(2, "Push To Mute"));

            SetText(str);
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_selectionHandler.HandleKeypress(key))
            {
                BaseGameInterface.SetPttMode((BaseGameInterface.EPTTMode)_selectionHandler.CurrentSelectionIndex);
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