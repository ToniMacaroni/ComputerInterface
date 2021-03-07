using System.Collections.Generic;
using System.Text;
using ComputerInterface.ViewLib;
using UnityEngine;

namespace ComputerInterface.Views
{
    public class CommandLineHelpView : ComputerView
    {
        private readonly CommandHandler _commandHandler;
        private Dictionary<int, Command[]> _pageCommandsDict;

        private int _commandsPerPage = 7;
        private int _maxPage = 12;

        private int _currentPage;

        public CommandLineHelpView(CommandHandler commandHandler)
        {
            _commandHandler = commandHandler;
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);

            _pageCommandsDict = CreatePages(_commandHandler.GetAllCommands());
            Redraw();
        }

        public void Redraw()
        {
            var str = new StringBuilder();

            DrawHeader(str);
            DrawCommands(str);

            Text = str.ToString();
        }

        public void DrawHeader(StringBuilder str)
        {
            str.Append("<color=#ffffff80><align=\"center\">Page ").Append(_currentPage+1).Append("</align>").AppendLine();
            str.Append("Navigate with left / right arrow key").AppendLine();

            for (int x = 0; x < SCREEN_WIDTH; x++)
            {
                str.Append("=");
            }

            str.Append("</color>").AppendLine();
        }

        public void DrawCommands(StringBuilder str)
        {
            var commands = _pageCommandsDict[_currentPage];

            foreach (var command in commands)
            {
                if (command == null)
                {
                    return;
                }

                str.Append("- ").Append(command.Name);

                if (command.ArgumentTypes != null)
                {
                    foreach (var argType in command.ArgumentTypes)
                    {
                        if(argType == null) continue;
                        str.Append(" <").Append(argType.Name).Append(">");
                    }
                }

                str.AppendLine();
            }
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            switch (key)
            {
                case EKeyboardKey.Left:
                    PrevPage();
                    break;
                case EKeyboardKey.Right:
                    NextPage();
                    break;
                case EKeyboardKey.Back:
                    ReturnView();
                    break;
            }
        }

        private void PrevPage()
        {
            ChangePage(_currentPage - 1);
        }

        private void NextPage()
        {
            ChangePage(_currentPage + 1);
        }

        private void ChangePage(int page)
        {
            _currentPage = page;
            ClampPage();

            Redraw();
        }

        private void ClampPage()
        {
            if (_currentPage < 0)
            {
                _currentPage = 0;
                return;
            }

            if (_currentPage >= _maxPage)
            {
                _currentPage = _maxPage-1;
            }
        }

        private Dictionary<int, Command[]> CreatePages(IList<Command> commandList)
        {
            var dict = new Dictionary<int, Command[]>();

            int numOfCommands = commandList.Count;
            _maxPage = Mathf.CeilToInt((float)numOfCommands / _commandsPerPage);

            for (int pageNum = 0; pageNum < _maxPage; pageNum++)
            {
                var commands = new Command[_commandsPerPage];
                for (int i = 0; i < _commandsPerPage; i++)
                {
                    var commandIdx = (pageNum*_commandsPerPage) + i;

                    if (commandIdx > numOfCommands - 1)
                    {
                        dict.Add(pageNum, commands);
                        return dict;
                    }

                    commands[i] = commandList[commandIdx];
                }
                dict.Add(pageNum, commands);
            }

            return dict;
        }
    }
}