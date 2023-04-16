using System.Text;
using ComputerInterface.ViewLib;
using UnityEngine;

namespace ComputerInterface.Views.GameSettings
{
    internal class VoiceSettingsView : ComputerView
    {
        private readonly UISelectionHandler _selectionHandler;
        private bool SwitchedName = false;

        public VoiceSettingsView()
        {
            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down);
            _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}> ></color> ", "", "   ", "");
            _selectionHandler.MaxIdx = 1;
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            UpdateState();
            Redraw();
        }

        public void UpdateState() => _selectionHandler.CurrentSelectionIndex = BaseGameInterface.GetVoiceMode() ? 0 : 1;

        public void Redraw()
        {
            var str = new StringBuilder();

            str.BeginCenter().Repeat("=", SCREEN_WIDTH).AppendLine();
            str.Append("Voice Tab").AppendLine();
            str.AppendClr(!SwitchedName ? "Enter to save" : $"Voice Chat is now {(BaseGameInterface.GetVoiceMode() ? "Enabled" : "Disabled")}", "ffffff50").AppendLine();
            str.Repeat("=", SCREEN_WIDTH).EndAlign().AppendLines(2);

            str.AppendLine("Voice Chat: ");
            DrawOptions(ref str);

            SetText(str);
        }

        public void DrawOptions(ref StringBuilder str)
        {
            str.Append(_selectionHandler.GetIndicatedText(0, "Enabled")).AppendLine();
            str.Append(_selectionHandler.GetIndicatedText(1, "Disabled")).AppendLine();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            SwitchedName = false;
            switch (key)
            {
                case EKeyboardKey.Enter:
                    BaseGameInterface.SetVoiceMode(_selectionHandler.CurrentSelectionIndex == 0);
                    SwitchedName = true;
                    Redraw();
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