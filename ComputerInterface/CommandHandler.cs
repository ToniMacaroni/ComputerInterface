using System;
using System.Collections.Generic;
using UnityEngine.Windows.WebCam;

namespace ComputerInterface
{
    public class CommandHandler
    {
        private readonly Dictionary<string, Command> _commands = new Dictionary<string, Command>();

        public void AddCommand(Command command)
        {
            if (_commands.ContainsKey(command.Name)) return;
            _commands.Add(command.Name, command);
        }

        public bool Execute(string commandString, out string messageString)
        {
            commandString = commandString.ToLower();

            messageString = "";

            var commandStrings = commandString.Split(' ');
            if (!_commands.TryGetValue(commandStrings[0], out var command))
            {
                messageString = "couldn't find command";
                return false;
            }

            var argumentCount = commandStrings.Length - 1;
            if (argumentCount != command.ArgumentCount)
            {
                messageString = $"wrong argument count\nGot {argumentCount}\nShould be {command.ArgumentCount}";
                return false;
            }

            if (argumentCount == 0)
            {
                messageString = command.Callback?.Invoke(null);
            }

            var arguments = new string[argumentCount];
            for (int i = 1; i < argumentCount+1; i++)
            {
                arguments[i - 1] = commandStrings[i];
            }

            messageString = command.Callback?.Invoke(arguments);

            return true;
        }
    }

    public class Command
    {
        public string Name;
        public int ArgumentCount;
        public Func<string[], string> Callback;

        public Command(string name, int argumentCount, Func<string[], string> callback)
        {
            Name = name;
            ArgumentCount = argumentCount;
            Callback = callback;
        }
    }
}