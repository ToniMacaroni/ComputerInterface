using System;
using System.Text;
using ComputerInterface.ViewLib;
using static ComputerInterface.BaseGameInterface;

namespace ComputerInterface.Views.GameSettings
{
    internal class MicSettingsView : ComputerView
    {
        private readonly UISelectionHandler _selectionHandler;
        private bool hasSwitched = false;
        private string microphoneMode = "";

        public MicSettingsView()
        {
            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down);
            _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}> ></color> ", "", "   ", "");
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

            DrawHeader(ref str);
            DrawOptions(ref str);

            SetText(str);
        }

        public void DrawHeader(ref StringBuilder str)
        {
            str.BeginCenter().Repeat("=", SCREEN_WIDTH).AppendLine();
            str.Append("Mic Tab").AppendLine();
            str.AppendClr(hasSwitched ? $"Set mode to {microphoneMode}" : "Enter to save", "ffffff50").AppendLine();
            str.Repeat("=", SCREEN_WIDTH).EndAlign().AppendLines(2);

            str.AppendLine("Mic Mode: ");
        }

        public void DrawOptions(ref StringBuilder str)
        {
            str.Append(_selectionHandler.GetIndicatedText(0, "All Chat    ")).AppendLine()
                .Append(_selectionHandler.GetIndicatedText(1, "Push To Talk")).AppendLine()
                .Append(_selectionHandler.GetIndicatedText(2, "Push To Mute")).AppendLine();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            hasSwitched = false;
            switch (key)
            {
                case EKeyboardKey.Enter:
                    SetMode();
                    microphoneMode = (EPTTMode)_selectionHandler.CurrentSelectionIndex switch
                    {
                        EPTTMode.AllChat => "All Chat",
                        EPTTMode.PushToTalk => "Push to Talk",
                        EPTTMode.PushToMute => "Push To Mute",
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    hasSwitched = true;
                    Redraw();
                    return;
                case EKeyboardKey.Back:
                    ShowView<GameSettingsView>();
                    break;
                default:
                    if (_selectionHandler.HandleKeypress(key)) Redraw();
                    break;
            }
        }
    }
}