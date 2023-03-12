using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using ComputerInterface.ViewLib;

namespace ComputerInterface.Views.GameSettings
{
    public class CreditsView : ComputerView
    {
        private int ScrollLevel;
        private readonly int MaxPage = 6;
        private readonly List<string> CreditsList = new List<string>();

        public CreditsView()
        {
            SetList();
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            Redraw();
        }

        private void SetList()
        {
            CreditsList.Clear();
            
            if (BaseGameInterface.CheckForComputer(out var computer))
            {
                var creditView = computer.creditsView;

                for(int i = 0; i < MaxPage; i++)
                {
                    var page = "";
                    creditView.SetField("currentPage", i);
                    var lines = creditView.GetScreenText().Split(new string[] { "\n" }, StringSplitOptions.None).ToList();
                    lines.RemoveAt(lines.Count - 1); // remove the extra space
                    lines.RemoveAt(lines.Count - 1); // remove the enter statement

                    page += string.Join("\n", lines);
                    page += "\n";
                    CreditsList.Add(page);
                }
            }
        }

        private void Redraw()
        {
            var str = new StringBuilder();

            str.Repeat("=", SCREEN_WIDTH).AppendLines(2)
                .Append(CreditsList[ScrollLevel])
                .Repeat("=", SCREEN_WIDTH);

            Text = str.ToString();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            switch (key)
            {
                case EKeyboardKey.Enter:
                    ScrollLevel++;
                    if (ScrollLevel == CreditsList.Count) ScrollLevel = 0;
                    Redraw();
                    break;
                case EKeyboardKey.Back:
                    ShowView<GameSettingsView>();
                    break;
            }
        }
    }
}
