using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

namespace ComputerInterface
{
    public class CommandHandler
    {
        private readonly Dictionary<string, Command> _commands = new Dictionary<string, Command>();

        public CommandToken AddCommand(Command command)
        {
            if (_commands.ContainsKey(command.Name))
            {
                throw new CommandAddException(command.Name, "Command already exists");
            }

            if (command.ArgumentTypes != null)
            {
                foreach (var argumentType in command.ArgumentTypes)
                {
                    if(argumentType==null) continue;

                    if (!TomlTypeConverter.CanConvert(argumentType))
                    {
                        throw new CommandAddException(command.Name, $"Type {argumentType.Name} has no converter");
                    }
                }
            }

            _commands.Add(command.Name, command);
            return new CommandToken(this, command.Name, true);
        }

        internal void UnregisterCommand(string name)
        {
            _commands.Remove(name);
            Debug.LogError("unregistered " + name);
        }

        public bool Execute(string commandString, out string messageString)
        {
            commandString = commandString.ToLower();

            messageString = "";

            var commandStrings = commandString.Split(' ');
            if (!_commands.TryGetValue(commandStrings[0], out var command))
            {
                messageString = "Command not found!";
                return false;
            }

            // check if number of arguments is correct
            var argumentCount = commandStrings.Length - 1;
            if (argumentCount != command.ArgumentCount)
            {
                messageString = $"Incorrect arguments!\n{command.ArgumentCount}";
                return false;
            }

            // if there are no arguments passed the the desired argument count is zero
            // execute the command
            if (argumentCount == 0)
            {
                messageString = command.Callback?.Invoke(null);
            }

            // if there are arguments present move them into a new array
            var arguments = new object[argumentCount];
            for (int i = 1; i < argumentCount+1; i++)
            {
                if (command.ArgumentTypes[i-1] == null)
                {
                    arguments[i - 1] = commandStrings[i];
                    continue;
                }

                try
                {
                    arguments[i - 1] = TomlTypeConverter.ConvertToValue(commandStrings[i], command.ArgumentTypes[i - 1]);
                }
                catch
                {
                    messageString = "Incorrect arguments!\nArguments aren't in the correct format.";
                    return false;
                }
                
            }

            messageString = command.Callback?.Invoke(arguments);

            return true;
        }

        public IList<Command> GetAllCommands()
        {
            return _commands.Values.ToList();
        }
    }

    public class Command
    {
        public string Name;
        public Type[] ArgumentTypes;
        public Func<object[], string> Callback;

        public int ArgumentCount => ArgumentTypes?.Length ?? 0;

        public Command(string name, Type[] argumentTypes, Func<object[], string> callback)
        {
            Name = name;
            ArgumentTypes = argumentTypes;
            Callback = callback;
        }
    }

    public class CommandToken
    {
        private readonly CommandHandler _commandHandler;
        private readonly string _name;
        private readonly bool _success;

        private bool _unregistered;

        internal CommandToken(CommandHandler commandHandler, string name, bool success)
        {
            _commandHandler = commandHandler;
            _name = name;
            _success = success;
        }

        public void UnregisterCommand()
        {
            if (!_success || _unregistered) return;

            _unregistered = true;
            _commandHandler.UnregisterCommand(_name);
        }
    }

    public class CommandAddException : Exception
    {
        public CommandAddException(string commandName, string message) : base($"Error adding command {commandName}\n{message}")
        {
        }
    }
}