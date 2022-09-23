using ComputerInterface.ViewLib;
using System.Text;
using UnityEngine;

namespace ComputerInterface.Views.GameSettings
{
    public class CreditsView : ComputerView
    {
        private int page = 1;

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            Redraw();
        }

        private void Redraw()
        {
            var str = new StringBuilder();

            str.Repeat("=", SCREEN_WIDTH).AppendLines(2);

            str.AppendLine("Credits (1 - 3 to change change pages)").AppendLine();

            if (page == 1)
            {
                str.AppendLine("Game by Another Axiom").AppendLine()
                    .AppendLine("(Kerestell, David Yee, David Neubelt)")
                    .AppendLines(6).Repeat("=", SCREEN_WIDTH);
            }
            else if (page == 2)
            {
                str.AppendLine("\"Monke Need To Swing\"")
                    .AppendLine("Composed by Stunshine")
                    .AppendLine("Produced by Audiopfeil & Owlobe").AppendLine();

                str.AppendLine("\"Cave Wave\" & \"Campfire\"")
                    .AppendLine("Composed by Stunshine")
                    .AppendLine("Sound design by David Anderson Kirk")
                    .AppendLine().AppendLine().Repeat("=", SCREEN_WIDTH);
            }
            else // i mean i guess you're on page three, are you??
            {
                str.AppendLine("Additional art by:").AppendLine()
                    .AppendLine("Lulu (Laura) Lorian")
                    .AppendLine("@LuluLorian").AppendLine()
                    .AppendLine("Lilith Tothill")
                    .AppendLines(3).Repeat("=", SCREEN_WIDTH);
            }

            Text = str.ToString();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (key.TryParseNumber(out var num))
            {
                if (num >= 1 && num <= 3)
                {
                    page = num;
                    Redraw();
                    return;
                }
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
