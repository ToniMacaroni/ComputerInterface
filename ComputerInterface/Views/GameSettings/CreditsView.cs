using ComputerInterface.ViewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ComputerInterface.Views.GameSettings
{
    public class CreditsView : ComputerView
    {
        private int MaxPage;
        private int ScrollLevel;
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
                PropertyInfo propInfo = creditView.GetType().GetProperty("TotalPages", BindingFlags.NonPublic | BindingFlags.Instance);
                MaxPage = (int)propInfo.GetValue(creditView);
                for (int i = 0; i < MaxPage; i++)
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

            str.AppendLines(2)
                .Append(CreditsList[ScrollLevel])
                .Append($"<color=#ffffff50><align=\"center\"><  {ScrollLevel + 1}/{CreditsList.Count}  ></align></color>");

            Text = str.ToString();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            switch (key)
            {
                case EKeyboardKey.Left:
                    ScrollLevel--;
                    if (ScrollLevel < 0) ScrollLevel = 0;
                    Redraw();
                    break;
                case EKeyboardKey.Right:
                    ScrollLevel++;
                    if (ScrollLevel >= CreditsList.Count) ScrollLevel = CreditsList.Count - 1;
                    Redraw();
                    break;
                case EKeyboardKey.Back:
                    ShowView<GameSettingsView>();
                    break;
            }
        }
    }
}
