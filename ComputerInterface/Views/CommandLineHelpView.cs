using System.Collections.Generic;
using System.Text;
using ComputerInterface.ViewLib;
using UnityEngine;

namespace ComputerInterface.Views
{
    public class CommandLineHelpView : ComputerView
    {
        private readonly CommandHandler _commandHandler;
        private readonly UIPageHandler _pageHandler;

        public CommandLineHelpView(CommandHandler commandHandler)
        {
            _commandHandler = commandHandler;
            _pageHandler = new UIPageHandler(EKeyboardKey.Left, EKeyboardKey.Right);
            _pageHandler.EntriesPerPage = 5;
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);

            var commands = _commandHandler.GetAllCommands();
            var lines = new string[commands.Count];

            for (int i = 0; i < lines.Length; i++)
            {
                var command = commands[i];
                if (command == null)
                {
                    lines[i] = "-";
                    continue;
                }

                lines[i] = "- " + command.Name;

                if (command.ArgumentTypes != null)
                {
                    foreach (var argType in command.ArgumentTypes)
                    {
                        if (argType == null) continue;
                        lines[i] += " <" + argType.Name + ">";
                    }
                }
            }
            _pageHandler.SetLines(lines);

            Redraw();
        }

        public void Redraw()
        {
            var str = new StringBuilder();

            DrawHeader(str);
            DrawCommands(str);

            SetText(str);
        }

        public void DrawHeader(StringBuilder str)
        {
            str.Append("<color=#ffffff80><align=\"center\">Page ").Append(_pageHandler.CurrentPage+1).Append("</align>").AppendLine();
            str.Append("Navigate with left / right arrow key").AppendLine();

            for (int x = 0; x < SCREEN_WIDTH; x++)
            {
                str.Append("=");
            }

            str.Append("</color>").AppendLine();
        }

        public void DrawCommands(StringBuilder str)
        {
            var lines = _pageHandler.GetLinesForCurrentPage();
            for (var i = 0; i < lines.Length; i++)
            {
                str.Append(lines[i]);
                str.AppendLine();
            }

            str.AppendLine();
            _pageHandler.AppendFooter(str);
            str.AppendLine();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_pageHandler.HandleKeyPress(key))
            {
                Redraw();
                return;
            }

            switch (key)
            {
                case EKeyboardKey.Back:
                    ReturnView();
                    break;
            }
        }
    }
}