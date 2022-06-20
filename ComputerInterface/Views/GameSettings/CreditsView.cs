using ComputerInterface.ViewLib;
using System.Text;
using UnityEngine;

namespace ComputerInterface.Views.GameSettings
{
    public class CreditsView : ComputerView
    {
        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            Redraw();
        }

        private void Redraw()
        {
            var str = new StringBuilder();

            str.Repeat("=", SCREEN_WIDTH).AppendLine();

            str.AppendLine("Credits").AppendLine()
                .AppendLine("Game by Kerestell").AppendLine();

            str.AppendLine("\"Monke Need To Swing\"")
                .AppendLine("Composed by Stunshine")
                .AppendLine("Produced by Audiopfeil & Owlobe").AppendLine();

            str.AppendLine("\"Cave Wave\" & \"Campfire\"")
                .AppendLine("Composed by Stunshine")
				.AppendLine("Sound design by David Anderson Kirk");

            Text = str.ToString();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            switch (key)
            {
                case EKeyboardKey.Back:
                    ShowView<GameSettingsView>();
                    break;
            }
        }
    }
}
