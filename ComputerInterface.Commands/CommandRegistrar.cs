using System;
using UnityEngine;
using Zenject;
using Photon.Pun;

namespace ComputerInterface.Commands
{
    public class CommandRegistrar : IInitializable
    {
        private CommandHandler _commandHandler;
        private CustomComputer _computer;

        [Inject]
        public void Construct(CommandHandler commandHandler, CustomComputer computer)
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
                return $"Updated color:\n\nR: {r} ({args[0]})\nG: {g} ({args[1]})\nB: {b} ({args[2]})\n";
            }));

            // setname: setname <name>
            _commandHandler.AddCommand(new Command("setname", new Type[] { typeof(string) }, args =>
            {
                var newName = ((string)args[0]).ToUpper();

                var result = BaseGameInterface.SetName(newName);

                if (result == BaseGameInterface.WordCheckResult.Allowed) return $"Updated name: {newName.Replace(" ", "")}";
                else return $"Error: {BaseGameInterface.WordCheckResultToMessage(result)}";
            }));

            // leave: leave
            // disconnects from the current room
            _commandHandler.AddCommand(new Command("leave", null, args =>
            {
                if (PhotonNetwork.InRoom)
                {
                    BaseGameInterface.Disconnect();
                    return "Left room!";
                }
                return "You aren't currently in a room.";
            }));

            // join <roomId>
            // join a private room
            _commandHandler.AddCommand(new Command("join", new Type[] { typeof(string) }, args =>
            {
                var roomId = (string)args[0];

                roomId = roomId.ToUpper();
                var result = BaseGameInterface.JoinRoom(roomId);

                if (result == BaseGameInterface.WordCheckResult.Allowed) return $"Joining room: {roomId}";
                else return $"Error: {BaseGameInterface.WordCheckResultToMessage(result)}";
            }));

            // cam <fp|tp>
            // sets the screen camera to either first or third person
            _commandHandler.AddCommand(new Command("cam", new Type[] { typeof(string) }, args =>
            {
                var cam = GorillaTagger.Instance.thirdPersonCamera.GetComponentInChildren<Camera>();
                if (cam == null) return "Error: Could not find camera";

                var argString = (string)args[0];

                if (argString == "fp" || argString == "tp")
                {
                    cam.enabled = argString == "tp";
                    return $"Updated camera: {(argString == "tp" ? "Third" : "First")} person";
                }

                return "Invalid syntax! Use fp/tp to use the command";
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

                return $"Updated background:\n\nR: {r} ({args[0]})\nG: {g} ({args[1]})\nB: {b} ({args[2]})\n";
            }));
        }

        public void Initialize()
        {
            RegisterCommands();
        }
    }
}