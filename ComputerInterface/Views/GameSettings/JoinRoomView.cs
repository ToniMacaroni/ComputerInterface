using System;
using System.Text;

using BepInEx;
using HarmonyLib;

using UnityEngine;

using GorillaNetworking;
using Photon.Pun;

using ComputerInterface.ViewLib;

namespace ComputerInterface.Views.GameSettings
{
    public class JoinRoomView : ComputerView
    {
        private readonly UITextInputHandler _textInputHandler;

        private string _joinedRoom;

        public JoinRoomView()
        {
            _textInputHandler = new UITextInputHandler();

            GameObject callbacks = new GameObject();
            callbacks.name = "RoomCallbacks";
            GameObject.Instantiate(callbacks);
            JoinRoomViewCallbacks calllbacksComponent = callbacks.AddComponent<JoinRoomViewCallbacks>();
            calllbacksComponent.view = this;
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            Redraw();
        }

        public void Redraw()
        {
            var str = new StringBuilder();

            str.Repeat("=", SCREEN_WIDTH).AppendLine();
            str.BeginCenter().Append("Join Room").AppendLine();

            bool showState = true;


            if (GorillaComputer.instance.roomFull)
            {
                str.AppendClr("Room full", "ffffff50").EndAlign().AppendLine();
                showState = false;
            }

            if (GorillaComputer.instance.roomNotAllowed)
            {
                str.AppendClr("Room not allowed", "ffffff50").EndAlign().AppendLine();
                showState = false;
            }



            if (showState)
            {
                switch (GetConnectionState())
                {
                    // Stop being American
                    case PhotonNetworkController.ConnectionState.Initialization:
                        str.AppendClr("Initialisation", "ffffff50").EndAlign().AppendLine();
                        break;
                    case PhotonNetworkController.ConnectionState.WrongVersion:
                        // I doubt anyone is gonna see this but still
                        str.AppendClr("Invalid version", "ffffff50").EndAlign().AppendLine();
                        break;
                    case PhotonNetworkController.ConnectionState.DeterminingPingsAndPlayerCount:
                        str.AppendClr("Connecting", "ffffff50").EndAlign().AppendLine();
                        break;
                    case PhotonNetworkController.ConnectionState.ConnectedAndWaiting:
                        str.AppendClr("Enter to join", "ffffff50").EndAlign().AppendLine();
                        break;
                    case PhotonNetworkController.ConnectionState.DisconnectingFromRoom:
                        str.AppendClr("Leaving room", "ffffff50").EndAlign().AppendLine();
                        break;
                    case PhotonNetworkController.ConnectionState.JoiningFriend:
                    case PhotonNetworkController.ConnectionState.JoiningPublicRoom:
                        str.AppendClr("Joining room", "ffffff50").EndAlign().AppendLine();
                        break;
                    case PhotonNetworkController.ConnectionState.JoiningSpecificRoom:
                        str.AppendClr($"Joining room {_joinedRoom}", "ffffff50").EndAlign().AppendLine();
                        break;
                    case PhotonNetworkController.ConnectionState.InPrivateRoom:
                    case PhotonNetworkController.ConnectionState.InPublicRoom:
                        if (PhotonNetwork.InRoom)
                            str.AppendClr($"In room {PhotonNetwork.CurrentRoom.Name}", "ffffff50").EndAlign().AppendLine();
                        else
                            str.AppendClr($"Error", "ffffff50").EndAlign().AppendLine();
                        break;
                    default:
                        Console.WriteLine("Invalid connection state");
                        ShowView<GameSettingsView>();
                        break;
                }
            }

            str.Repeat("=", SCREEN_WIDTH).AppendLine();
            str.AppendLine();
            str.BeginColor("ffffff50").Append("> ").EndColor().Append(_textInputHandler.Text).AppendClr("_", "ffffff50");

            Text = str.ToString();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_textInputHandler.HandleKey(key))
            {
                if (_textInputHandler.Text.Length > BaseGameInterface.MAX_ROOM_LENGTH)
                {
                    _textInputHandler.Text = _textInputHandler.Text.Substring(0, BaseGameInterface.MAX_ROOM_LENGTH);
                }

                Redraw();
                return;
            }

            switch (key)
            {
                case EKeyboardKey.Back:
                    ShowView<GameSettingsView>();
                    break;
                case EKeyboardKey.Enter:
                    if (!_textInputHandler.Text.IsNullOrWhiteSpace())
                    {
                        _joinedRoom = _textInputHandler.Text.ToUpper();
                        GorillaComputer.instance.roomFull = false;
                        GorillaComputer.instance.roomNotAllowed = false;
                        BaseGameInterface.JoinRoom(_joinedRoom);
                        Redraw();
                    }
                    break;
                case EKeyboardKey.Option1:
                    BaseGameInterface.Disconnect();
                    break;
            }
        }

        // Gets connection state if that wasn't obvious
        private PhotonNetworkController.ConnectionState GetConnectionState()
        {
            return (PhotonNetworkController.ConnectionState)Traverse.Create(PhotonNetworkController.Instance).Field("currentState").GetValue();
        }
    }
}
