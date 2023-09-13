using System.Text;
using ComputerInterface.ViewLib;
using UnityEngine;

namespace ComputerInterface.Views
{
    public class CommandLineHelpView : ComputerView
    {
        private readonly CommandHandler _commandHandler;
        private readonly UITextPageHandler _pageHandler;

        public CommandLineHelpView(CommandHandler commandHandler)
        {
            _commandHandler = commandHandler;
            _pageHandler = new UITextPageHandler(EKeyboardKey.Left, EKeyboardKey.Right);
            _pageHandler.EntriesPerPage = 8;
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);

            var commands = _commandHandler.GetAllCommands();
            var lines = new string[commands.Count];

            for (int i = 0; i < lines.Length; i++)
            {
                var command = commands[i];

                lines[i] = "- ";

                if (command == null) continue;

                lines[i] += command.Name;

                if (command.ArgumentTypes != null)
                {
                    foreach (var argType in command.ArgumentTypes)
                    {
                        if (argType == null)
                        {
                            lines[i] += " <string>";
                            continue;
                        }

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
            str.BeginColor("ffffff50").Append("== ").EndColor();
            str.Append("Command Line Info").BeginColor("ffffff50").Append(" ==").EndColor().AppendLine();
            str.Append("<size=40>Nativate with the left/right arrow keys</size>").AppendLines(2);
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