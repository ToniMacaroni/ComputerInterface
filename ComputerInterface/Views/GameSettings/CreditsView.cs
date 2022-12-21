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

            switch (page)
            {
                case 1:
                    str.AppendLine("Game by Another Axiom").AppendLine()
                        .AppendLine("(Kerestell, David Yee, David Neubelt)");

                    str.AppendLine().AppendLine("\"Nice Gorilla Store\"")
                        .AppendLine("Composed by Stunshine & Jaguar Jen,")
                        .AppendLine("Sound design by David Anderson Kirk")
                        .AppendLines(2).Repeat("=", SCREEN_WIDTH);
                    break;

                case 2:
                    str.AppendLine("\"Monke Need To Swing\"")
                        .AppendLine("Composed by Stunshine")
                        .AppendLine("Produced by Audiopfeil & Owlobe").AppendLine();

                    str.AppendLine("\"Cave Wave\", \"Campfire\", \"Monkebone Bash\"")
                        .AppendLine("Composed by Stunshine")
                        .AppendLine("Sound design by David Anderson Kirk")
                        .AppendLine().AppendLine().Repeat("=", SCREEN_WIDTH);
                    break;

                default:
                    str.AppendLine("Additional art by:").AppendLine()
                        .AppendLine("Lulu (Laura) Lorian")
                        .AppendLine("@LuluLorian").AppendLine()
                        .AppendLine("Lilith Tothill")
                        .AppendLines(3).Repeat("=", SCREEN_WIDTH);
                    break;
            }

            Text = str.ToString();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (key.TryParseNumber(out var num) && num >= 1 && num <= 3)
            {
                page = num;
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
