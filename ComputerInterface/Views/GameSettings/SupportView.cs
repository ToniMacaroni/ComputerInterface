using System.Text;
using ComputerInterface.ViewLib;
using GorillaNetworking;
using UnityEngine;

namespace ComputerInterface.Views.GameSettings
{
    internal class SupportView : ComputerView
    {
        private bool supportVisible = false;

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            supportVisible = false;

            Redraw();
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
            str.Append("Support Tab").AppendLine();
            str.AppendClr("Only show this to AA support", "ffffff50").AppendLine();
            str.Repeat("=", SCREEN_WIDTH).EndColor().EndAlign().AppendLines(2);
        }

        public void DrawOptions(StringBuilder str)
        {
            if (!supportVisible)
            {
                str.AppendLine("To view support and account inforamtion, press the Option 1 key.").AppendLines(2);
                str.AppendClr("Only show this information to Another Axiom support.", ColorUtility.ToHtmlStringRGB(Color.red));
                SetText(str);
                return;
            }

            str.Append("Player ID: ").Append(PlayFabAuthenticator.instance._playFabPlayerIdCache).AppendLine();
            str.Append("Version: ").Append(GorillaComputer.instance.GetField<string>("version")).AppendLine();
            str.Append("Platform: ").Append("Steam (Modded)").AppendLine();
            str.Append("Build Date: ").Append(GorillaComputer.instance.GetField<string>("buildDate")).AppendLine();
            SetText(str);
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            switch (key)
            {
                case EKeyboardKey.Option1:
                    supportVisible = true;
                    Redraw();
                    break;
                case EKeyboardKey.Back:
                    ShowView<GameSettingsView>();
                    break;
            }
        }
    }
}
