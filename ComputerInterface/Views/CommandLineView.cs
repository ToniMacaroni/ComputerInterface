using System;
using System.Text;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;

namespace ComputerInterface.Views
{
    public class CommandLineEntry : IComputerModEntry
    {
        public string EntryName => "Command Line";
        public Type EntryViewType => typeof(CommandLineView);
    }

    public class CommandLineView : ComputerView
    {
        private readonly CommandHandler _commandHandler;
        private readonly UITextInputHandler _textInputHandler;

        private string _notification = "";
        private string _previousCommand = "";

        public CommandLineView(CommandHandler commandHandler)
        {
            _commandHandler = commandHandler;
            _textInputHandler = new UITextInputHandler();
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            Redraw();
        }

        public void Redraw()
        {
            var builder = new StringBuilder();
            DrawHeader(builder);
            DrawCurrentCommand(builder);

            Text = builder.ToString();
        }

        public void DrawHeader(StringBuilder str)
        {
            str.BeginColor("ffffff50").Append("== ").EndColor();
            str.Append("Command Line").BeginColor("ffffff50").Append(" ==").EndColor().AppendLine();
            str.Append("<size=40>Press Option 1 to view command list</size>").AppendLines(2);
        }

        public void DrawCurrentCommand(StringBuilder str)
        {
            if (!string.IsNullOrEmpty(_notification))
            {
                str.Append("  <color=#ffffff60>").Append(_notification.Replace("\n", "\n  ")).Append("</color>").AppendLine();
            }

            str.AppendClr(">", "ffffff60").Append(_textInputHandler.Text).AppendClr("_", "ffffff60").AppendLine();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_textInputHandler.HandleKey(key))
            {
                Redraw();
                return;
            }

            switch (key)
            {
                case EKeyboardKey.Enter:
                    RunCommand();
                    break;
                case EKeyboardKey.Back:
                    ReturnToMainMenu();
                    break;
                case EKeyboardKey.Option1:
                    ShowView<CommandLineHelpView>();
                    break;
                case EKeyboardKey.Up:
                    if (_previousCommand == "") return;
                    _textInputHandler.Text = _previousCommand;
                    _previousCommand = "";
                    Redraw();
                    break;
                default:
                    _previousCommand = "";
                    break;
            }
        }

        public void RunCommand()
        {
            _notification = "";
            var success = _commandHandler.Execute(_textInputHandler.Text, out var messageString);

            _notification = messageString;

            _previousCommand = "";
            if (success)
            {
                _previousCommand = _textInputHandler.Text;
                _textInputHandler.Text = "";
            }

            Redraw();
        }
    }
}