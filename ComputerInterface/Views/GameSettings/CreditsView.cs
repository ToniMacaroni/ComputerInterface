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
        int page;
        GorillaNetworking.CreditsView creditsView;

        int MaxPage => (int)totalPages.GetValue(creditsView);
        PropertyInfo totalPages;

        MethodInfo getPage;

        public override void OnShow(object[] args)
        {
			if (!BaseGameInterface.CheckForComputer(out var computer))
			{
				ShowView<GameSettingsView>();
				return;
			}

            creditsView = computer.creditsView;
            creditsView.pageSize = SCREEN_HEIGHT - 3;
            totalPages = creditsView.GetType().GetProperty("TotalPages", BindingFlags.NonPublic | BindingFlags.Instance);
            getPage = creditsView.GetType().GetMethod("GetPage", BindingFlags.NonPublic | BindingFlags.Instance);

			base.OnShow(args);
            Redraw();
        }

        private void Redraw()
        {
            var str = new StringBuilder();

            str.Append(GetPage(page))
                .Append($"<color=#ffffff50><align=\"center\"><  {page + 1}/{MaxPage}  ></align></color>");

			SetText(str);
        }

        string GetPage(int page)
        {
            var text = getPage.Invoke(creditsView, new object[] { page }) as string;
            var lines = text.Split('\n');
            return string.Join("\n", lines.Take(lines.Length - 2));
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            switch (key)
            {
                case EKeyboardKey.Left:
                    page--;
                    page %= MaxPage;
                    Redraw();
                    break;
                case EKeyboardKey.Right:
                    page++;
                    page %= MaxPage;
                    Redraw();
                    break;
                case EKeyboardKey.Back:
                    ShowView<GameSettingsView>();
                    break;
            }
        }
    }
}
