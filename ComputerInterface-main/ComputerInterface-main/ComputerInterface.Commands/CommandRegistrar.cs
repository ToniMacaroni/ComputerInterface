﻿using System;
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

                var result = BaseGameInterface.SetName(newName);

                if (result == BaseGameInterface.WordCheckResult.Allowed) return $"Name set to {newName.Replace(" ", "")}";
                else return BaseGameInterface.WordCheckResultToMessage(result);
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

                roomId = roomId.ToUpper();
                var result = BaseGameInterface.JoinRoom(roomId);

                if (result == BaseGameInterface.WordCheckResult.Allowed) return $"Joined {roomId}";
                else return BaseGameInterface.WordCheckResultToMessage(result);
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