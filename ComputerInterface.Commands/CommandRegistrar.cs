using System;
using BepInEx;
using UnityEngine;
using Zenject;

namespace ComputerInterface.Commands
{
    internal class CommandRegistrar : IInitializable
    {
        private readonly CommandHandler _commandHandler;
        private readonly CustomComputer _computer;

        public CommandRegistrar(CommandHandler commandHandler, CustomComputer computer)
        {
            _commandHandler = commandHandler;
            _computer = computer;
        }

        public void RegisterCommands()
        {
            // setcolor: setcolor <r> <g> <b>
            _commandHandler.AddCommand(new Command("setcolor", new[] { typeof(float), typeof(float), typeof(float) }, args =>
            {
                var r = (float)args[0];
                var g = (float)args[1];
                var b = (float)args[2];

                if (r > 0) r /= 255;
                if (g > 0) g /= 255;
                if (b > 0) b /= 255;

                BaseGameInterface.SetColor(r, g, b);
                return $"R: {r} ({args[0]})\nG: {g} ({args[1]})\nB: {b} ({args[2]})\n";
            }));

            // setname: setname <name>
            _commandHandler.AddCommand(new Command("setname", new Type[] { null }, args =>
            {
                var newName = ((string)args[0]).ToUpper();

                if (newName.Length > BaseGameInterface.MAX_NAME_LENGTH)
                {
                    return "Name too long";
                }

                BaseGameInterface.SetName(newName);
                return $"Name set to {newName}";
            }));

            // leave: leave
            // disconnects from the current room
            _commandHandler.AddCommand(new Command("leave", null, args =>
            {
                BaseGameInterface.Disconnect();
                return "Left room";
            }));

            // join <roomId>
            // join a private room
            _commandHandler.AddCommand(new Command("join", new Type[] { null }, args =>
            {
                var roomId = (string)args[0];

                if (roomId.IsNullOrWhiteSpace())
                {
                    return "Invalid room";
                }

                if (roomId.Length > BaseGameInterface.MAX_ROOM_LENGTH)
                {
                    return "Room too long";
                }

                roomId = roomId.ToUpper();
                BaseGameInterface.JoinRoom(roomId);

                return $"Joined {args[0]}";
            }));

            // cam <fp|tp>
            // sets the screen camera to either first or third person
            _commandHandler.AddCommand(new Command("cam", new Type[] { null }, args =>
            {
                var cam = GameObject.Find("Shoulder Camera")?.GetComponent<Camera>();
                if (cam == null) return "camera not found";

                var argString = (string)args[0];

                if (argString == "fp")
                {
                    cam.enabled = false;
                    return "Set to first person";
                }

                if (argString == "tp")
                {
                    cam.enabled = true;
                    return "Set to third person";
                }

                return "usage: cam <fp|tp>";
            }));

            // setbg <r> <g> <b>
            // sets the background of the screen
            _commandHandler.AddCommand(new Command("setbg", new[] { typeof(float), typeof(float), typeof(float) }, args =>
            {
                var r = (float)args[0];
                var g = (float)args[1];
                var b = (float)args[2];

                if (r > 0) r /= 255;
                if (g > 0) g /= 255;
                if (b > 0) b /= 255;

                _computer.SetBG(r, g, b);

                return "Background set";
            }));
        }

        public void Initialize()
        {
            RegisterCommands();
        }
    }
}