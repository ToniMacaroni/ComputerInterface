using ComputerInterface.ViewLib;
using System.Text;
using UnityEngine;

namespace ComputerInterface.Views.GameSettings
{
    public class CreditsView : ComputerView
    {
        private readonly UISelectionHandler _selectionHandler;

        private CreditsView()
        {
            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down);
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            Redraw();
        }

        private void Redraw()
        {
            var str = new StringBuilder();

            str.AppendLines(1);

            str.Append("Credits\n\nGame by Kerestell\n\n\"Monke Need To Swing\"\nComposed by Stunshine\nProduced by Audiopfeil & Owlobe\n\n\"Cave Wave\" & \"Campfire\"\nComposed by Stunshine\nSound design by David Anderson Kirk");

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

        private string GetSelector(int idx)
        {
            return idx == _selectionHandler.CurrentSelectionIndex ? "<color=#ed6540> <</color>" : "  ";
        }
    }
}
