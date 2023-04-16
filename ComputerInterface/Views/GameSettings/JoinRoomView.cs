using ComputerInterface.ViewLib;
using GorillaNetworking;
using Photon.Pun;
using System;
using System.Text;
using UnityEngine;

namespace ComputerInterface.Views.GameSettings
{
    public class JoinRoomView : ComputerView
    {
        private readonly UITextInputHandler _textInputHandler;
        private GameObject callbacks;
        private string _joinedRoom;

        public JoinRoomView()
        {
            _textInputHandler = new UITextInputHandler();
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);

            callbacks = new GameObject();
            callbacks.name = "RoomCallbacks";
            UnityEngine.Object.DontDestroyOnLoad(callbacks);

            JoinRoomViewCallbacks calllbacksComponent = callbacks.AddComponent<JoinRoomViewCallbacks>();
            calllbacksComponent.view = this;

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
                    case PhotonNetworkController.ConnectionState.Initialization:
                        str.AppendClr("Initialization", "ffffff50").EndAlign().AppendLine();
                        break;
                    case PhotonNetworkController.ConnectionState.WrongVersion:
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
                        str.AppendClr("Joining group", "ffffff50").EndAlign().AppendLine();
                        break;
                    case PhotonNetworkController.ConnectionState.JoiningPublicRoom:
                        str.AppendClr("Joining room", "ffffff50").EndAlign().AppendLine();
                        break;
                    case PhotonNetworkController.ConnectionState.JoiningSpecificRoom:
                        str.AppendClr($"Joining room {_joinedRoom}", "ffffff50").EndAlign().AppendLine();
                        break;
                    case PhotonNetworkController.ConnectionState.InPrivateRoom:
                        str.AppendClr($"In room {(PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.Name : "")}", "ffffff50").EndAlign().AppendLine(); ;
                        break;
                    case PhotonNetworkController.ConnectionState.InPublicRoom:
                        str.AppendClr($"In room {(PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.Name : "")}", "ffffff50").EndAlign().AppendLine();
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
            switch (key)
            {
                case EKeyboardKey.Back:
                    UnityEngine.Object.Destroy(callbacks);
                    ShowView<GameSettingsView>();
                    break;
                case EKeyboardKey.Enter:
                    _joinedRoom = _textInputHandler.Text.ToUpper();
                    GorillaComputer.instance.roomFull = false;
                    GorillaComputer.instance.roomNotAllowed = false;
                    BaseGameInterface.JoinRoom(_joinedRoom, out _, out _);
                    Redraw();
                    break;
                case EKeyboardKey.Option1:
                    BaseGameInterface.Disconnect();
                    break;
                default:
                    if (_textInputHandler.HandleKey(key))
                    {
                        if (_textInputHandler.Text.Length > BaseGameInterface.MAX_ROOM_LENGTH)
                        {
                            _textInputHandler.Text = _textInputHandler.Text.Substring(0, BaseGameInterface.MAX_ROOM_LENGTH);
                        }

                        Redraw();
                        return;
                    }
                    break;
            }
        }

        // Gets connection state if that wasn't obvious
        private PhotonNetworkController.ConnectionState GetConnectionState()
        {
            return PhotonNetworkController.Instance.GetField<PhotonNetworkController.ConnectionState>("currentState");
        }
    }
}
