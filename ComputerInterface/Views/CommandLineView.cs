using System;
using System.Text;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;
using UnityEngine;
using Zenject;

namespace ComputerInterface.Views
{
    public class CommandLineEntry : IComputerModEntry
    {
        public string EntryName => "Commandline";
        public Type EntryViewType => typeof(CommandLineView);
    }

    public class CommandLineView : ComputerView
    {
        [Inject] private readonly CustomComputer _computer = null;

        private readonly CommandHandler _commandHandler;

        private string _currentCommand = "";
        private string _notification = "";

        public CommandLineView(CommandHandler commandHandler)
        {
            _commandHandler = commandHandler;

            // setcolor: setcolor <r> <g> <b>
            _commandHandler.AddCommand(new Command("setcolor", 3, args =>
            {
                var r = float.Parse(args[0]);
                var g = float.Parse(args[1]);
                var b = float.Parse(args[2]);

                if (r > 0) r /= 255;
                if (g > 0) g /= 255;
                if (b > 0) b /= 255;

                BaseGameInterface.SetColor(r, g, b);
                return $"R: {r} ({args[0]})\nG: {g} ({args[1]})\nB: {b} ({args[2]})\n";
            }));

            // setname: setname <name>
            _commandHandler.AddCommand(new Command("setname", 1, args =>
            {
                BaseGameInterface.SetName(args[0]);
                return $"Name set to {args[0]}";
            }));

            // leave: leave
            // disconnects from the current room
            _commandHandler.AddCommand(new Command("leave", 0, args =>
            {
                BaseGameInterface.Disconnect();
                return "Left room";
            }));

            // join <roomId>
            // join a private room
            _commandHandler.AddCommand(new Command("join", 1, args =>
            {
                BaseGameInterface.JoinRoom(args[0]);
                return $"Joined {args[0]}";
            }));

            // cam <fp|tp>
            // sets the screen camera to either first or third person
            _commandHandler.AddCommand(new Command("cam", 1, args =>
            {
                var cam = GameObject.Find("Shoulder Camera")?.GetComponent<Camera>();
                if (cam == null) return "camera not found";

                if (args[0] == "fp")
                {
                    cam.enabled = false;
                    return "Set to first person";
                }

                if(args[0] == "tp")
                {
                    cam.enabled = true;
                    return "Set to third person";
                }

                return "usage: cam <fp|tp>";
            }));

            // setbg <r> <g> <b>
            // sets the background of the screen
            _commandHandler.AddCommand(new Command("setbg", 3, args =>
            {
                var r = float.Parse(args[0]);
                var g = float.Parse(args[1]);
                var b = float.Parse(args[2]);

                if (r > 0) r /= 255;
                if (g > 0) g /= 255;
                if (b > 0) b /= 255;

                _computer.SetBG(r, g, b);

                return "Background set";
            }));
        }

        public override void OnShow()
        {
            base.OnShow();
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
            str.Append("/// CICL ///").AppendLine();
            str.Append("/ Option 1 = Back").AppendLine();
            str.Append("/ Option 3 = Space").AppendLine();
        }

        public void DrawCurrentCommand(StringBuilder str)
        {
            if (!string.IsNullOrEmpty(_notification))
            {
                str.Append("  ").Append(_notification.Replace("\n", "\n  ")).AppendLine();
            }

            str.Append("> ").Append(_currentCommand).AppendLine();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (!key.IsFunctionKey())
            {
                TypeChar(key);
                return;
            }

            switch (key)
            {
                case EKeyboardKey.Enter:
                    RunCommand();
                    break;
                case EKeyboardKey.Delete:
                    DeleteCharacter();
                    break;
                case EKeyboardKey.Option1:
                    ReturnToMainMenu();
                    break;
                case EKeyboardKey.Option3:
                    AddSpace();
                    break;
            }
        }

        public void TypeChar(EKeyboardKey key)
        {
            if (key.TryParseNumber(out var num))
            {
                _currentCommand += num;
                Redraw();
                return;
            }

            _currentCommand += key;
            Redraw();
        }

        public void AddSpace()
        {
            _currentCommand += " ";
            Redraw();
        }

        public void RunCommand()
        {
            _notification = "";
            var success = _commandHandler.Execute(_currentCommand, out var messageString);

            _notification = messageString;

            if (success)
            {
                _currentCommand = "";
            }

            Redraw();
        }

        public void DeleteCharacter()
        {
            if (_currentCommand.Length == 0) return;

            _currentCommand = _currentCommand.Substring(0, _currentCommand.Length - 1);
            Redraw();
        }
    }
}