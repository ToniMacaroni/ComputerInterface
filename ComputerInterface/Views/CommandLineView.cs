using System;
using System.Text;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;

namespace ComputerInterface.Views
{
    public class CommandLineEntry : IComputerModEntry
    {
        public string EntryName => "Commandline";
        public Type EntryViewType => typeof(CommandLineView);
    }

    public class CommandLineView : ComputerView
    {
        private readonly CommandHandler _commandHandler;
        private readonly UITextInputHandler _textInputHandler;

        private string _notification = "";

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
            str.AppendClr("/// MonkeShell ///", "ffffff").AppendLine();
            str.AppendClr("/ Press option 1 for help", "ffffff50").AppendLine();
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
            }
        }

        public void RunCommand()
        {
            _notification = "";
            var success = _commandHandler.Execute(_textInputHandler.Text, out var messageString);

            _notification = messageString;

            if (success)
            {
                _textInputHandler.Text = "";
            }

            Redraw();
        }
    }
}